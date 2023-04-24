using System.Text;
using System.Xml;

namespace DayZLauncher.UnixPatcher.Patches;

public static class LauncherConfigPatcher
{
    public static async Task PatchLauncherConfigFile(string gamePath)
    {
        var configFilePath = $"{gamePath}/DayZLauncher.exe.config";
        string? newXml = string.Empty;

        if (!File.Exists(configFilePath))
        {
            Common.WriteLine("Failed to find DayZLauncher.exe.config!");
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

        newXml = IndentXml(newXml);
        await File.WriteAllTextAsync(configFilePath, newXml);
    }

    public static void RemoveOldUserConfig(string gamePath)
    {
        var prefixPath = $"{gamePath}/../../compatdata/221100/pfx";

        if (!Directory.Exists(prefixPath))
        {
            var systemPrefix = Common.TryGetGamePrefixFromSystem();
            if (systemPrefix is null)
            {
                Common.WriteLine($"Failed to find game prefix at: '{prefixPath}'", ConsoleColor.Red);
                Common.WriteLine("""
                You should manually delete the following file for launcher to work!!! 
                > C:/users/steamuser/AppData/Local/Bohemia Interactive/DayZ Launcher_*/*/user.config
                """,
                ConsoleColor.Yellow);
                return;
            }

            prefixPath = systemPrefix;
        }

        Common.WriteLine("Proton prefix found!");

        var bohemiaPath = $"{prefixPath}/drive_c/users/steamuser/AppData/Local/Bohemia Interactive/";
        if (!Directory.Exists(bohemiaPath))
        {
            Common.WriteLine("Could not find config settings folder, nothing to patch");
            return;
        }

        var configFiles = Directory.EnumerateFiles(bohemiaPath, "user.config", SearchOption.AllDirectories);
        foreach (var file in configFiles)
        {
            Common.WriteLine($"Deleting settings file: {file}", ConsoleColor.Green);
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
            NewLineHandling = NewLineHandling.Replace,
            Encoding = Encoding.UTF8
        };

        var sb = new StringBuilder();
        using var xmlWriter = XmlWriter.Create(sb, settings);
        doc.Save(xmlWriter);
        var xmlString = sb.ToString();
        return xmlString.Replace("utf-16", "utf-8");
    }
}
