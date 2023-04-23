namespace DayZLauncher.UnixPatcher;

public static class Common
{
    public static void WriteLine(string? message, ConsoleColor color = ConsoleColor.White)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.BackgroundColor = ConsoleColor.White;
    }

    public static string MoveFileToBackup(string filePath)
    {
        var newPath = filePath + ".bak";

        if (File.Exists(newPath))
        {
            Console.WriteLine("Warning! Backup Utils.dll already exists, overwriting!");
        }

        File.Move(filePath, newPath, true);
        return newPath;
    }
}
