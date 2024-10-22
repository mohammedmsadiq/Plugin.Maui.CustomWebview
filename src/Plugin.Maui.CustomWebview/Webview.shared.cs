namespace Plugin.Maui.CustomWebview;

public static class Webview
{
	static IWebview? defaultImplementation;

	/// <summary>
	/// Provides the default implementation for static usage of this API.
	/// </summary>
	public static IWebview Default =>
		defaultImplementation ??= new WebviewImplementation();

	internal static void SetDefault(IWebview? implementation) =>
		defaultImplementation = implementation;
}
