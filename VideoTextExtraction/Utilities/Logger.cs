namespace VideoTextExtraction.Utilities;

/// <summary>
/// Simple console logger with colored output
/// </summary>
public static class Logger
{
    public static void Info(string message)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"[INFO] {DateTime.Now:HH:mm:ss} - {message}");
        Console.ResetColor();
    }

    public static void Success(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[SUCCESS] {DateTime.Now:HH:mm:ss} - {message}");
        Console.ResetColor();
    }

    public static void Warning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[WARNING] {DateTime.Now:HH:mm:ss} - {message}");
        Console.ResetColor();
    }

    public static void Error(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[ERROR] {DateTime.Now:HH:mm:ss} - {message}");
        Console.ResetColor();
    }

    public static void Error(string message, Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[ERROR] {DateTime.Now:HH:mm:ss} - {message}");
        Console.WriteLine($"Exception: {ex.Message}");
        Console.ResetColor();
    }
}
