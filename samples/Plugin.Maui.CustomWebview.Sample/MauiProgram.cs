using Plugin.Maui.CustomWebView;
using Plugin.Maui.CustomWebView.Implementations;

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
#if ANDROID || IOS
				handlers.AddHandler(typeof(ExtendedWebView), typeof(CustomWebviewRenderer));
#endif
			});

		builder.Services.AddTransient<MainPage>();

		return builder.Build();
	}
}