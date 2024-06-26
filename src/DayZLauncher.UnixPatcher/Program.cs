﻿using DayZLauncher.UnixPatcher;
using DayZLauncher.UnixPatcher.Patches;

var gameSystemPath = Common.TryGetGameInstallPathFromSystem();
var gameFound = !string.IsNullOrWhiteSpace(gameSystemPath);

string? userInput;
if (gameFound)
{
    Common.WriteLine($"Found DayZ installation at '{gameSystemPath}'", ConsoleColor.DarkGreen);
    userInput = gameSystemPath;
}
else if (args is null || args.Length < 1)
{

    Common.WriteLine("");
    Common.WriteLine("Enter full DayZ installation path and press ENTER");
    Console.Write("> ");
    userInput = Console.ReadLine()?.Trim() + '/';
}
else
{
    userInput = Path.GetDirectoryName(args[0].Trim() + '/');
}

if (string.IsNullOrWhiteSpace(userInput) || !Directory.Exists(userInput))
{
    Common.WriteLine("Must provide path to DayZ installation folder as argument!", ConsoleColor.Yellow);
    return;
}

var targetAssembly = $"{userInput}/Launcher/Utils.dll";
if (!File.Exists(targetAssembly))
{
    Common.WriteLine("Could not find 'Launcher/Utils.dll' in target folder!", ConsoleColor.Red);
    return;
}

var backupAssembly = Common.MoveFileToBackup(targetAssembly);
Common.WriteLine("Applying workshop mods fix...");
try
{
    var baseDirectory = Path.GetDirectoryName(AppContext.BaseDirectory + '/');
    var utilsPatchPath = baseDirectory + "/DayZLauncher.UnixPatcher.Utils.dll";

    using (var patchedUtils = UtilsAssemblyPatcher.PatchAssembly(backupAssembly, utilsPatchPath))
    {
        patchedUtils.Write(targetAssembly);
        Common.WriteLine("Utils.dll patched!", ConsoleColor.Green);
    }

    File.Copy(utilsPatchPath, userInput + "/Launcher/DayZLauncher.UnixPatcher.Utils.dll", true);
    Common.WriteLine("UnixPatcher.Utils.dll deployed!", ConsoleColor.Green);
}
catch (Exception e)
{
    Common.WriteLine(e.Message);
    Common.WriteLine("Failed to write files into DayZ directory!", ConsoleColor.Red);
    Common.WriteLine("Workshop mods will not work!", ConsoleColor.Red);
    Common.WriteLine("Reverting changes...", ConsoleColor.Yellow);

    File.Delete(targetAssembly);
    File.Move(backupAssembly, targetAssembly);
}

Common.WriteLine("Patching finished!");