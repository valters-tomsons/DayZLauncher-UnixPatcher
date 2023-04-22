using DayZLauncher.UnixPatcher;

var userInput = args[0].Trim();
if (string.IsNullOrWhiteSpace(userInput))
{
    Console.WriteLine("Must provide path to DayZ installation folder as argument!");
    return;
}

var targetAssembly = $"{userInput}/Launcher/Utils.dll";
if (!File.Exists(targetAssembly))
{
    Console.WriteLine("Could not find 'Launcher/Utils.dll' in target folder!");
    return;
}

var backupAssembly = Utils.MoveFileToBackup(targetAssembly);

var baseDirectory = Path.GetDirectoryName(AppContext.BaseDirectory + '/');
var utilsPatchPath = baseDirectory + "/DayZLauncher.UnixPatcher.Utils.dll";

using var patchedUtils = AssemblyPatcher.PatchUtilsAssembly(backupAssembly, utilsPatchPath);
Console.WriteLine("Writing patches to disk...");

try
{
    patchedUtils.Write(targetAssembly);
    File.Copy(utilsPatchPath, args[0].Trim() + "/Launcher/DayZLauncher.UnixPatcher.Utils.dll", true);
}
catch
{
    Console.WriteLine("Failed to write files into DayZ directory!");
    Console.WriteLine("Aborting...");
    File.Move(backupAssembly, targetAssembly);
}

Console.WriteLine("Patch applied!");
