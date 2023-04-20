using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace DayZLauncher.UnixPatcher.Utils;

// Ignore warnings about unused method arguments
#pragma warning disable RCS1163, IDE0060

/// <summary>
/// Contains methods replacing DayZLauncher's Utils.IO.Junctions
/// </summary>
public static class UnixJunctions
{
    private static bool IsRunningOnMono => Type.GetType("Mono.Runtime") != null;

    static UnixJunctions()
    {
        if (IsRunningOnMono)
        {
            Console.WriteLine("UnixJunctions: running on Mono runtime!");
        }
    }

    public static void Create(string junctionPoint, string targetDir, bool overwrite)
    {
        targetDir = Path.GetFullPath(targetDir);

        if (Directory.Exists(junctionPoint))
        {
            Delete(junctionPoint);
        }

        junctionPoint = ToUnixPath(junctionPoint);
        targetDir = ToUnixPath(targetDir);

        RunShellCommand("ln", $"-s -T \"{targetDir}\" \"{junctionPoint}\"");
    }

    public static void Delete(string junctionPoint)
    {
        if (!Directory.Exists(junctionPoint))
        {
            if (File.Exists(junctionPoint))
            {
                throw new IOException("UnixJunctions: Path is not a junction point");
            }
            return;
        }

        if (Directory.Exists(junctionPoint))
        {
            junctionPoint = ToUnixPath(junctionPoint);
            RunShellCommand("rm", $"-r \"{junctionPoint}\"");
        }
    }

    public static bool Exists(string path)
    {
        if (!Directory.Exists(path))
        {
            return false;
        }

        try
        {
            path = ToUnixPath(path);
            string output = RunShellCommand("ls", $"-la \"{path}\"");
            return output.Contains("->");
        }
        catch
        {
            return false;
        }
    }

    public static string GetTarget(string junctionPoint)
    {
        junctionPoint = ToUnixPath(junctionPoint);
        string output = RunShellCommand("readlink", $"\"{junctionPoint}\"");
        return output.Trim();
    }

    private static string RunShellCommand(string command, string arguments)
    {
        Console.WriteLine("UnixJunctions.RunShellCommand: command= " + command + " ;arguments= " + arguments);

        var gameLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        var basePath = gameLocation + @"\!Linux";
        Directory.CreateDirectory(basePath);

        string uniqueId = Guid.NewGuid().ToString("N");
        string tempOutputPath = basePath + @$"\tmp_output_{uniqueId}.txt";
        string lockFilePath = basePath + @$"\{uniqueId}.lock";

        Console.WriteLine($"UnixJunctions.RunShellCommand: tempOutputPath='{tempOutputPath}'");

        var script = $"""
        #!/bin/sh
        touch "{ToUnixPath(lockFilePath)}"
        {command} {arguments} > "{ToUnixPath(tempOutputPath)}"
        rm "{ToUnixPath(lockFilePath)}"
        """;

        // Execute the shell script
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/C start /unix /bin/sh -c \"{script}\"",
            RedirectStandardOutput = false,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        Console.WriteLine("UnixJunctions.RunShellCommand: about to execute script " + uniqueId);

        using (Process process = new() { StartInfo = processStartInfo })
        {
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                Console.WriteLine($"UnixJunctions: Error executing script '{uniqueId}'. Exit code: {process.ExitCode}");

                if (File.Exists(lockFilePath))
                {
                    Console.WriteLine($"UnixJunctions: Cleaning up {lockFilePath}");
                    File.Delete(lockFilePath);
                    return string.Empty;
                }
            }
        }

        while (!File.Exists(tempOutputPath))
        {
            Console.WriteLine("UnixJunctions.RunShellCommand: waiting for output file " + uniqueId);
            Thread.Sleep(50);
        }

        while (File.Exists(lockFilePath))
        {
            Console.WriteLine("UnixJunctions.RunShellCommand: waiting for unix write unlock " + uniqueId);
            Thread.Sleep(50);
        }

        // Read the output file
        string scriptOutput = File.ReadAllText(tempOutputPath);
        Console.WriteLine($"UnixJunctions.RunShellCommand: {uniqueId} output= {scriptOutput}");
        File.Delete(tempOutputPath);

        return scriptOutput;
    }

    private static string ToUnixPath(string windowsPath)
    {
        return windowsPath.Replace("Z:", string.Empty).Replace("\\", "/");
    }
}