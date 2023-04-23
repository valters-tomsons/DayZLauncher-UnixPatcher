using System.Xml.Linq;

namespace DayZLauncher.UnixPatcher.Patches;

public static class LauncherConfigPatcher
{
    public static void PatchLauncherConfigFile(string gamePath)
    {
        var configFilePath = $"{gamePath}/DayZLauncher.exe.config";
        var configFile = XDocument.Load(configFilePath);

        var userGroup = configFile.Descendants("sectionGroup").First(x => x.Attribute("name").Equals("userSettings"));
        userGroup?.Remove();

        var userSettings = configFile.Descendants("userSettings");
        userSettings?.Remove();
    }

    public static void RemoveOldUserConfig(string gamePath)
    {
        var prefixPath = $"{gamePath}/../../compatdata/221100/pfx";
        var bohemiaPath = $"{prefixPath}/drive_c/users/steamuser/AppData/Local/Bohemia Interactive/";

        if (!Directory.Exists(prefixPath))
        {
            Utils.WriteLine($"Failed to find game prefix at: '{prefixPath}'", ConsoleColor.Red);
            Utils.WriteLine($"""
                You should delete the following file manually for saving to work!!! 
                > {bohemiaPath}/DayZ Launcher_*/*/user.config
            """, ConsoleColor.Yellow);
            return;
        }

        Utils.WriteLine("Proton prefix found!");

        if (!Directory.Exists(bohemiaPath))
        {
            Utils.WriteLine("Could not find config settings folder, nothing to patch");
            return;
        }

        var enumerationOptions = new EnumerationOptions() { RecurseSubdirectories = true };
        var configFiles = Directory.EnumerateFiles(bohemiaPath, "user.config", SearchOption.AllDirectories);

        foreach (var file in configFiles)
        {
            Utils.WriteLine($"Deleting settings file: {file}", ConsoleColor.Green);
            File.Delete(file);
        }
    }
}
