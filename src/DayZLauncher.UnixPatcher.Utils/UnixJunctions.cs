using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;


namespace DayZLauncher.UnixPatcher.Utils;

// Ignore warnings about unused method arguments
#pragma warning disable RCS1163, IDE0060

/// <summary>
/// Contains methods replacing DayZLauncher's Utils.IO.Junctions
/// </summary>
public static class UnixJunctions
{
    private static bool IsRunningOnMono => Type.GetType("Mono.Runtime") != null;
    private static string steamFolder;
    private static string steamDrive;
    private static string steamPath;
    private static string appFolder;
    private static string absolutePath;
    
    static UnixJunctions()
    {
        if (IsRunningOnMono)
        {
            Console.WriteLine("UnixJunctions: running on Mono runtime!");
        }
        try
        {
            List<string> LibraryFolders()
            {
                string wineFolder = @"C:\Program Files (x86)\Steam\steamapps";
                string wineFile = wineFolder + @"\libraryfolders.vdf";
                Regex wineRegex = new Regex("[A-Z]:\\\\.[A-z+.]*");
                Console.WriteLine("UnixJunctions.LibraryFolders: Searching for Linux Steam installation...");
                using (StreamReader reader = new StreamReader(wineFile))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        Match match = wineRegex.Match(line);
                        if (match.Success)
                        {
                            steamPath = Regex.Unescape(match.Value).Replace(@"\\", @"\");
                            Console.WriteLine($"UnixJunctions.LibraryFolders: Found Linux Steam installation folder in: {steamPath}");
                            break;
                        }
                        else
                        {
                            Console.WriteLine($"UnixJunctions.LibraryFolders: Error finding Linux Steam installation! Patch may not work correctly!");
                        }
                    }
                }

                List<string> libraryFolders = new List<string>();
                string libraryFile = steamPath + @"\steamapps\libraryfolders.vdf";
                Regex libraryRegex = new Regex(@"path\x22\s*\x22([A-Za-z0-9+\x2F+\x2D+\x2E]*)\x22");
                Console.WriteLine("UnixJunctions.LibraryFolders: Searching for library folders...");
                using (StreamReader reader = new StreamReader(libraryFile))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        Match match = libraryRegex.Match(line);
                        if (match.Success)
                        {
                            string matchBuild = Regex.Unescape(match.Groups[1].Value) + @"/steamapps/common";
                            steamFolder = "Z:" + matchBuild.Replace(@"/", @"\");
                            libraryFolders.Add(steamFolder);
                            Console.WriteLine($"UnixJunctions.LibraryFolders: Found steam library folder at: {steamFolder}");
                        }
                        else
                        {
                            Console.WriteLine($"UnixJunctions.LibraryFolders: Error finding library folders! Patch may not work correctly!");
                        }
                    }
                }
                return libraryFolders;
            }
            
            string AppFolder()
            {
                var appFolders = LibraryFolders().Select(x => x);
                Console.WriteLine("UnixJunctions.AppFolder: Searching the library for DayZ");
                foreach (var folder in appFolders)
                {
                    try
                    {
                        Console.Write($"UnixJunctions.AppFolder: Searching for DayZ in {folder}...");
                        var matches = Directory.GetDirectories(folder, "DayZ");
                        if (matches.Length >= 1)
                        {
                            appFolder = matches[0];
                            Console.WriteLine($"UnixJunctions.AppFolder: Found app folder: {appFolder}");
                            return appFolder;
                        }
                        else
                        {
                            Console.WriteLine($"UnixJunctions.AppFolder: No DayZ installation found in {folder}!");
                        }
                    }
                    catch (DirectoryNotFoundException)
                    {
                        Console.WriteLine($"UnixJunctions.AppFolder: No DayZ installation found! Patch may not work correctly!");
                        //continue;
                    }

                }
                return null; // Add a return statement to ensure a value is always returned
            }
            string dayZPath = AppFolder();
            if (dayZPath != null)
            {
                steamDrive = Path.GetPathRoot(dayZPath).Replace("\\", "");
                int start = steamDrive.Length;
                int end = dayZPath.IndexOf("\\steamapps");
                if (end > start)
                {
                    absolutePath = dayZPath.Substring(start, end - start);
                    Console.WriteLine($"UnixJunctions.AppFolder: Found DayZ path in {absolutePath}");
                }
                else
                {
                    Console.WriteLine("UnixJunctions.AppFolder: Invalid DayZ path! Patch may not work correctly!");
                }
            }
            else
            {
                Console.WriteLine("UnixJunctions.AppFolder: DayZ installation not found! Patch may not work correctly!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("UnixJunctions: Exception locating DayZ library: " + ex.Message);
        }
    }

    public static void Create(string junctionPoint, string targetDir, bool overwrite)
    {
        Console.WriteLine("UnixJunctions: Create() called junctionPoint='" + junctionPoint + "' ; targetDir='" + targetDir + "'");

        targetDir = Path.GetFullPath(targetDir);

        if (Directory.Exists(junctionPoint) || File.Exists(junctionPoint))
        {
            Delete(junctionPoint);
        }

        junctionPoint = ToUnixPath(junctionPoint);
        targetDir = ToUnixPath(targetDir);

        junctionPoint = EscapeSingleQuotes(junctionPoint);
        targetDir = EscapeSingleQuotes(targetDir);

        RunShellCommand("ln", $"-s -T '{targetDir}' '{junctionPoint}'");
    }

    public static void Delete(string junctionPoint)
    {
        Console.WriteLine("UnixJunctions: Delete() called junctionPoint='" + junctionPoint + "'");

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
            junctionPoint = EscapeSingleQuotes(junctionPoint);
            RunShellCommand("rm", $"-r '{junctionPoint}'");
        }
    }

    public static bool Exists(string path)
    {
        Console.WriteLine("UnixJunctions: Exists() called path='" + path + "'");

        if (!Directory.Exists(path))
        {
            return false;
        }

        try
        {
            path = ToUnixPath(path);
            path = EscapeSingleQuotes(path);
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
        Console.WriteLine("UnixJunctions: GetTarget() called junctionPoint='" + junctionPoint + "'");

        junctionPoint = ToUnixPath(junctionPoint);
        junctionPoint = EscapeSingleQuotes(junctionPoint);
        string output = RunShellCommand("readlink", $"'{junctionPoint}'");
        return output.Trim();
    }

    private static string RunShellCommand(string command, string arguments)
    {
        Console.WriteLine("UnixJunctions.RunShellCommand: command= " + command + " ;arguments= " + arguments);

        var gameLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        var basePath = gameLocation + @"\linux-temp";
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
        var result = Regex.Replace(windowsPath, @"[A-Z]:", $"{absolutePath}").Replace("\\", "/");
        Console.WriteLine($"UnixJunctions.ToUnixPath: windowsPath='{windowsPath}', result='{result}'");
        return result;
    }

    private static string EscapeSingleQuotes(string path)
    {
        var result = path.Replace("'", @"'\''");
        Console.WriteLine($"UnixJunctions.EscapeSingleQutoes: path='{path}', result='{result}'");
        return result;
    }
}
