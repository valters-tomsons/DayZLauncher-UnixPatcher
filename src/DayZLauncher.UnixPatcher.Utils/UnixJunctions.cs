﻿using System;
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

    private static readonly string GamePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
    private static readonly string LinuxLauncherDataPath = GamePath + @"\LinuxLauncherData";

    private static readonly bool EnableDebugLogging = Environment.GetEnvironmentVariable("DAYZLAUNCHER_UNIX_LOGS") is not null;
    private static readonly string DebugLogFilePath = LinuxLauncherDataPath + @"\launcher.log";

    static UnixJunctions()
    {
        if (IsRunningOnMono)
        {
            Log("UnixJunctions: running on Mono runtime!");
            Directory.CreateDirectory(LinuxLauncherDataPath);
        }
    }

    public static void Create(string junctionPoint, string targetDir, bool overwrite)
    {
        Log("UnixJunctions: Create() called junctionPoint='" + junctionPoint + "' ; targetDir='" + targetDir + "'");

        targetDir = Path.GetFullPath(targetDir);

        if (Directory.Exists(junctionPoint) || File.Exists(junctionPoint))
        {
            Delete(junctionPoint);
        }

        junctionPoint = ToUnixPath(junctionPoint);
        targetDir = ToUnixPath(targetDir);

        RunShellCommand("ln", $"-s -T '{targetDir}' '{junctionPoint}'");
    }

    public static void Delete(string junctionPoint)
    {
        Log("UnixJunctions: Delete() called junctionPoint='" + junctionPoint + "'");

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
            RunShellCommand("rm", $"-r '{junctionPoint}'");
        }
    }

    public static bool Exists(string path)
    {
        Log("UnixJunctions: Exists() called path='" + path + "'");

        if (!Directory.Exists(path))
        {
            return false;
        }

        try
        {
            path = ToUnixPath(path);
            string output = RunShellCommand("ls", $"-la '{path}'");
            return output.Contains("->");
        }
        catch
        {
            return false;
        }
    }

    public static string GetTarget(string junctionPoint)
    {
        Log("UnixJunctions: GetTarget() called junctionPoint='" + junctionPoint + "'");

        junctionPoint = ToUnixPath(junctionPoint);
        string output = RunShellCommand("readlink", $"'{junctionPoint}'");
        return output.Trim();
    }

    private static string RunShellCommand(string command, string arguments)
    {
        Log("UnixJunctions.RunShellCommand: command= " + command + " ;arguments= " + arguments);

        string uniqueId = Guid.NewGuid().ToString("N");
        string tempOutputPath = LinuxLauncherDataPath + @$"\tmp_output_{uniqueId}.txt";
        string lockFilePath = LinuxLauncherDataPath + @$"\{uniqueId}.lock";

        Log($"UnixJunctions.RunShellCommand: tempOutputPath='{tempOutputPath}'");

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

        Log("UnixJunctions.RunShellCommand: about to execute script " + uniqueId);

        using (Process process = new() { StartInfo = processStartInfo })
        {
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                Log($"UnixJunctions: Error executing script '{uniqueId}'. Exit code: {process.ExitCode}");
            }
        }

        while (!File.Exists(tempOutputPath))
        {
            Log("UnixJunctions.RunShellCommand: waiting for output file " + uniqueId);
            Thread.Sleep(50);
        }

        while (File.Exists(lockFilePath))
        {
            Log("UnixJunctions.RunShellCommand: waiting for unix write unlock " + uniqueId);
            Thread.Sleep(50);
        }

        // Read the output file
        string scriptOutput = File.ReadAllText(tempOutputPath);
        Log($"UnixJunctions.RunShellCommand: {uniqueId} output= {scriptOutput}");
        File.Delete(tempOutputPath);

        return scriptOutput;
    }

    private static string ToUnixPath(string windowsPath)
    {
        // Skip drive letter (e.g. 'Z:')
        var result = windowsPath.Substring(2).Replace("\\", "/");
        Log($"UnixJunctions.ToUnixPath: windowsPath='{windowsPath}', result='{result}'");
        return EscapeSymbols(result);
    }

    private static readonly char[] BadSymbols = new char[] { '\'', '"', ';', ',', '\n', '\r', '$', '`', '&', '|', '<', '>' };
    private static string EscapeSymbols(string path)
    {
        var buffer = path.Split(BadSymbols, StringSplitOptions.RemoveEmptyEntries);
        var result = string.Join(string.Empty, buffer);
        Log($"UnixJunctions.EscapeSymbols: path='{path}', result='{result}'");
        return result;
    }

    private static void Log(string message)
    {
        if (EnableDebugLogging)
        {
            File.AppendAllText(DebugLogFilePath, $"{DateTime.Now}: {message}{Environment.NewLine}");
        }
    }
}