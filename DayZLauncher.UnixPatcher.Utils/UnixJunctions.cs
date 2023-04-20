using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace DayZLauncher.UnixPatcher.Utils;

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
        var basePath = gameLocation + @$"\!Linux";
        Directory.CreateDirectory(basePath);

        string uniqueId = Guid.NewGuid().ToString("N");
        string tempScriptPath = basePath + @$"\tmp_script_{uniqueId}.sh";
        string tempOutputPath = basePath + @$"\tmp_output_{uniqueId}.txt";
        string lockFilePath = basePath + @$"\{uniqueId}.lock";

        Console.WriteLine("UnixJunctions.RunShellCommand: tempScriptPath= " + tempScriptPath + " ;tempOutputPath= " + tempOutputPath);

        // Write the shell script
        var script = @$"#!/bin/sh
touch ""{ToUnixPath(lockFilePath)}""
{command} {arguments} > ""{ToUnixPath(tempOutputPath)}""
rm ""{ToUnixPath(lockFilePath)}""

";

        File.WriteAllText(tempScriptPath, script.Replace("\r\n", "\n"));

        Console.WriteLine("UnixJunctions.RunShellCommand: shell script generated " + uniqueId);

        // Set executable permission on the script
        var chmodStartInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/C start /unix /usr/bin/chmod +x \"{ToUnixPath(tempScriptPath)}\"",
            RedirectStandardOutput = false,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        Console.WriteLine("UnixJunctions.RunShellCommand: about to chmod " + uniqueId + tempScriptPath);

        using (Process chmodProcess = new Process { StartInfo = chmodStartInfo })
        {
            chmodProcess.Start();
            chmodProcess.WaitForExit();

            if (chmodProcess.ExitCode != 0)
            {
                throw new Exception($"UnixJunctions.RunShellCommand: Error setting executable permission on '{tempScriptPath}'. Exit code: {chmodProcess.ExitCode}");
            }
            else
            {
                Console.WriteLine("UnixJunctions.RunShellCommand: chmod exited: " + chmodProcess.ExitCode);
            }
        }

        Console.WriteLine("UnixJunctions.RunShellCommand: chmod finished for " + uniqueId);

        // Execute the shell script
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/C start /unix /usr/bin/sh -c \"{ToUnixPath(tempScriptPath)}\"",
            RedirectStandardOutput = false,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        Console.WriteLine("UnixJunctions.RunShellCommand: about to launch script " + uniqueId);

        using (Process process = new Process { StartInfo = processStartInfo })
        {
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception($"UnixJunctions: Error running shell script '{tempScriptPath}'. Exit code: {process.ExitCode}");
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
        string output = File.ReadAllText(tempOutputPath);
        Console.WriteLine("UnixJunctions.RunShellCommand: output= " + output);

        File.Delete(tempOutputPath);
        File.Delete(tempScriptPath);

        return output;
    }

    private static string ToUnixPath(string windowsPath)
    {
        return windowsPath.Replace("Z:", string.Empty).Replace("\\", "/");
    }
}