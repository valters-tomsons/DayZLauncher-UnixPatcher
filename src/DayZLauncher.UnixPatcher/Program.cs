using DayZLauncher.UnixPatcher;
using DayZLauncher.UnixPatcher.Patches;

var userInput = Path.GetDirectoryName(args[0].Trim() + '/');
if (string.IsNullOrWhiteSpace(userInput))
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
Console.WriteLine("Applying workshop mod fix...");
try
{
    var baseDirectory = Path.GetDirectoryName(AppContext.BaseDirectory + '/');
    var utilsPatchPath = baseDirectory + "/DayZLauncher.UnixPatcher.Utils.dll";

    using (var patchedUtils = UtilsAssemblyPatcher.PatchAssembly(backupAssembly, utilsPatchPath))
    {
        patchedUtils.Write(targetAssembly);
        Common.WriteLine("Utils.dll patched!", ConsoleColor.Green);
    }

    File.Copy(utilsPatchPath, args[0].Trim() + "/Launcher/DayZLauncher.UnixPatcher.Utils.dll", true);
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

Common.WriteLine("Applying launcher settings fix...");
try
{
    await LauncherConfigPatcher.PatchLauncherConfigFile(userInput);
    LauncherConfigPatcher.RemoveOldUserConfig(userInput);
}
catch
{
    Common.WriteLine("Failed to patch launcher settings", ConsoleColor.Red);
    Common.WriteLine("Launcher may continue to not save properly", ConsoleColor.Yellow);
}

Common.WriteLine("Patching finished!");