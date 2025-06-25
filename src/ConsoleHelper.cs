namespace PeterRosser.GitTools.FindLargeFiles;

using System;

internal static class ConsoleHelper
{
    public static void PrintProgress(string message, bool clearLine = true)
    {
        if (clearLine)
        {
            Console.Error.Write("\r" + new string(' ', Console.WindowWidth - 1) + "\r");
        }

        Console.Error.Write(message.Length > Console.WindowWidth - 1 ? message[0..(Console.WindowWidth - 4)] + "..." : message);
        Console.Error.Flush();
    }
}