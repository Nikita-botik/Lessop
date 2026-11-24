using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using Syncfusion.Maui.Core.Hosting;

namespace Lessop;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder.UseMauiApp<App>();
        builder

            .UseMauiApp<App>()
            .ConfigureSyncfusionCore()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("bold.ttf", "BoldFont");
                fonts.AddFont("bold.otf", "BoldFontOTF");
            })
            .ConfigureLifecycleEvents(events =>
             {
#if WINDOWS
                            events.AddWindows(windows =>
                            {
                                windows.OnWindowCreated(window =>
                                {
                                    var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                                    var id = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
                                    var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(id);

                                    // Робимо чорний фон
                                    if (window.Content is Microsoft.UI.Xaml.FrameworkElement fe)
{
    fe.RequestedTheme = Microsoft.UI.Xaml.ElementTheme.Dark;
}

                                });
                            });
#endif
             });
#if DEBUG
        builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
