# Plugin.Maui.CustomWebview

[![NuGet](https://img.shields.io/nuget/v/Plugin.Maui.CustomWebview.svg?label=NuGet)](https://www.nuget.org/packages/Plugin.Maui.CustomWebview/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Plugin.Maui.CustomWebview.svg)](https://www.nuget.org/packages/Plugin.Maui.CustomWebview/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET MAUI](https://img.shields.io/badge/.NET%20MAUI-9.0-blue.svg)](https://dotnet.microsoft.com/apps/maui)

`Plugin.Maui.CustomWebview` is a lightweight, cross-platform WebView plugin for .NET MAUI that extends the standard WebView control with enhanced functionality. It leverages native WebView components on Android and iOS to provide better control, JavaScript integration, and navigation handling.

## 🚀 Features

- ✅ **Cross-Platform Support** - Works seamlessly on Android and iOS
- 🔄 **JavaScript Bridge** - Two-way communication between C# and JavaScript
- 🎯 **Navigation Control** - Advanced navigation events and URL filtering
- 📝 **Multiple Content Types** - Support for web URLs, local files, and HTML strings
- 🔧 **Customizable Rendering** - Access to native WebView controls for advanced customization
- 🎨 **Event-Driven Architecture** - Rich set of events for navigation lifecycle
- 🔐 **Cookie Management** - Built-in cookie clearing functionality
- 🌐 **Custom Headers** - Support for custom HTTP headers
- ⚡ **High Performance** - Optimized for smooth scrolling and fast rendering

## 📦 Installation

### NuGet Package Manager

Available on [NuGet](http://www.nuget.org/packages/Plugin.Maui.CustomWebview).

**Via .NET CLI:**
```bash
dotnet add package Plugin.Maui.CustomWebview
```

**Via Package Manager Console:**
```powershell
Install-Package Plugin.Maui.CustomWebview
```

**Via Visual Studio:**
Search for `Plugin.Maui.CustomWebview` in the NuGet Package Manager UI.

### Supported Platforms

| Platform | Minimum Version Supported |
|----------|---------------------------|
| iOS      | 14.2+                     |
| Android  | 5.0 (API 21)+             |

## ⚙️ Setup

### 1. Register the Handler

Add the following code to your `MauiProgram.cs` file in the `CreateMauiApp` method:

```csharp
using Plugin.Maui.CustomWebview;
using Plugin.Maui.CustomWebview.Implementations;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureMauiHandlers(handlers =>
            {
#if ANDROID
                handlers.AddHandler(typeof(ExtendedWebView), typeof(CustomWebviewRenderer));
#endif
#if IOS
                handlers.AddHandler(typeof(ExtendedWebView), typeof(CustomWebviewRenderer));
#endif
            });

        return builder.Build();
    }
}
```

### 2. Add Namespace in XAML

Add the following namespace declaration to your XAML pages:

```xml
xmlns:webview="clr-namespace:Plugin.Maui.CustomWebview.Implementations;assembly=Plugin.Maui.CustomWebview"
```

## 📖 Usage

### Basic WebView for Internet Content

**XAML:**
```xml
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:webview="clr-namespace:Plugin.Maui.CustomWebview.Implementations;assembly=Plugin.Maui.CustomWebview"
             x:Class="YourApp.MainPage">
    
    <webview:ExtendedWebView x:Name="myWebView" 
                             Source="https://www.example.com"
                             ContentType="Internet" />
</ContentPage>
```

**C#:**
```csharp
using Plugin.Maui.CustomWebview.Implementations;
using Plugin.Maui.CustomWebview.Enums;

var webView = new ExtendedWebView
{
    ContentType = WebViewContentType.Internet,
    Source = "https://www.example.com"
};
```

### HTML String Content

```csharp
var webView = new ExtendedWebView
{
    ContentType = WebViewContentType.StringData,
    Source = @"
        <!DOCTYPE html>
        <html>
            <head>
                <title>My Page</title>
            </head>
            <body>
                <h1>Hello from HTML String!</h1>
                <button onclick='csharp(\"Button clicked!\")'>Click Me</button>
            </body>
        </html>"
};
```

### Local File Content

```csharp
var webView = new ExtendedWebView
{
    ContentType = WebViewContentType.LocalFile,
    Source = "index.html",
    BaseUrl = "file:///android_asset/" // Android
    // BaseUrl = "file://" + NSBundle.MainBundle.BundlePath + "/" // iOS
};
```

## 🎯 Advanced Features

### JavaScript Bridge - C# to JavaScript

Inject and execute JavaScript code from C#:

```csharp
// Inject JavaScript
await webView.InjectJavascriptAsync("alert('Hello from C#!');");

// Execute JavaScript and get result
var result = await webView.InjectJavascriptAsync("document.title");
```

### JavaScript Bridge - JavaScript to C#

Call C# methods from JavaScript:

**1. Register a callback in C#:**
```csharp
webView.AddLocalCallback("myFunction", (data) => 
{
    Debug.WriteLine($"Received from JavaScript: {data}");
    // Handle the callback
});
```

**2. Call from JavaScript:**
```javascript
// In your HTML/JavaScript
csharp('myFunction', JSON.stringify({ message: "Hello C#!" }));
```

**3. Remove callback when done:**
```csharp
webView.RemoveLocalCallback("myFunction");
// Or remove all callbacks
webView.RemoveAllLocalCallbacks();
```

### Navigation Events

Handle various navigation lifecycle events:

```csharp
// Fired when navigation starts (can be cancelled)
webView.OnNavigationStarted += (sender, e) =>
{
    Debug.WriteLine($"Navigating to: {e.Uri}");
    
    // Cancel navigation to specific URLs
    if (e.Uri.Contains("blocked-site.com"))
    {
        e.Cancel = true;
    }
    
    // Offload to external browser
    if (e.Uri.Contains("external-site.com"))
    {
        e.OffloadOntoDevice = true;
    }
};

// Fired when navigation completes
webView.OnNavigationCompleted += (sender, url) =>
{
    Debug.WriteLine($"Navigation completed: {url}");
};

// Fired when content (DOM) is loaded
webView.OnContentLoaded += (sender, e) =>
{
    Debug.WriteLine("DOM Ready - safe to inject JavaScript");
    // Best place to inject JavaScript
    webView.InjectJavascriptAsync("console.log('Page loaded');");
};

// Fired when navigation fails
webView.OnNavigationError += (sender, errorCode) =>
{
    Debug.WriteLine($"Navigation error: {errorCode}");
};
```

### Navigation Control

```csharp
// Check if can navigate
if (webView.CanGoBack)
{
    webView.GoBack();
}

if (webView.CanGoForward)
{
    webView.GoForward();
}

// Refresh current page
webView.Refresh();

// Check if currently navigating
bool isNavigating = webView.Navigating;
```

### Cookie Management

```csharp
// Clear all cookies
await webView.ClearCookiesAsync();
```

### Custom Headers

```csharp
// Enable global headers
webView.EnableGlobalHeaders = true;

// Add custom headers
webView.LocalRegisteredHeaders.Add("Authorization", "Bearer token123");
webView.LocalRegisteredHeaders.Add("Custom-Header", "value");
```

### Access Native Controls

For advanced customization, access the native WebView control:

```csharp
// In platform-specific code
CustomWebviewRenderer.OnControlChanged += (renderer) =>
{
#if ANDROID
    var nativeWebView = renderer.Control; // Android.Webkit.WebView
    // Customize Android WebView
    nativeWebView.Settings.SetSupportZoom(true);
#elif IOS
    var nativeWebView = renderer.Control; // WebKit.WKWebView
    // Customize iOS WKWebView
    nativeWebView.AllowsBackForwardNavigationGestures = true;
#endif
};
```

## 📋 Complete Example

```csharp
public partial class MainPage : ContentPage
{
    private ExtendedWebView webView;

    public MainPage()
    {
        InitializeComponent();
        SetupWebView();
    }

    private void SetupWebView()
    {
        webView = new ExtendedWebView
        {
            ContentType = WebViewContentType.Internet,
            Source = "https://www.example.com",
            EnableGlobalCallbacks = true,
            VerticalOptions = LayoutOptions.FillAndExpand,
            HorizontalOptions = LayoutOptions.FillAndExpand
        };

        // Register JavaScript callbacks
        webView.AddLocalCallback("showAlert", (message) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                DisplayAlert("Alert", message, "OK");
            });
        });

        webView.AddLocalCallback("logMessage", (message) =>
        {
            Debug.WriteLine($"JS Log: {message}");
        });

        // Navigation events
        webView.OnNavigationStarted += OnNavigationStarted;
        webView.OnNavigationCompleted += OnNavigationCompleted;
        webView.OnContentLoaded += OnContentLoaded;
        webView.OnNavigationError += OnNavigationError;

        Content = webView;
    }

    private void OnNavigationStarted(object sender, DecisionHandlers e)
    {
        Debug.WriteLine($"Navigation started: {e.Uri}");

        // Block certain domains
        if (e.Uri.Contains("blocked.com"))
        {
            e.Cancel = true;
            DisplayAlert("Blocked", "This site is blocked", "OK");
        }

        // Open external links in browser
        if (e.Uri.Contains("external.com"))
        {
            e.OffloadOntoDevice = true;
        }
    }

    private void OnNavigationCompleted(object sender, string url)
    {
        Debug.WriteLine($"Navigation completed: {url}");
    }

    private void OnContentLoaded(object sender, EventArgs e)
    {
        Debug.WriteLine("DOM Ready");
        
        // Inject JavaScript after page loads
        webView.InjectJavascriptAsync(@"
            document.getElementById('myButton')?.addEventListener('click', function() {
                csharp('showAlert', 'Button was clicked!');
            });
        ");
    }

    private void OnNavigationError(object sender, int errorCode)
    {
        Debug.WriteLine($"Navigation error: {errorCode}");
        DisplayAlert("Error", "Failed to load page", "OK");
    }
}
```

## 🔧 API Reference

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Source` | `string` | The URL, HTML string, or local file path to load |
| `ContentType` | `WebViewContentType` | Type of content: `Internet`, `StringData`, or `LocalFile` |
| `BaseUrl` | `string` | Base URL for resolving relative paths in local content |
| `EnableGlobalCallbacks` | `bool` | Enable global JavaScript callbacks |
| `EnableGlobalHeaders` | `bool` | Enable global HTTP headers |
| `Navigating` | `bool` | Indicates if the WebView is currently navigating (read-only) |
| `CanGoBack` | `bool` | Indicates if backward navigation is possible (read-only) |
| `CanGoForward` | `bool` | Indicates if forward navigation is possible (read-only) |
| `UseWideViewPort` | `bool` | Enable wide viewport support (Android) |

### Methods

| Method | Parameters | Returns | Description |
|--------|------------|---------|-------------|
| `AddLocalCallback` | `string functionName, Action<string> action` | `void` | Register a C# callback that can be called from JavaScript |
| `RemoveLocalCallback` | `string functionName` | `void` | Remove a specific callback |
| `RemoveAllLocalCallbacks` | - | `void` | Remove all registered callbacks |
| `InjectJavascriptAsync` | `string js` | `Task<string>` | Execute JavaScript and optionally get result |
| `GoBack` | - | `void` | Navigate to previous page |
| `GoForward` | - | `void` | Navigate to next page |
| `Refresh` | - | `void` | Reload current page |
| `ClearCookiesAsync` | - | `Task` | Clear all cookies |

### Events

| Event | EventArgs Type | Description |
|-------|----------------|-------------|
| `OnNavigationStarted` | `DecisionHandlers` | Fired when navigation starts. Can cancel or offload |
| `OnNavigationCompleted` | `string` | Fired when navigation completes successfully |
| `OnNavigationError` | `int` | Fired when navigation fails |
| `OnContentLoaded` | `EventArgs` | Fired when DOM is ready |

### Enums

**WebViewContentType**
- `Internet` - Load from internet URL
- `StringData` - Load from HTML string
- `LocalFile` - Load from local file

## 🛠️ Troubleshooting

### Common Issues

**1. WebView not displaying content**
- Ensure you've registered the handler in `MauiProgram.cs`
- Check that `ContentType` is set correctly before setting `Source`
- Verify internet permissions on Android (if loading from internet)

**2. JavaScript callbacks not working**
- Make sure `EnableGlobalCallbacks` is set to `true`
- Register callbacks before loading content
- Use `OnContentLoaded` event to ensure DOM is ready before injecting JavaScript

**3. Navigation events not firing**
- Attach event handlers before setting the `Source` property
- Ensure handlers are not being garbage collected

**4. Build errors about missing workloads**
```bash
# Install required .NET MAUI workloads
dotnet workload install maui
```

## 📱 Platform-Specific Notes

### Android
- Minimum API Level: 21 (Android 5.0)
- Uses `Android.Webkit.WebView`
- JavaScript is enabled by default
- No special permissions required for internet content

### iOS
- Minimum Version: iOS 14.2
- Uses `WebKit.WKWebView`
- Supports modern web standards
- No special permissions required

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

### Development Setup

1. Clone the repository
```bash
git clone https://github.com/mohammedmsadiq/Plugin.Maui.CustomWebview.git
```

2. Install .NET MAUI workloads
```bash
dotnet workload install maui
```

3. Open the solution
```bash
cd Plugin.Maui.CustomWebview
dotnet build
```

4. Run the sample app to test changes

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

### Version 1.0.x
- Initial release with core functionality
- Support for Android and iOS
- JavaScript bridge implementation
- Navigation event handling

## 🙏 Acknowledgements

This project was made possible thanks to:
- The .NET MAUI team for the excellent framework
- Contributors and users who provide feedback and improvements
- The open-source community

## 📞 Support

- 🐛 **Issues**: [GitHub Issues](https://github.com/mohammedmsadiq/Plugin.Maui.CustomWebview/issues)
- 💬 **Discussions**: [GitHub Discussions](https://github.com/mohammedmsadiq/Plugin.Maui.CustomWebview/discussions)
- 📧 **Contact**: Open an issue for support

## ⭐ Show Your Support

If this plugin helped you, please consider:
- ⭐ Starring the repository
- 🐛 Reporting bugs
- 💡 Suggesting new features
- 🤝 Contributing to the code

---

**Made with ❤️ for the .NET MAUI community**
