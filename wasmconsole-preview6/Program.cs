using System;

Console.WriteLine("Hello, Console!");

public static partial class SampleClass
{
    public static string CallFromJS()
    {
        return "Hello, World!";
    }

    public static int Add42(int number)
    {
        return number + 42;
    }
}
