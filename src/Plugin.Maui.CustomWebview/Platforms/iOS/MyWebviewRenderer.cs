#if IOS
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using ObjCRuntime;
using Plugin.Maui.CustomWebview.Enums;
using Plugin.Maui.CustomWebview.Implementations;
using System.ComponentModel;
using System.Linq;
using UIKit;
using WebKit;

namespace Plugin.Maui.CustomWebview;

public class MyWKUIDelegate : NSObject, IWKUIDelegate
{
    private UIViewController GetVisibleViewController()
    {
        UIViewController viewController = null;

        if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
        {
            var window = UIApplication.SharedApplication.ConnectedScenes
                .OfType<UIWindowScene>()
                .SelectMany(scene => scene.Windows)
                .FirstOrDefault(w => w.IsKeyWindow);

            viewController = window?.RootViewController;
        }
        else
        {
#pragma warning disable CA1422 // Validate platform compatibility
            viewController = UIApplication.SharedApplication.KeyWindow?.RootViewController;
#pragma warning restore CA1422 // Validate platform compatibility
        }

        while (viewController?.PresentedViewController != null)
        {
            viewController = viewController.PresentedViewController;
        }

        return viewController;
    }

    [Export("webView:runJavaScriptAlertPanelWithMessage:initiatedByFrame:completionHandler:")]
    public void RunJavaScriptAlertPanel(WebKit.WKWebView webView, string message, WKFrameInfo frame, Action completionHandler)
    {
        var alertController = UIAlertController.Create(null, message, UIAlertControllerStyle.Alert);
        alertController.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
        
        var viewController = GetVisibleViewController();
        if (viewController != null)
        {
            viewController.PresentViewController(alertController, true, null);
        }

        completionHandler();
    }

    [Export("webView:runJavaScriptConfirmPanelWithMessage:initiatedByFrame:completionHandler:")]
    public void RunJavaScriptConfirmPanel(WKWebView webView, string message, WKFrameInfo frame, Action<bool> completionHandler)
    {
        var alertController = UIAlertController.Create(null, message, UIAlertControllerStyle.Alert);

        alertController.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, okAction =>
        {

            completionHandler(true);

        }));

        alertController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Default, cancelAction =>
        {

            completionHandler(false);

        }));

        var viewController = GetVisibleViewController();
        if (viewController != null)
        {
            viewController.PresentViewController(alertController, true, null);
        }
    }

    [Export("webView:runJavaScriptTextInputPanelWithPrompt:defaultText:initiatedByFrame:completionHandler:")]
    public void RunJavaScriptTextInputPanel(WKWebView webView, string prompt, string defaultText, WebKit.WKFrameInfo frame, System.Action<string> completionHandler)
    {
        var alertController = UIAlertController.Create(null, prompt, UIAlertControllerStyle.Alert);

        UITextField alertTextField = null;
        alertController.AddTextField(textField =>
        {
            textField.Placeholder = defaultText;
            alertTextField = textField;
        });

        alertController.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, okAction =>
        {

            completionHandler(alertTextField.Text);

        }));

        alertController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Default, cancelAction =>
        {

            completionHandler(null);

        }));

        var viewController = GetVisibleViewController();
        if (viewController != null)
        {
            viewController.PresentViewController(alertController, true, null);
        }
    }
}

 public class RequestHandler : NSObject, IWKScriptMessageHandler
    {
        private readonly MyWebviewRenderer renderer;

        public RequestHandler(MyWebviewRenderer renderer)
        {
            this.renderer = renderer;
        }

        private class RequestModel
        {
            public string Url { get; set; }
            public string Method { get; set; }
        }

        [Export("userContentController:didReceiveScriptMessage:")]
        public void DidReceiveScriptMessage(WKUserContentController userContentController, WKScriptMessage message)
        {
              if (renderer.VirtualView == null || message == null || message.Body == null) return;
                renderer.VirtualView.HandleScriptReceived(message.Body.ToString());
        }
    }

public class MyWebviewRenderer : ViewHandler<ExtendedWebView, WebKit.WKWebView>
{
    WKWebView wkWebView;
    public static string BaseUrl { get; set; } = NSBundle.MainBundle.BundlePath;

    public NativeHandle Handle => throw new NotImplementedException();

    public static event EventHandler<WKWebView> OnControlChanged;
    MyNavigationDelegate _navigationDelegate;
    WKWebViewConfiguration _configuration;
    WKUserContentController _contentController;
    public static IPropertyMapper<ExtendedWebView, MyWebviewRenderer> PropertyMapper = new PropertyMapper<ExtendedWebView, MyWebviewRenderer>(ViewHandler.ViewMapper)
    {
        [nameof(ExtendedWebView.Source)] = ConfigureWebViewData
    };

    private static void ConfigureWebViewData(MyWebviewRenderer handler, ExtendedWebView extendedWebView)
    {
        if (handler is MyWebviewRenderer)
        {
            handler.SetSource();
        }
    }

    void SetupControl()
    {
        _navigationDelegate = new MyNavigationDelegate(this);
        _contentController = new WKUserContentController();
        _contentController.AddScriptMessageHandler(new RequestHandler(this), "invokeAction");
        _configuration = new WKWebViewConfiguration
        {
            UserContentController = _contentController
        };

        wkWebView = new WKWebView(new CGRect(0, 0, 100, 200), _configuration)
        {
            Opaque = false,
            UIDelegate = new MyWKUIDelegate(),
            NavigationDelegate = _navigationDelegate
        };
        ExtendedWebView.CallbackAdded += OnCallbackAdded;
        OnControlChanged?.Invoke(this, wkWebView);
    }

    void SetupElement(ExtendedWebView element)
    {
        element.PropertyChanged += OnPropertyChanged;
        element.OnJavascriptInjectionRequest += OnJavascriptInjectionRequest;
        element.OnClearCookiesRequested += OnClearCookiesRequest;
        element.OnBackRequested += OnBackRequested;
        element.OnForwardRequested += OnForwardRequested;
        element.OnRefreshRequested += OnRefreshRequested;

        SetSource();
    }

    public MyWebviewRenderer() : base(PropertyMapper)
    {
    }
    protected override WKWebView CreatePlatformView()
    {
        SetupControl();
        SetupElement(VirtualView);
        return wkWebView;
    }

    protected override void ConnectHandler(WKWebView platformView)
    {
        base.ConnectHandler(platformView);
    }

    void SetSource()
    {
        if (VirtualView == null) return;

        switch (VirtualView.ContentType)
        {
            case WebViewContentType.Internet:
                LoadInternetContent();
                break;

            case WebViewContentType.LocalFile:
                LoadLocalFile();
                break;

            case WebViewContentType.StringData:
                LoadStringData();
                break;
        }
    }

    void LoadStringData()
    {
        if (VirtualView == null) return;

        var nsBaseUri = new NSUrl($"file://{VirtualView.BaseUrl ?? BaseUrl}");
        wkWebView.LoadHtmlString(VirtualView.Source, nsBaseUri);
    }

    void LoadLocalFile()
    {
        if (VirtualView == null) return;

        var path = Path.Combine(VirtualView.BaseUrl ?? BaseUrl, VirtualView.Source);
        var nsFileUri = new NSUrl($"file://{path}");
        var nsBaseUri = new NSUrl($"file://{VirtualView.BaseUrl ?? BaseUrl}");

        wkWebView.LoadFileUrl(nsFileUri, nsBaseUri);
    }

    void LoadInternetContent()
    {
        if (wkWebView == null || VirtualView == null) return;

        var headers = new NSMutableDictionary();

        foreach (var header in VirtualView.LocalRegisteredHeaders)
        {
            var key = new NSString(header.Key);
            if (!headers.ContainsKey(key))
                headers.Add(key, new NSString(header.Value));
        }

        if (VirtualView.EnableGlobalHeaders)
        {
            foreach (var header in ExtendedWebView.GlobalRegisteredHeaders)
            {
                var key = new NSString(header.Key);
                if (!headers.ContainsKey(key))
                    headers.Add(key, new NSString(header.Value));
            }
        }

        var url = new NSUrl(VirtualView.Source);
        var request = new NSMutableUrlRequest(url)
        {
            Headers = headers
        };

        wkWebView.LoadRequest(request);
    }

    async void OnCallbackAdded(object sender, string e)
    {
        if (VirtualView == null || string.IsNullOrWhiteSpace(e)) return;

        if ((sender == null && VirtualView.EnableGlobalCallbacks) || sender != null)
            await OnJavascriptInjectionRequest(ExtendedWebView.GenerateFunctionScript(e));
    }

    void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case "Source":
                SetSource();
                break;
        }
    }

    private async Task OnClearCookiesRequest()
    {
        if (wkWebView == null) return;

        var store = _configuration.WebsiteDataStore.HttpCookieStore;

        var cookies = await store.GetAllCookiesAsync();
        foreach (var c in cookies)
        {
            await store.DeleteCookieAsync(c);
        }

    }

    internal async Task<string> OnJavascriptInjectionRequest(string js)
    {
        if (wkWebView == null || VirtualView == null) return string.Empty;

        var response = string.Empty;

        try
        {
            var obj = await wkWebView.EvaluateJavaScriptAsync(js).ConfigureAwait(true);
            if (obj != null)
                response = obj.ToString();
        }

        catch (Exception) { /* The Webview might not be ready... */ }
        return response;
    }

    void OnRefreshRequested(object sender, EventArgs e)
    {
        if (wkWebView == null) return;
        wkWebView.ReloadFromOrigin();
    }

    void OnForwardRequested(object sender, EventArgs e)
    {
        if (wkWebView == null || VirtualView == null) return;

        if (wkWebView.CanGoForward)
            wkWebView.GoForward();
    }

    void OnBackRequested(object sender, EventArgs e)
    {
        if (wkWebView == null || VirtualView == null) return;

        if (wkWebView.CanGoBack)
            wkWebView.GoBack();
    }

    public void Dispose()
    {

    }
}
#endif