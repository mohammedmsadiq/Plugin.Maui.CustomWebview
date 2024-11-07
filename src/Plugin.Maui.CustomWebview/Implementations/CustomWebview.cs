using Newtonsoft.Json;
using Plugin.Maui.CustomWebview;
using Plugin.Maui.CustomWebview.Interfaces;
using Plugin.Maui.CustomWebview.Delegates;
using Plugin.Maui.CustomWebview.Enums;
using Plugin.Maui.CustomWebview.Models;
using System.Text;

namespace Plugin.Maui.CustomWebview.Implementations;

public partial class ExtendedWebView : View, ICustomWebview, IDisposable
{
    public delegate Task<string> JavascriptInjectionRequestDelegate(string js);

    public delegate Task ClearCookiesRequestDelegate();

    public event EventHandler<DecisionHandlers> OnNavigationStarted;

    public event EventHandler<string> OnNavigationCompleted;

    public event EventHandler<int> OnNavigationError;

    public event EventHandler OnContentLoaded;

    internal event EventHandler OnBackRequested;

    internal event EventHandler OnForwardRequested;

    internal event EventHandler OnRefreshRequested;

    internal event JavascriptInjectionRequestDelegate OnJavascriptInjectionRequest;

    internal event ClearCookiesRequestDelegate OnClearCookiesRequested;

    internal readonly Dictionary<string, Action<string>> LocalRegisteredCallbacks = new Dictionary<string, Action<string>>();

    public readonly Dictionary<string, string> LocalRegisteredHeaders = new Dictionary<string, string>();

    public WebViewContentType ContentType
    {
        get => (WebViewContentType)GetValue(ContentTypeProperty);
        set => SetValue(ContentTypeProperty, value);
    }

    public string Source
    {
        get => (string)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public string BaseUrl
    {
        get { return (string)GetValue(BaseUrlProperty); }
        set { SetValue(BaseUrlProperty, value); }
    }

    public bool EnableGlobalCallbacks
    {
        get => (bool)GetValue(EnableGlobalCallbacksProperty);
        set => SetValue(EnableGlobalCallbacksProperty, value);
    }

    public bool EnableGlobalHeaders
    {
        get => (bool)GetValue(EnableGlobalHeadersProperty);
        set => SetValue(EnableGlobalHeadersProperty, value);
    }

    public bool Navigating
    {
        get => (bool)GetValue(NavigatingProperty);
        internal set => SetValue(NavigatingProperty, value);
    }

    public bool CanGoBack
    {
        get => (bool)GetValue(CanGoBackProperty);
        internal set => SetValue(CanGoBackProperty, value);
    }

    public bool CanGoForward
    {
        get => (bool)GetValue(CanGoForwardProperty);
        internal set => SetValue(CanGoForwardProperty, value);
    }

    public bool UseWideViewPort
    {
        get => (bool)GetValue(UseWideViewPortProperty);
        set => SetValue(UseWideViewPortProperty, value);
    }
    WebViewContentType ICustomWebview.ContentType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    string ICustomWebview.Source { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    string ICustomWebview.BaseUrl { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    bool ICustomWebview.EnableGlobalCallbacks { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    bool ICustomWebview.EnableGlobalHeaders { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    bool ICustomWebview.Navigating => throw new NotImplementedException();

    bool ICustomWebview.CanGoBack => throw new NotImplementedException();

    bool ICustomWebview.CanGoForward => throw new NotImplementedException();

    public ExtendedWebView()
    {
        HorizontalOptions = VerticalOptions = LayoutOptions.FillAndExpand;
    }

    event EventHandler<DecisionHandlers> ICustomWebview.OnNavigationStarted
    {
        add
        {
            throw new NotImplementedException();
        }

        remove
        {
            throw new NotImplementedException();
        }
    }

    event EventHandler<string> ICustomWebview.OnNavigationCompleted
    {
        add
        {
            throw new NotImplementedException();
        }

        remove
        {
            throw new NotImplementedException();
        }
    }

    event EventHandler<int> ICustomWebview.OnNavigationError
    {
        add
        {
            throw new NotImplementedException();
        }

        remove
        {
            throw new NotImplementedException();
        }
    }

    event EventHandler ICustomWebview.OnContentLoaded
    {
        add
        {
            throw new NotImplementedException();
        }

        remove
        {
            throw new NotImplementedException();
        }
    }

    public void GoBack()
    {
        if (!CanGoBack)
        {
            return;
        }
        OnBackRequested?.Invoke(this, EventArgs.Empty);
    }

    public void GoForward()
    {
        if (!CanGoForward)
        {
            return;
        }
        OnForwardRequested?.Invoke(this, EventArgs.Empty);
    }

    public void Refresh()
    {
        OnRefreshRequested?.Invoke(this, EventArgs.Empty);
    }

    public async Task ClearCookiesAsync()
    {
        if (OnClearCookiesRequested != null)
        {
            await OnClearCookiesRequested.Invoke();
        }
    }

    public async Task<string> InjectJavascriptAsync(string js)
    {
        if (string.IsNullOrWhiteSpace(js))
        {
            return string.Empty;
        }

        if (OnJavascriptInjectionRequest != null)
        {
            return await OnJavascriptInjectionRequest.Invoke(js);
        }
        return string.Empty;
    }

    public void AddLocalCallback(string functionName, Action<string> action)
    {
        if (string.IsNullOrWhiteSpace(functionName))
        {
            return;
        }

        if (LocalRegisteredCallbacks.ContainsKey(functionName))
        {
            LocalRegisteredCallbacks.Remove(functionName);
        }

        LocalRegisteredCallbacks.Add(functionName, action);
        CallbackAdded?.Invoke(this, functionName);
    }

    public void RemoveLocalCallback(string functionName)
    {
        if (LocalRegisteredCallbacks.ContainsKey(functionName))
        {
            LocalRegisteredCallbacks.Remove(functionName);
        }
    }

    public void RemoveAllLocalCallbacks()
    {
        LocalRegisteredCallbacks.Clear();
    }

    public void Dispose()
    {
        LocalRegisteredCallbacks.Clear();
        LocalRegisteredHeaders.Clear();
    }

    #region Internals

    internal DecisionHandlers HandleNavigationStartRequest(string uri)
    {
        // By default, we only attempt to offload valid Uris with none http/s schemes
        bool validUri = Uri.TryCreate(uri, UriKind.Absolute, out Uri uriResult);
        bool validScheme = false;

        if (validUri)
        {
            validScheme = uriResult.Scheme.StartsWith("http") || uriResult.Scheme.StartsWith("file");
        }
        var handler = new DecisionHandlers()
        {
            Uri = uri,
            OffloadOntoDevice = validUri && !validScheme
        };

        OnNavigationStarted?.Invoke(this, handler);
        return handler;
    }

    internal void HandleNavigationCompleted(string uri)
    {
        OnNavigationCompleted?.Invoke(this, uri);
    }

    internal void HandleNavigationError(int errorCode)
    {
        OnNavigationError?.Invoke(this, errorCode);
    }

    internal void HandleContentLoaded()
    {
        OnContentLoaded?.Invoke(this, EventArgs.Empty);
    }

    internal void HandleScriptReceived(string data)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            return;
        }
        var action = JsonConvert.DeserializeObject<ActionEventModel>(data);

        byte[] dBytes = Convert.FromBase64String(action.Data);
        action.Data = Encoding.UTF8.GetString(dBytes, 0, dBytes.Length);

        if (LocalRegisteredCallbacks.ContainsKey(action.Action))
        {
            LocalRegisteredCallbacks[action.Action]?.Invoke(action.Data);
        }
        else if (GlobalRegisteredCallbacks.ContainsKey(action.Action))
        {
            GlobalRegisteredCallbacks[action.Action]?.Invoke(action.Data);
        }
    }

    void ICustomWebview.GoBack()
    {
        throw new NotImplementedException();
    }

    void ICustomWebview.GoForward()
    {
        throw new NotImplementedException();
    }

    void ICustomWebview.Refresh()
    {
        throw new NotImplementedException();
    }

    Task<string> ICustomWebview.InjectJavascriptAsync(string js)
    {
        throw new NotImplementedException();
    }

    void ICustomWebview.AddLocalCallback(string functionName, Action<string> action)
    {
        throw new NotImplementedException();
    }

    void ICustomWebview.RemoveLocalCallback(string functionName)
    {
        throw new NotImplementedException();
    }

    void ICustomWebview.RemoveAllLocalCallbacks()
    {
        throw new NotImplementedException();
    }

    Task ICustomWebview.ClearCookiesAsync()
    {
        throw new NotImplementedException();
    }

    #endregion
}