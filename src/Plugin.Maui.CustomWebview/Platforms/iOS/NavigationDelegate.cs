using Foundation;
using Plugin.Maui.CustomWebView.Implementations;
using UIKit;
using WebKit;

namespace Plugin.Maui.CustomWebView.Platforms.iOS

public class NavigationDelegate : WKNavigationDelegate
{
    readonly WeakReference<CustomWebviewRenderer> Reference;

    public NavigationDelegate(CustomWebviewRenderer renderer)
    {
        Reference = new WeakReference<CustomWebviewRenderer>(renderer);
    }

    public bool AttemptOpenCustomUrlScheme(NSUrl url)
    {
        var app = UIApplication.SharedApplication;

        if (app.CanOpenUrl(url))
            return app.OpenUrl(url);

        return false;
    }

    [Export("webView:decidePolicyForNavigationAction:decisionHandler:")]
    public override void DecidePolicy(WKWebView webView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler)
    {
        if (Reference == null || !Reference.TryGetTarget(out CustomWebviewRenderer renderer)) return;
        if (renderer.Element == null) return;

        var response = renderer.Element.HandleNavigationStartRequest(navigationAction.Request.Url.ToString());

        if (response.Cancel || response.OffloadOntoDevice)
        {
            if (response.OffloadOntoDevice)
                AttemptOpenCustomUrlScheme(navigationAction.Request.Url);

            decisionHandler(WKNavigationActionPolicy.Cancel);
        }

        else
        {
            decisionHandler(WKNavigationActionPolicy.Allow);
            renderer.Element.Navigating = true;
        }
    }

    public override void DecidePolicy(WKWebView webView, WKNavigationResponse navigationResponse, Action<WKNavigationResponsePolicy> decisionHandler)
    {
        if (Reference == null || !Reference.TryGetTarget(out CustomWebviewRenderer renderer)) return;
        if (renderer.Element == null) return;

        if (navigationResponse.Response is NSHttpUrlResponse)
        {
            var code = ((NSHttpUrlResponse)navigationResponse.Response).StatusCode;
            if (code >= 400)
            {
                renderer.Element.Navigating = false;
                renderer.Element.HandleNavigationError((int)code);
                decisionHandler(WKNavigationResponsePolicy.Cancel);
                return;
            }
        }

        decisionHandler(WKNavigationResponsePolicy.Allow);
    }

    [Export("webView:didFinishNavigation:")]
    public async override void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
    {
        if (Reference == null || !Reference.TryGetTarget(out CustomWebviewRenderer renderer)) return;
        if (renderer.Element == null) return;

        renderer.Element.HandleNavigationCompleted(webView.Url.ToString());
        await renderer.OnJavascriptInjectionRequest(ExtendedWebView.InjectedFunction);

        if (renderer.Element.EnableGlobalCallbacks)
            foreach (var function in ExtendedWebView.GlobalRegisteredCallbacks)
                await renderer.OnJavascriptInjectionRequest(ExtendedWebView.GenerateFunctionScript(function.Key));

        foreach (var function in renderer.Element.LocalRegisteredCallbacks)
            await renderer.OnJavascriptInjectionRequest(ExtendedWebView.GenerateFunctionScript(function.Key));

        renderer.Element.CanGoBack = webView.CanGoBack;
        renderer.Element.CanGoForward = webView.CanGoForward;
        renderer.Element.Navigating = false;
        renderer.Element.HandleContentLoaded();
    }
}
