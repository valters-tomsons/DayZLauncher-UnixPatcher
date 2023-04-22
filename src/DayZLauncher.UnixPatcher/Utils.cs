namespace DayZLauncher.UnixPatcher;

public static class Utils
{
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
