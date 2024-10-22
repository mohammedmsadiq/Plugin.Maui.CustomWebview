namespace Plugin.Maui.CustomWebview;

public static class CustomWebview
{
	static ICustomWebview? defaultImplementation;

	/// <summary>
	/// Provides the default implementation for static usage of this API.
	/// </summary>
	public static ICustomWebview Default =>
		defaultImplementation ??= new WebviewImplementation();

	internal static void SetDefault(ICustomWebview? implementation) =>
		defaultImplementation = implementation;
}
