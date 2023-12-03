namespace DayZLauncher.UnixPatcher.Patches;

public static class LauncherConfigPatcher
{
    public static void ApplySettingsBandAid(string gamePath)
    {
        var prefixPath = $"{gamePath}/../../compatdata/221100/pfx";

        if (!Directory.Exists(prefixPath))
        {
            var systemPrefix = Common.TryGetGamePrefixFromSystem();
            if (systemPrefix is null)
            {
                Common.WriteLine($"Failed to find game prefix at: '{prefixPath}'", ConsoleColor.Red);
                throw new Exception();
            }

            prefixPath = systemPrefix;
        }

        Common.WriteLine("Proton prefix found!");

        var bohemiaPath = $"{prefixPath}/drive_c/users/steamuser/AppData/Local/Bohemia Interactive a.s.";
        if (!Directory.Exists(bohemiaPath))
        {
            Common.WriteLine($"Failed to find Bohemia settings folder at: '{bohemiaPath}'", ConsoleColor.Red);
            throw new Exception();
        }

        var symLinkPath = bohemiaPath[0..^1];
        File.CreateSymbolicLink(symLinkPath, bohemiaPath);
    }
}
