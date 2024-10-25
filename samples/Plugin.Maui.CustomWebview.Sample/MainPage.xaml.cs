using Plugin.Maui.CustomWebView;
using Plugin.Maui.CustomWebView.Interfaces;

namespace Plugin.Maui.CustomWebview.Sample;

public partial class MainPage : ContentPage
{
	readonly ICustomWebview Webview;

	public MainPage(ICustomWebview Webview)
	{
		InitializeComponent();
		
		this.Webview = Webview;
	}
}
