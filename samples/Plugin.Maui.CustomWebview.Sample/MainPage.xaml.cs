using Plugin.Maui.CustomWebview;

namespace Plugin.Maui.CustomWebview.Sample;

public partial class MainPage : ContentPage
{
	readonly IWebview Webview;

	public MainPage(IWebview Webview)
	{
		InitializeComponent();
		
		this.Webview = Webview;
	}
}
