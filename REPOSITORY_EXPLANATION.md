# Plugin.Maui.CustomWebview - Repository Explanation

## Table of Contents
1. [Overview](#overview)
2. [Repository Structure](#repository-structure)
3. [Architecture](#architecture)
4. [Core Components](#core-components)
5. [Platform-Specific Implementations](#platform-specific-implementations)
6. [Key Features](#key-features)
7. [How It Works](#how-it-works)
8. [Usage Examples](#usage-examples)
9. [Build and Development](#build-and-development)
10. [Contributing](#contributing)

---

## Overview

**Plugin.Maui.CustomWebview** is a lightweight cross-platform WebView plugin for .NET MAUI (Multi-platform App UI) applications. It provides enhanced functionality over the standard WebView control by leveraging native WebView components on Android and iOS platforms.

### Purpose
The plugin bridges the gap between .NET MAUI applications and web content, offering:
- **JavaScript-to-C# communication**: Call C# code from JavaScript running in the WebView
- **C#-to-JavaScript injection**: Execute JavaScript code from your C# application
- **Navigation control**: Handle navigation events and control navigation flow
- **Enhanced WebView features**: Access platform-specific WebView capabilities

### Target Platforms
- **iOS**: Version 14.2 and above (Note: README mentions iOS 11+, but the csproj specifies 14.2 as the minimum)
- **Android**: API Level 21 (Android 5.0) and above
- **.NET**: Built with .NET 8.0

---

## Repository Structure

```
Plugin.Maui.CustomWebview/
├── .github/
│   └── workflows/          # CI/CD pipelines
│       ├── ci.yml          # Main CI pipeline
│       ├── ci-sample.yml   # Sample app CI
│       └── release-nuget.yml # NuGet release automation
├── samples/
│   └── Plugin.Maui.CustomWebview.Sample/  # Sample application
│       ├── MainPage.xaml                   # Demo UI
│       ├── MainPage.xaml.cs               # Demo code
│       └── MauiProgram.cs                 # App configuration
├── src/
│   └── Plugin.Maui.CustomWebview/         # Main plugin library
│       ├── Delegates/                      # Event delegates
│       ├── Enums/                          # Enumerations
│       ├── Implementations/                # Core implementations
│       ├── Interfaces/                     # Public interfaces
│       ├── Models/                         # Data models
│       └── Platforms/                      # Platform-specific code
│           ├── Android/                    # Android implementation
│           └── iOS/                        # iOS implementation
├── LICENSE                 # MIT License
├── README.md              # User documentation
└── nuget.png             # Package icon
```

---

## Architecture

### Design Pattern
The plugin follows a **Handler-based architecture** pattern used in .NET MAUI:

1. **Cross-Platform Layer**: Defines the API surface and behavior
2. **Platform Handlers**: Implement platform-specific rendering and functionality
3. **Native Components**: Wrap native WebView controls (WKWebView on iOS, WebView on Android)

### Key Architecture Components

```
┌─────────────────────────────────────────┐
│   ExtendedWebView (Cross-Platform)      │
│   - Public API                           │
│   - Events & Properties                  │
│   - Callback Management                  │
└──────────────┬──────────────────────────┘
               │
       ┌───────┴────────┐
       ↓                ↓
┌──────────────┐  ┌──────────────┐
│   Android    │  │     iOS      │
│   Handler    │  │   Handler    │
└──────┬───────┘  └──────┬───────┘
       ↓                  ↓
┌──────────────┐  ┌──────────────┐
│Android WebView│ │  WKWebView   │
│  (Native)    │  │  (Native)    │
└──────────────┘  └──────────────┘
```

---

## Core Components

### 1. ExtendedWebView (Main Control)

**Location**: `src/Plugin.Maui.CustomWebview/Implementations/CustomWebview.cs`

The primary control that developers interact with. It extends MAUI's `View` class and implements `ICustomWebview`.

**Key Properties**:
- `Source`: URL or HTML content to display
- `ContentType`: Type of content (Internet, StringData, LocalFile)
- `BaseUrl`: Base URL for resolving relative paths
- `Navigating`: Indicates if navigation is in progress
- `CanGoBack`/`CanGoForward`: Navigation state
- `EnableGlobalCallbacks`: Enable/disable global JavaScript callbacks
- `EnableGlobalHeaders`: Enable/disable global HTTP headers
- `UseWideViewPort`: Android-specific viewport setting

**Key Events**:
- `OnNavigationStarted`: Fired when navigation begins (can be cancelled)
- `OnNavigationCompleted`: Fired when navigation completes
- `OnNavigationError`: Fired on navigation errors
- `OnContentLoaded`: Fired when DOM is ready (ideal for JavaScript injection)

**Key Methods**:
- `InjectJavascriptAsync(string js)`: Execute JavaScript code
- `AddLocalCallback(string name, Action<string> action)`: Register C# callback for JavaScript
- `RemoveLocalCallback(string name)`: Unregister callback
- `ClearCookiesAsync()`: Clear WebView cookies
- `GoBack()`/`GoForward()`/`Refresh()`: Navigation controls

### 2. WebView.Shared (Static Functionality)

**Location**: `src/Plugin.Maui.CustomWebview/Implementations/WebView.Shared.cs`

Contains shared static properties, bindable properties, and global callback management.

**Global Features**:
- Global callbacks: Callbacks shared across all WebView instances
- Global headers: HTTP headers applied to all requests
- Bindable properties: MAUI-specific property definitions

### 3. Content Types (Enum)

**Location**: `src/Plugin.Maui.CustomWebview/Enums/ContentType.cs`

```csharp
public enum WebViewContentType
{
    Internet = 0,     // Load from URL (http/https)
    StringData = 1,   // Load HTML string
    LocalFile = 2     // Load local file
}
```

### 4. DecisionHandlers (Navigation Control)

**Location**: `src/Plugin.Maui.CustomWebview/Delegates/DecisionHandlers.cs`

Used in navigation events to control navigation behavior:
- `Uri`: The target URI
- `Cancel`: Set to true to prevent navigation
- `OffloadOntoDevice`: Set to true to open in external browser

---

## Platform-Specific Implementations

### Android Implementation

**Location**: `src/Plugin.Maui.CustomWebview/Platforms/Android/`

**Key Files**:
1. **CustomWebviewRenderer.cs**: Main renderer that creates and manages Android WebView
2. **Client.cs**: WebViewClient for handling navigation events
3. **ChromeClient.cs**: WebChromeClient for handling JavaScript dialogs and progress
4. **Bridge.cs**: JavaScript-to-C# bridge using `@JavascriptInterface`
5. **JavascriptValueCallback.cs**: Callback for JavaScript execution results

**How It Works**:
- Uses Android's `WebView` class
- JavaScript calls C# via a bridge object: `bridge.invokeAction(data)`
- Injects the bridge using `AddJavascriptInterface()`
- Handles SSL, cookies, and download events
- Supports custom user agents and headers

### iOS Implementation

**Location**: `src/Plugin.Maui.CustomWebview/Platforms/iOS/`

**Key Files**:
1. **CustomWebviewRenderer.cs**: Main renderer using WKWebView
2. **NavigationDelegate.cs**: WKNavigationDelegate for navigation events
3. **MyWebviewRenderer.cs**: Additional renderer utilities

**How It Works**:
- Uses iOS's `WKWebView` class (modern WebKit-based WebView)
- JavaScript calls C# via message handlers: `window.webkit.messageHandlers.invokeAction.postMessage(data)`
- Uses `WKUserContentController` to register message handlers
- Supports JavaScript injection via `WKUserScript`
- Handles cookies via `WKHTTPCookieStore`

---

## Key Features

### 1. JavaScript-to-C# Communication

The plugin creates a bi-directional bridge between JavaScript and C#:

**JavaScript Side**:
```javascript
// Automatically injected function
csharp('your data here');

// Or use registered callbacks
myCallback('data');
```

**C# Side**:
```csharp
webView.AddLocalCallback("myCallback", (data) => {
    Debug.WriteLine($"Received from JS: {data}");
});
```

**Internal Mechanism**:
1. JavaScript calls are serialized as JSON with action name and base64-encoded data
2. Platform-specific bridge receives the call
3. Data is deserialized and routed to the appropriate callback
4. Both local (instance-specific) and global (app-wide) callbacks are supported

### 2. C#-to-JavaScript Injection

Execute JavaScript code from C#:

```csharp
await webView.InjectJavascriptAsync("alert('Hello from C#');");
string result = await webView.InjectJavascriptAsync("document.title");
```

### 3. Navigation Control

Intercept and control navigation:

```csharp
webView.OnNavigationStarted += (sender, e) => {
    // Block navigation to certain sites
    if (e.Uri.Contains("blocked.com"))
        e.Cancel = true;
    
    // Open in external browser
    if (e.Uri.Contains("external.com"))
        e.OffloadOntoDevice = true;
};
```

### 4. Content Loading Modes

Support for three content types:
- **Internet**: Load from web URLs
- **StringData**: Load HTML strings directly
- **LocalFile**: Load local HTML files

### 5. Cookie Management

Clear cookies across platforms:

```csharp
await webView.ClearCookiesAsync();
```

---

## How It Works

### Initialization Flow

1. **Application Startup**: Register handlers in `MauiProgram.cs`
   ```csharp
   .ConfigureMauiHandlers(handlers => {
       handlers.AddHandler(typeof(ExtendedWebView), typeof(CustomWebviewRenderer));
   });
   ```

2. **Control Creation**: When ExtendedWebView is created in XAML/code
   - MAUI creates the platform handler
   - Handler creates native WebView (Android WebView or iOS WKWebView)

3. **Setup Phase**:
   - Configure WebView settings
   - Inject JavaScript bridge code
   - Register callbacks and event handlers
   - Set up navigation delegates

4. **Runtime**:
   - Navigation events flow from native to managed code
   - JavaScript calls are routed through the bridge
   - C# code can inject JavaScript and control navigation

### JavaScript Bridge Mechanism

**Data Flow**:
```
JavaScript → JSON Serialization → Native Bridge → Deserialization → C# Callback
```

**Message Format**:
```json
{
  "action": "callbackName",
  "data": "base64EncodedData"
}
```

**Platform-Specific Bridges**:
- **Android**: Uses `@JavascriptInterface` annotation
- **iOS**: Uses `WKScriptMessageHandler` protocol

---

## Usage Examples

### Basic Internet Content

```csharp
var webView = new ExtendedWebView {
    ContentType = WebContentType.Internet,
    Source = "https://www.example.com"
};
```

### HTML String Content

```csharp
var webView = new ExtendedWebView {
    ContentType = WebContentType.StringData,
    Source = "<html><body><h1>Hello World</h1></body></html>"
};
```

### JavaScript Integration

```csharp
// Register callback
webView.AddLocalCallback("test", (data) => {
    Console.WriteLine($"Received: {data}");
});

// Inject JavaScript on content loaded
webView.OnContentLoaded += async (sender, e) => {
    await webView.InjectJavascriptAsync("test('Hello from page')");
};
```

### Navigation Interception

```csharp
webView.OnNavigationStarted += (sender, e) => {
    if (e.Uri.Contains("mailto:")) {
        e.OffloadOntoDevice = true; // Open in mail app
    }
};
```

### Complete Example (from Sample App)

```csharp
public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        // HTML content
        stringContent.Source = @"
        <!doctype html>
        <html>
            <body>
                <h1>Custom WebView Demo</h1>
                <a href='mailto:someone@example.com'>Send Email</a>
                <a href='https://www.bbc.co.uk'>External Link</a>
            </body>
        </html>";
    }

    private void OnNavigationStarted(object sender, DecisionHandlers e)
    {
        // Open BBC links in external browser
        if (e.Uri.Contains("bbc.co.uk"))
            e.OffloadOntoDevice = true;
    }
}
```

---

## Build and Development

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code with C# extension
- Android SDK (for Android development)
- Xcode (for iOS development, macOS only)

### Building the Plugin

```bash
# Restore dependencies
dotnet restore

# Build the plugin
dotnet build src/Plugin.Maui.CustomWebview/Plugin.Maui.CustomWebview.csproj

# Pack as NuGet package
dotnet pack src/Plugin.Maui.CustomWebview/Plugin.Maui.CustomWebview.csproj
```

### Running the Sample

```bash
# Navigate to sample directory
cd samples/Plugin.Maui.CustomWebview.Sample

# Run on Android
dotnet build -t:Run -f net8.0-android

# Run on iOS (macOS only)
dotnet build -t:Run -f net8.0-ios
```

### CI/CD Pipelines

The repository includes three GitHub Actions workflows:

1. **ci.yml**: Main continuous integration
   - Builds the plugin library
   - Runs on push and pull requests

2. **ci-sample.yml**: Sample app verification
   - Builds and tests the sample application
   - Ensures examples stay working

3. **release-nuget.yml**: NuGet package publishing
   - Automated package publishing on version tags
   - Publishes to NuGet.org

---

## Contributing

### Code Organization

- **Keep platform code isolated**: Platform-specific code goes in `Platforms/Android` or `Platforms/iOS`
- **Shared logic in Implementations**: Cross-platform logic stays in the `Implementations` folder
- **Public API in Interfaces**: Public contracts defined in `Interfaces`

### Testing Approach

Test changes using the sample application:
1. Make changes to the plugin
2. Run the sample app on target platforms
3. Verify functionality works as expected

### Release Process

1. Update version in `.csproj` file
2. Update CHANGELOG (if exists)
3. Create a git tag (e.g., `v1.0.7`)
4. Push tag to trigger automated release

---

## Technical Notes

### Why Two WebView Properties?

The `ExtendedWebView` class implements `ICustomWebview` interface explicitly, which creates separate implementations for interface members. This is likely legacy code that could be refactored. Most of these interface implementations throw `NotImplementedException`.

### Global vs Local Callbacks

- **Local callbacks**: Specific to a WebView instance
- **Global callbacks**: Shared across all WebView instances in the app
- Useful for common functionality like analytics or logging

### Security Considerations

- SSL errors can be ignored globally (Android): `IgnoreSSLGlobally = true` (use with caution)
- JavaScript execution: Only inject trusted JavaScript code
- Navigation control: Always validate and sanitize URIs in navigation events
- Cookie management: Clear cookies when handling sensitive data

### Performance Considerations

- JavaScript injection is async: Always await the result
- Callbacks are invoked on UI thread: Keep handlers lightweight
- Large HTML strings: Consider using LocalFile mode for better performance

---

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Author

Mohammed Sadiq

## NuGet Package

Available on NuGet: [Plugin.Maui.CustomWebview](http://www.nuget.org/packages/Plugin.Maui.CustomWebview)

```bash
dotnet add package Plugin.Maui.CustomWebview
```

---

## Summary

**Plugin.Maui.CustomWebview** is a production-ready, cross-platform WebView plugin that extends .NET MAUI's capabilities by providing:

- ✅ Bi-directional JavaScript ↔ C# communication
- ✅ Advanced navigation control
- ✅ Platform-specific optimizations
- ✅ Cookie management
- ✅ Multiple content loading modes
- ✅ Clean, maintainable architecture

The plugin follows MAUI's handler pattern and provides a consistent API across platforms while leveraging native WebView capabilities for optimal performance.
