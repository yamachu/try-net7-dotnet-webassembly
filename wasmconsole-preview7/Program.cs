using System;
using System.Runtime.InteropServices.JavaScript;

Console.WriteLine("Hello, Console!");

public static partial class SampleClass
{
    [JSExport]
    internal static string CallFromJS()
    {
        return $"Hello, World! {GetNodeVersion()}";
    }

    [JSExport]
    internal static int Add42(int number)
    {
        return number + 42;
    }

    [JSImport("node.process.version")]
    internal static partial string GetNodeVersion();
}
