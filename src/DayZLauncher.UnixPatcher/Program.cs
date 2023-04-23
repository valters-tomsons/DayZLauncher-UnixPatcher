using DayZLauncher.UnixPatcher;
using DayZLauncher.UnixPatcher.Patches;

var userInput = Path.GetDirectoryName(args[0].Trim() + '/');
if (string.IsNullOrWhiteSpace(userInput))
{
    Utils.WriteLine("Must provide path to DayZ installation folder as argument!", ConsoleColor.Yellow);
    return;
}

var targetAssembly = $"{userInput}/Launcher/Utils.dll";
if (!File.Exists(targetAssembly))
{
    Utils.WriteLine("Could not find 'Launcher/Utils.dll' in target folder!", ConsoleColor.Red);
    return;
}

var backupAssembly = Utils.MoveFileToBackup(targetAssembly);

var baseDirectory = Path.GetDirectoryName(AppContext.BaseDirectory + '/');
var utilsPatchPath = baseDirectory + "/DayZLauncher.UnixPatcher.Utils.dll";

using var patchedUtils = UtilsAssemblyPatcher.PatchAssembly(backupAssembly, utilsPatchPath);
Console.WriteLine("Applying workshop mod fix...");
try
{
    patchedUtils.Write(targetAssembly);
    Utils.WriteLine("Utils.dll patched!", ConsoleColor.Green);

    File.Copy(utilsPatchPath, args[0].Trim() + "/Launcher/DayZLauncher.UnixPatcher.Utils.dll", true);
    Utils.WriteLine("UnixPatcher.Utils.dll deployed!", ConsoleColor.Green);
}
catch
{
    Utils.WriteLine("Failed to write files into DayZ directory!", ConsoleColor.Red);
    Utils.WriteLine("Reverting changes...", ConsoleColor.Yellow);
    File.Move(backupAssembly, targetAssembly);
    return;
}

Utils.WriteLine("Patching finished!", ConsoleColor.Green);
