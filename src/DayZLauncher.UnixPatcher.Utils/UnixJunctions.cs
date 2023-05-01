using System;
using System.IO;
using Mono.Unix;

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

        if (Directory.Exists(junctionPoint) || File.Exists(junctionPoint))
        {
            Delete(junctionPoint);
        }

        junctionPoint = ToUnixPath(junctionPoint);
        targetDir = ToUnixPath(targetDir);

        var symlinkInfo = new UnixSymbolicLinkInfo(junctionPoint);
        symlinkInfo.CreateSymbolicLinkTo(targetDir);
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
            UnixFileSystemInfo.TryGetFileSystemEntry(junctionPoint, out var unixFileInfo);
            unixFileInfo?.Delete();
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
            var symlinkInfo = new UnixSymbolicLinkInfo(path);
            return symlinkInfo.HasContents;
        }
        catch
        {
            return false;
        }
    }

    public static string GetTarget(string junctionPoint)
    {
        var symlinkInfo = new UnixSymbolicLinkInfo(junctionPoint);
        return symlinkInfo.ContentsPath;
    }

    private static string ToUnixPath(string windowsPath)
    {
        return windowsPath.Replace("Z:", string.Empty).Replace("\\", "/");
    }
}