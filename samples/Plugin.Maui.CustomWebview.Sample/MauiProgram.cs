using Plugin.Maui.CustomWebview;
using Plugin.Maui.CustomWebview.Implementations;

namespace Plugin.Maui.CustomWebview.Sample;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			})
			.ConfigureMauiHandlers(handlers =>
			{
#if ANDROID
				handlers.AddHandler(typeof(ExtendedWebView), typeof(CustomWebviewRenderer));
#endif
#if IOS
				handlers.AddHandler(typeof(ExtendedWebView), typeof(Plugin.Maui.CustomWebview.MyWebviewRenderer));
				// handlers.AddHandler(typeof(ExtendedWebView), typeof(Plugin.Maui.CustomWebview.CustomWebviewRenderer));
				
#endif
			});

		builder.Services.AddTransient<MainPage>();

		return builder.Build();
	}
}