using AutoMeetingsPort;
internal static class FileSystem
{
    public static string AppDataDirectory { get; } = Path.Combine(AppContext.BaseDirectory, Guid.NewGuid().ToString());
}

internal static class MainThread
{
    public static Task<T> InvokeOnMainThreadAsync<T>(Func<Task<T>> action) => action();
}
internal static class Launcher
{
    public static int Opened { get; set; }
    public static Task<bool> OpenAsync(Uri uri)
    {
        Opened++;
        return Task.FromResult(true);
    }
}

namespace AutoMeetingsPort
{
    public class OptionItem
    {
        public string Name { get; set; } = "";
        public string Tag { get; set; } = "";
    }

    internal sealed class ReadFromURL
    {
        public static bool Alarm { get; set; }
        public static bool Unavailable { get; set; }
        public Task<bool> ReadAsync(int region, CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();
            if (Unavailable)
                throw new IOException("Test connection failure");
            return Task.FromResult(Alarm);
        }
    }

    internal static class App
    {
        public static Notifications Notifications { get; } = new();
    }

    internal sealed class Notifications
    {
        public TaskCompletionSource<bool>? Question { get; private set; }
        public string? Description { get; private set; }
        public int Questions { get; private set; }
        public int Errors { get; private set; }
        public Task<bool> ShowWindowsToastQuestion(string title, string description, string yes, string no, CancellationToken cancellation)
        {
            Questions++;
            Description = description;
            Question = new();
            return Question.Task.WaitAsync(cancellation);
        }
        public void ShowWindowsToast(string title, string description) => Errors++;
    }
}

namespace AutoMeetingsPort.Resources.Strings
{
    internal static class AppResources
    {
        public const string Kyiv = "Kyiv", ErrorTitle = "Error", AlarmStatusError = "Alarm status unavailable",
            TheUrlOpenError = "Cannot open", ConfirmationTitle = "Confirm", ConfTextOnAlert = "Alert",
            ConfTextOnUnAlert = "All clear", Yes = "Yes", No = "No";
    }
}
