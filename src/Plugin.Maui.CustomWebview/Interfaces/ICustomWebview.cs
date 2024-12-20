using Plugin.Maui.CustomWebview.Delegates;
using Plugin.Maui.CustomWebview.Enums;

namespace Plugin.Maui.CustomWebview.Interfaces;

public interface ICustomWebview
{
    event EventHandler<DecisionHandlers> OnNavigationStarted;

    event EventHandler<string> OnNavigationCompleted;

    event EventHandler<int> OnNavigationError;

    event EventHandler OnContentLoaded;

    WebViewContentType ContentType { get; set; }

    string Source { get; set; }

    string BaseUrl { get; set; }

    bool EnableGlobalCallbacks { get; set; }

    bool EnableGlobalHeaders { get; set; }

    bool Navigating { get; }

    bool CanGoBack { get; }

    bool CanGoForward { get; }

    void GoBack();

    void GoForward();

    void Refresh();

    Task<string> InjectJavascriptAsync(string js);

    void AddLocalCallback(string functionName, Action<string> action);

    void RemoveLocalCallback(string functionName);

    void RemoveAllLocalCallbacks();
    Task ClearCookiesAsync();
}