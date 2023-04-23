using System.Xml;
using System.Xml.Linq;

namespace DayZLauncher.UnixPatcher.Patches;

public static class LauncherConfigPatcher
{
    public static async Task PatchLauncherConfigFile(string gamePath)
    {
        var configFilePath = $"{gamePath}/DayZLauncher.exe.config";
        string? newXml = string.Empty;

        if (!File.Exists(configFilePath))
        {
            Utils.WriteLine("Failed to find DayZLauncher.exe.config!");
            return;
        }

        using (var configStream = new FileStream(configFilePath, FileMode.Open, FileAccess.Read, FileShare.None, 4096, true))
        {
            var configXml = new XmlDocument();
            configXml.Load(configStream);
            await configStream.FlushAsync();

            var sectionGroup = configXml.SelectSingleNode("//sectionGroup[@name='userSettings']");
            sectionGroup?.ParentNode?.RemoveChild(sectionGroup);

            var userSettings = configXml.SelectSingleNode("//userSettings");
            userSettings?.ParentNode?.RemoveChild(userSettings);

            newXml = new string(configXml.OuterXml);
        }

        await File.WriteAllTextAsync(configFilePath, IndentXml(newXml));
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

    private static string IndentXml(string xml)
    {
        var doc = new XmlDocument();
        doc.LoadXml(xml);

        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            NewLineChars = "\r\n",
            NewLineHandling = NewLineHandling.Replace
        };

        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, settings);
        doc.Save(xmlWriter);
        return stringWriter.ToString();
    }

}
