namespace DayZLauncher.UnixPatcher;

public static class Common
{
    public static void WriteLine(string? message, ConsoleColor color = ConsoleColor.White)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = ConsoleColor.White;
    }

    public static string MoveFileToBackup(string filePath)
    {
        var newPath = filePath + ".bak";

        if (File.Exists(newPath))
        {
            WriteLine("Warning! Backup Utils.dll already exists, overwriting!");
        }

        File.Move(filePath, newPath, true);
        return newPath;
    }

    public static string? TryGetGamePrefixFromSystem()
    {
        var homePath = Environment.GetEnvironmentVariable("HOME");
        var homeSteamPrefix = $"{homePath}/.steam/steam/steamapps/compatdata/221100";

        if (Directory.Exists(homeSteamPrefix))
        {
            WriteLine($"Game prefix found from system: '{homeSteamPrefix}'");
            return homeSteamPrefix;
        }

        return null;
    }
}
