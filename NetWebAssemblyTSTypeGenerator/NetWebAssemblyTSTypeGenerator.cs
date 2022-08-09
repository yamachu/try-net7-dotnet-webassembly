using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NetWebAssemblyTSTypeGenerator
{
    [Generator]
    public class NetWebAssemblyTSTypeGenerator : ISourceGenerator
    {
        readonly string JSImportAttribute = "JSImport";
        readonly string JSExportAttribute = "JSExport";

        public void Execute(GeneratorExecutionContext context)
        {
            context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.AssemblyName", out var assemblyName);
            context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.RootNamespace", out var rootNamespace);
            var asmName = assemblyName ?? rootNamespace;

            var nodes = context.Compilation.SyntaxTrees.SelectMany(v => v.GetRoot().DescendantNodes());
            var classes = nodes
                .Where(n => n.IsKind(SyntaxKind.ClassDeclaration))
                .OfType<ClassDeclarationSyntax>();

            var attributeAttachedMethods = classes.SelectMany(
                c => c.Members.Where(m => HasAttribute(m, JSImportAttribute) || HasAttribute(m, JSExportAttribute))
            ).Where(m => m is MethodDeclarationSyntax)
            .Cast<MethodDeclarationSyntax>()
            .Aggregate((Imports: new List<MethodDeclarationSyntax>(), Exports: new List<MethodDeclarationSyntax>()), (prev, curr) =>
            {
                if (HasAttribute(curr, JSImportAttribute))
                {
                    return (new List<MethodDeclarationSyntax>(prev.Imports)
                    {
                        curr
                    }, prev.Exports);
                }
                if (HasAttribute(curr, JSExportAttribute))
                {
                    return (prev.Imports, new List<MethodDeclarationSyntax>(prev.Exports)
                    {
                        curr
                    });
                }

                throw new Exception();
            });

            var binds = new Binds()
            {
                // see: https://github.com/dotnet/runtime/blob/main/src/libraries/System.Runtime.InteropServices.JavaScript/src/System/Runtime/InteropServices/JavaScript/JSImportAttribute.cs
                Imports = new List<MethodDefinition>(), // TODO: Parse JSImportAttribute... functionName and moduleName...
                Exports = attributeAttachedMethods.Exports.Select(m =>
                {
                    var fullName = string.Join(".", DigParentName(m).AsEnumerable().Reverse()) + "." + m.Identifier.ToString();
                    var args = m.ParameterList.Parameters.Select(p => p.Identifier.ToString()).Select(n => new Argument { Name = n }).ToImmutableArray();
                    return new MethodDefinition
                    {
                        FullName = fullName,
                        Arguments = args
                    };
                })
            };

            var exportSymbolDict = binds.Exports.Aggregate(new Dictionary<string, dynamic>(), (prev, curr) =>
            {
                return SplitIntoDictionary(prev, (curr.FullName, curr));
            });

            var serializeOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters =
                {
                    new DynamicDictConverter(),
                    new ArgumentsJsonConverter()
                }
            };

            var jsonString = JsonSerializer.Serialize(exportSymbolDict, serializeOptions);

            var _template_ = $$""""
            import "@microsoft/dotnet-runtime"

            declare module "@microsoft/dotnet-runtime" {
                type ExportHelper = (assemblyName: "{{asmName + ".dll"}}") => Promise<
                        {{jsonString}}
                >;
            };
            """";
            context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.JSPortOverrideTypeDefinitionOutputDir", out var jsPortOverrideTypeDefinitionOutputDir);
            File.WriteAllText(Path.Combine(jsPortOverrideTypeDefinitionOutputDir, $"dotnet.{asmName}.override.d.ts"), _template_);
        }

        private IList<string> DigParentName(SyntaxNode decl, List<string> parentNames = null)
        {
            if (parentNames == null)
            {
                parentNames = new List<string>();
            }

            if (decl == null)
            {
                return parentNames;
            }

            var next = new List<string>(parentNames);
            switch (decl)
            {
                case ClassDeclarationSyntax classDecl:
                    next.Add(classDecl.Identifier.ToString());
                    break;
                case NamespaceDeclarationSyntax namespaceDecl:
                    next.Add(namespaceDecl.Name.ToString());
                    break;
                default:
                    break;
            }
            return DigParentName(decl.Parent, next);
        }

        private Dictionary<string, dynamic> SplitIntoDictionary(Dictionary<string, dynamic> baseDict, (string MaybeKey, MethodDefinition Definition) defs)
        {
            var (maybeKey, definition) = defs;
            var dotSplitted = maybeKey.Split('.');
            if (dotSplitted.Length == 1)
            {
                if (baseDict.TryGetValue(maybeKey, out var _))
                {
                    throw new Exception("duplicate entry");
                }
                else
                {
                    var next = new Dictionary<string, dynamic>(baseDict)
                    {
                        { maybeKey, definition.Arguments }
                    };
                    return next;
                }
            }
            var key = string.Join("", dotSplitted.Take(1));
            var nextKey = string.Join(".", dotSplitted.Skip(1));

            if (baseDict.TryGetValue(key, out var maybeDictionary))
            {
                if (maybeDictionary is Dictionary<string, dynamic> _dict)
                {
                    return new Dictionary<string, dynamic>(baseDict)
                    {
                        [key] = SplitIntoDictionary(_dict, (nextKey, definition))
                    };
                }
                else
                {
                    throw new Exception("value must Dictionary<string, dynamic>");
                }
            }
            else
            {
                return new Dictionary<string, dynamic>(baseDict)
                {
                    { key, SplitIntoDictionary(new Dictionary<string, dynamic>(), (nextKey, definition)) }
                };
            }
        }

        private bool HasAttribute(MemberDeclarationSyntax node, string attributeName)
        {
            return node.AttributeLists
                        .SelectMany(a => a.Attributes)
                        .Any(attr => attr.Name.ToString() == attributeName);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization required for this one
        }
    }

    // inspired: https://github.com/joseftw/JOS.STJ.DictionaryStringObjectJsonConverter
    internal class DynamicDictConverter : JsonConverter<Dictionary<string, dynamic>>
    {
        public override Dictionary<string, dynamic> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(
            Utf8JsonWriter writer,
            Dictionary<string, dynamic> argumentValues,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (var key in argumentValues.Keys)
            {
                dynamic v = argumentValues[key];
                HandleValue(writer, key, v, options);
            }
            writer.WriteEndObject();
        }

        private static void HandleValue(Utf8JsonWriter writer, string key, dynamic value, JsonSerializerOptions options)
        {
            if (key != null)
            {
                writer.WritePropertyName(key);
            }

            switch (value)
            {
                case string stringValue:
                    writer.WriteStringValue(stringValue);
                    break;
                case Dictionary<string, dynamic> dictionaryValue:
                    writer.WriteStartObject();
                    foreach (var i in dictionaryValue)
                    {
                        HandleValue(writer, i.Key, i.Value, options);
                    }
                    writer.WriteEndObject();
                    break;
                case IEnumerable<Argument> argumentsValue:
                    JsonSerializer.Serialize(writer, argumentsValue, options);
                    break;
                default:
                    break;
            }
        }
    }

    internal class ArgumentsJsonConverter : JsonConverter<IEnumerable<Argument>>
    {
        public override IEnumerable<Argument> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(
            Utf8JsonWriter writer,
            IEnumerable<Argument> argumentValues,
            JsonSerializerOptions options) =>
                writer.WriteRawValue($"""({string.Join(", ", argumentValues.Select(a => a.Name + ": any /* TODO */"))}) => any /* TODO */""", true);
    }

    internal struct Binds
    {
        internal IEnumerable<MethodDefinition> Imports;
        internal IEnumerable<MethodDefinition> Exports;
    }

    internal struct MethodDefinition
    {
        internal string FullName;
        internal IEnumerable<Argument> Arguments;
        // TODO: ReturnType
    }

    internal struct Argument
    {
        internal string Name;
        // TODO: Type
    }
}