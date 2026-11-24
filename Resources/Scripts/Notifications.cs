#if WINDOWS
using System.Collections.Concurrent;
using CommunityToolkit.WinUI.Notifications;

namespace Lessop;

public class Notifications
{
    private readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> questions = new();

    public async Task<bool> ShowWindowsToastQuestion(
        string title, string description, string yes, string no, CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        string id = Guid.NewGuid().ToString("N");
        var response = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        questions[id] = response;
        try
        {
            new ToastContentBuilder()
                .SetToastDuration(ToastDuration.Long)
                .AddText(title)
                .AddText(description)
                .AddButton(new ToastButton()
                    .SetContent(yes)
                    .AddArgument("question", id)
                    .AddArgument("action", "yes")
                    .SetBackgroundActivation())
                .AddButton(new ToastButton()
                    .SetContent(no)
                    .AddArgument("question", id)
                    .AddArgument("action", "no")
                    .SetBackgroundActivation())
                .Show();

            return await response.Task.WaitAsync(TimeSpan.FromMinutes(10), cancellation);
        }
        catch (TimeoutException)
        {
            return false;
        }
        finally
        {
            questions.TryRemove(id, out _);
        }
    }

    public void ProcessActivation(ToastNotificationActivatedEventArgsCompat activation)
    {
        var args = ToastArguments.Parse(activation.Argument);
        if (args.TryGetValue("question", out var id) &&
            args.TryGetValue("action", out var action) &&
            action is "yes" or "no" &&
            questions.TryGetValue(id, out var response))
        {
            response.TrySetResult(action == "yes");
        }
    }

    public void ShowWindowsToast(string title, string description)
    {
        new ToastContentBuilder()
            .SetToastDuration(ToastDuration.Long)
            .AddText(title)
            .AddText(description)
            .AddButton(new ToastButton()
                .SetContent("OK")
                .AddArgument("action", "Ok")
                .SetBackgroundActivation())
            .Show();
    }
}
#endif
