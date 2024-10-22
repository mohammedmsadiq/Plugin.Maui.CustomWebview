using Plugin.Maui.Webview;

namespace Plugin.Maui.Webview.Sample;

public partial class MainPage : ContentPage
{
	readonly IWebview Webview;

	public MainPage(IWebview Webview)
	{
		InitializeComponent();
		
		this.Webview = Webview;
	}
}
