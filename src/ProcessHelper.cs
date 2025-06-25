namespace PeterRosser.GitTools.FindLargeFiles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

internal static class ProcessHelper
{
    public static List<string>? RunGitCommand(string[] args, string[]? input = null)
    {
        try
        {
            Lock ioLock = new();
            Process process = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    RedirectStandardOutput = true,
                    RedirectStandardError = false,
                    RedirectStandardInput = input is not null,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardInputEncoding = input is not null ? Encoding.UTF8 : null,
                    StandardOutputEncoding = Encoding.UTF8,
                },
            };

            foreach (string arg in args)
            {
                process.StartInfo.ArgumentList.Add(arg);
            }

            var outputLines = new List<string>();

            // Set up the event handler for stdout
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data is not null)
                {
                    lock (ioLock) // Thread-safe access to the list
                    {
                        outputLines.Add(e.Data);
                    }
                }
            };

            if (!process.Start())
            {
                throw new InvalidOperationException("Failed to start git process. Ensure git is installed and available in PATH.");
            }

            // Begin asynchronous reading of stdout
            process.BeginOutputReadLine();

            // If we have input to send, write it to stdin
            if (input is not null)
            {
                process.StandardInput.AutoFlush = true; // Ensure input is flushed immediately
                process.StandardInput.Write(string.Join(Environment.NewLine, input));
                process.StandardInput.Close(); // Close stdin to signal end of input
            }

            // Wait for process to exit and output reading to complete
            process.WaitForExit();
            process.CancelOutputRead(); // Stop reading output

            // Dispose stdin after process exits (if used)
            if (input != null)
            {
                process.StandardInput.Dispose();
            }

            return process.ExitCode != 0
                ? throw new InvalidOperationException($"Git command failed with exit code {process.ExitCode}")
                : outputLines;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error executing git command: {ex.Message}");
            return null;
        }
    }
}
