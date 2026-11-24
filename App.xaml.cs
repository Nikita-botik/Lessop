using AutoMeetings;

#if WINDOWS
using CommunityToolkit.WinUI.Notifications;
#endif

namespace Lessop;

public partial class App : Application
{
#if WINDOWS
    public static Notifications Notifications { get; } = new();
#endif

    public App()
    {
        InitializeComponent();
        UserAppTheme = AppTheme.Dark;
#if WINDOWS
        ToastNotificationManagerCompat.OnActivated += Notifications.ProcessActivation;
#endif
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());
        var closing = new CancellationTokenSource();
        Task worker = Task.Run(() => new Programs().RunAsync(closing.Token));
        window.Destroying += async (_, _) =>
        {
            closing.Cancel();
            try
            {
                await worker;
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                closing.Dispose();
            }
        };

        return window;
    }
}
