using System.Net.Mime;
using Plugin.Maui.CustomWebView.Enums;
using WebViewContentType = Plugin.Maui.CustomWebView.Enums.WebViewContentType;

namespace Plugin.Maui.CustomWebView.Implementations;

public partial class ExtendedWebView
{
    internal static event EventHandler<string> CallbackAdded;

    public static readonly BindableProperty NavigatingProperty = BindableProperty.Create(nameof(Navigating), typeof(bool), typeof(ExtendedWebView), false);

    public static readonly BindableProperty SourceProperty = BindableProperty.Create(nameof(Source), typeof(string), typeof(ExtendedWebView));

    public static readonly BindableProperty ContentTypeProperty = BindableProperty.Create(nameof(ContentType), typeof(WebViewContentType), typeof(ExtendedWebView), WebViewContentType.Internet);

    public static readonly BindableProperty BaseUrlProperty = BindableProperty.Create(nameof(BaseUrl), typeof(string), typeof(ExtendedWebView));

    public static readonly BindableProperty CanGoBackProperty = BindableProperty.Create(nameof(CanGoBack), typeof(bool), typeof(ExtendedWebView), false);

    public static readonly BindableProperty CanGoForwardProperty = BindableProperty.Create(nameof(CanGoForward), typeof(bool), typeof(ExtendedWebView), false);

    public static readonly BindableProperty EnableGlobalCallbacksProperty = BindableProperty.Create(nameof(EnableGlobalCallbacks), typeof(bool), typeof(ExtendedWebView), true);

    public static readonly BindableProperty EnableGlobalHeadersProperty = BindableProperty.Create(nameof(EnableGlobalHeaders), typeof(bool), typeof(ExtendedWebView), true);

    public static readonly BindableProperty UseWideViewPortProperty = BindableProperty.Create(nameof(UseWideViewPort), typeof(bool), typeof(ExtendedWebView), false);

    public readonly static Dictionary<string, string> GlobalRegisteredHeaders = new Dictionary<string, string>();

    internal readonly static Dictionary<string, Action<string>> GlobalRegisteredCallbacks = new Dictionary<string, Action<string>>();

    public static void AddGlobalCallback(string functionName, Action<string> action)
    {
        if (string.IsNullOrWhiteSpace(functionName))
        {
            return;
        }

        if (GlobalRegisteredCallbacks.ContainsKey(functionName))
        {
            GlobalRegisteredCallbacks.Remove(functionName);
        }

        GlobalRegisteredCallbacks.Add(functionName, action);
        CallbackAdded?.Invoke(null, functionName);
    }

    public static void RemoveGlobalCallback(string functionName)
    {
        if (GlobalRegisteredCallbacks.ContainsKey(functionName))
        {
            GlobalRegisteredCallbacks.Remove(functionName);
        }
    }

    public static void RemoveAllGlobalCallbacks()
    {
        GlobalRegisteredCallbacks.Clear();
    }

    internal static string InjectedFunction
    {
        get
        {
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                return "function csharp(data){bridge.invokeAction(data);}";
            }
            else if (DeviceInfo.Platform == DevicePlatform.iOS || DeviceInfo.Platform == DevicePlatform.macOS)
            {
                return "function csharp(data){window.webkit.messageHandlers.invokeAction.postMessage(data);}";
            }
            else
            {
                return "function csharp(data){window.external.notify(data);}";
            }
        }
    }

    internal static string GenerateFunctionScript(string name)
    {
        return $"function {name}(str){{csharp(\"{{'action':'{name}','data':'\"+window.btoa(str)+\"'}}\");}}";
    }
}