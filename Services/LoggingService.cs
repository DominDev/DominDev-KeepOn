namespace KeepOn;

internal sealed class LoggingService
{
    private readonly Lock _lock = new();

    public void Info(string message)
    {
        Write("INFO", message);
    }

    public void Error(string message, Exception? exception = null)
    {
        Write("ERROR", exception is null ? message : $"{message} {exception}");
    }

    private void Write(string level, string message)
    {
        try
        {
            Directory.CreateDirectory(AppPaths.LogsDirectory);
            var path = Path.Combine(AppPaths.LogsDirectory, $"{AppPaths.AppName}_{DateTime.Now:yyyy-MM-dd}.log");
            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}{Environment.NewLine}";

            lock (_lock)
            {
                File.AppendAllText(path, line);
            }
        }
        catch
        {
            // Logging must never break the tray application lifecycle.
        }
    }
}
