using System;
using System.Runtime.InteropServices.JavaScript;

Console.WriteLine("Hello, Console!");

return 0;

public partial class MyClass
{
    [JSExport]
    internal static string Greeting()
    {
        var text = $"Hello, World! Greetings from node version: {GetNodeVersion()}";
        return text;
    }

    [JSImport("node.process.version")]
    internal static partial string GetNodeVersion();

    [JSExport]
    [return: JSMarshalAs<JSType.Function<JSType.String>>]
    internal static Func<string> GreetingFn()
    {
        var text = $"Hello, World! Greetings from node version: {GetNodeVersion()}";
        return () => text;
    }

    [JSExport]
    internal static void DoFunc([JSMarshalAs<JSType.Function<JSType.String>>] Action<string> jsFunc)
    {
        jsFunc.Invoke("Is C#");
    }
}
