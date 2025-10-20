# Developer Guide - Plugin.Maui.CustomWebview

## Table of Contents

1. [Getting Started](#getting-started)
2. [Development Environment Setup](#development-environment-setup)
3. [Project Structure Walkthrough](#project-structure-walkthrough)
4. [Building and Testing](#building-and-testing)
5. [Debugging Techniques](#debugging-techniques)
6. [Common Development Tasks](#common-development-tasks)
7. [Troubleshooting](#troubleshooting)
8. [Best Practices](#best-practices)
9. [Contributing Guidelines](#contributing-guidelines)

---

## Getting Started

### Prerequisites

**Required**:
- .NET 8.0 SDK or later
- Git

**Platform-Specific**:
- **Windows**: Visual Studio 2022 (17.8+) with .NET MAUI workload
- **macOS**: Visual Studio for Mac or VS Code with C# extension, Xcode 15+
- **Android**: Android SDK (API 21+)
- **iOS**: Xcode 15+, macOS only (minimum iOS 14.2 based on .csproj configuration)

### Clone and Initial Build

```bash
# Clone the repository
git clone https://github.com/mohammedmsadiq/Plugin.Maui.CustomWebview.git
cd Plugin.Maui.CustomWebview

# Install .NET MAUI workloads (required)
dotnet workload install maui

# Restore dependencies
dotnet restore

# Build the plugin (requires MAUI workloads)
dotnet build src/Plugin.Maui.CustomWebview/Plugin.Maui.CustomWebview.csproj

# Build the sample
dotnet build samples/Plugin.Maui.CustomWebview.Sample/Plugin.Maui.CustomWebview.Sample.csproj
```

---

## Development Environment Setup

### Visual Studio 2022 (Windows)

1. Install Visual Studio 2022 with workloads:
   - .NET Multi-platform App UI development
   - Mobile development with .NET

2. Open solution:
   ```
   Plugin.Maui.CustomWebview.sln (main plugin)
   or
   samples/Plugin.Maui.CustomWebview.Sample.sln (sample app)
   ```

3. Set startup project to Sample app
4. Select target platform (Android/iOS)
5. Press F5 to run

### Visual Studio Code

1. Install extensions:
   - C# for Visual Studio Code
   - .NET MAUI (optional but helpful)

2. Open workspace:
   ```bash
   code .
   ```

3. Build and run:
   ```bash
   # Install MAUI workloads first
   dotnet workload install maui
   
   # Build
   dotnet build
   
   # Run (use -t:Run target)
   dotnet build -t:Run --project samples/Plugin.Maui.CustomWebview.Sample/Plugin.Maui.CustomWebview.Sample.csproj -f net8.0-android
   ```

### macOS Development

For iOS development:

```bash
# Ensure Xcode is installed
xcode-select --install

# Ensure MAUI workloads are installed
dotnet workload install maui

# Build for iOS simulator
dotnet build samples/Plugin.Maui.CustomWebview.Sample/Plugin.Maui.CustomWebview.Sample.csproj -f net8.0-ios

# Run on iOS simulator (use -t:Run target instead of dotnet run)
dotnet build -t:Run samples/Plugin.Maui.CustomWebview.Sample/Plugin.Maui.CustomWebview.Sample.csproj -f net8.0-ios
```

---

## Project Structure Walkthrough

### Source Code Organization

```
src/Plugin.Maui.CustomWebview/
│
├── Delegates/
│   └── DecisionHandlers.cs           # Navigation decision model
│
├── Enums/
│   └── ContentType.cs                 # WebView content types enum
│
├── Implementations/
│   ├── CustomWebview.cs              # Main ExtendedWebView class
│   └── WebView.Shared.cs             # Shared static functionality
│
├── Interfaces/
│   └── ICustomWebview.cs             # Public interface contract
│
├── Models/
│   └── ActionEventModel.cs           # JavaScript callback data model
│
└── Platforms/
    ├── Android/
    │   ├── Bridge.cs                 # JavaScript-to-C# bridge
    │   ├── ChromeClient.cs           # Chrome client for dialogs
    │   ├── Client.cs                 # WebViewClient for navigation
    │   ├── CustomWebviewRenderer.cs  # Main Android renderer
    │   └── JavascriptValueCallback.cs # JS execution callback
    │
    └── iOS/
        ├── CustomWebviewRenderer.cs  # Main iOS renderer
        ├── MyWebviewRenderer.cs      # Additional renderer utilities
        └── NavigationDelegate.cs     # WKNavigationDelegate implementation
```

### Key Files Deep Dive

#### 1. `CustomWebview.cs` (Cross-Platform)

**Purpose**: Main control that developers use in their apps

**Key sections**:
```csharp
// Public properties (lines 39-91)
public WebViewContentType ContentType { get; set; }
public string Source { get; set; }
// ... etc

// Public methods (lines 161-228)
public void GoBack() { }
public void AddLocalCallback(string name, Action<string> action) { }
// ... etc

// Internal event handlers (lines 243-298)
internal DecisionHandlers HandleNavigationStartRequest(string uri) { }
internal void HandleScriptReceived(string data) { }
// ... etc
```

**Note**: Lines 92-159 and 299-339 contain `NotImplementedException` - these are explicit interface implementations that appear to be legacy code.

#### 2. `WebView.Shared.cs` (Cross-Platform)

**Purpose**: Static/shared functionality across all WebView instances

**Key sections**:
```csharp
// Bindable properties (lines 11-27)
public static readonly BindableProperty SourceProperty = ...

// Global dictionaries (lines 29-31)
public readonly static Dictionary<string, string> GlobalRegisteredHeaders
internal readonly static Dictionary<string, Action<string>> GlobalRegisteredCallbacks

// Static methods (lines 33-61)
public static void AddGlobalCallback(string name, Action<string> action)

// JavaScript generation (lines 62-84)
internal static string InjectedFunction { get; }
internal static string GenerateFunctionScript(string name)
```

#### 3. `CustomWebviewRenderer.cs` (Android)

**Purpose**: Android-specific rendering and native WebView management

**Key methods**:
- `SetupControl()`: Creates and configures Android WebView
- `SetupElement()`: Wires up event handlers
- `InjectJavascript()`: Executes JavaScript on Android WebView
- `LoadContent()`: Handles different content types

**Native integrations**:
- `WebViewClient`: Navigation interception
- `WebChromeClient`: Progress, console messages, dialogs
- `Bridge`: JavaScript-to-C# communication via `@JavascriptInterface`

#### 4. `CustomWebviewRenderer.cs` (iOS)

**Purpose**: iOS-specific rendering using WKWebView

**Key methods**:
- `SetupControl()`: Creates WKWebView with configuration
- `SetupElement()`: Event handler registration
- `OnJavascriptInjectionRequest()`: JavaScript execution on iOS
- `DidReceiveScriptMessage()`: Handles JavaScript-to-C# calls

**Native integrations**:
- `WKNavigationDelegate`: Navigation events
- `WKUserContentController`: JavaScript message handlers
- `WKScriptMessageHandler`: Receives JavaScript messages

---

## Building and Testing

### Building the Plugin

```bash
# Debug build
dotnet build src/Plugin.Maui.CustomWebview/Plugin.Maui.CustomWebview.csproj

# Release build
dotnet build src/Plugin.Maui.CustomWebview/Plugin.Maui.CustomWebview.csproj -c Release

# Create NuGet package
dotnet pack src/Plugin.Maui.CustomWebview/Plugin.Maui.CustomWebview.csproj -c Release
```

Output: `bin/Release/Plugin.Maui.CustomWebview.{version}.nupkg`

### Building for Specific Platforms

```bash
# Android only
dotnet build -f net8.0-android

# iOS only (macOS required)
dotnet build -f net8.0-ios
```

### Running the Sample App

```bash
# Android (emulator or device)
cd samples/Plugin.Maui.CustomWebview.Sample
dotnet build -t:Run -f net8.0-android

# iOS (simulator, macOS required)
dotnet build -t:Run -f net8.0-ios

# Specific device
dotnet build -t:Run -f net8.0-android -p:AndroidDevice=<device-id>
```

### Testing Changes

Since there are no automated unit tests in this project, testing is manual:

1. Make changes to the plugin code
2. Build the plugin
3. Run the sample app
4. Test functionality:
   - Internet content loading
   - String content loading
   - JavaScript injection
   - JavaScript-to-C# callbacks
   - Navigation control
   - Cookie clearing

**Test Checklist**:
- [ ] Load internet content (YouTube example)
- [ ] Load HTML string content
- [ ] Test JavaScript injection after content loaded
- [ ] Test JavaScript callback to C#
- [ ] Test navigation interception (mailto: links)
- [ ] Test external navigation (OffloadOntoDevice)
- [ ] Test back/forward navigation
- [ ] Test refresh
- [ ] Test cookie clearing

---

## Debugging Techniques

### Debugging C# Code

**Visual Studio**:
1. Set breakpoints in plugin or sample code
2. Run in Debug mode (F5)
3. Step through code (F10, F11)

**Common breakpoint locations**:
- `CustomWebview.cs` line 244: `HandleNavigationStartRequest()`
- `CustomWebview.cs` line 278: `HandleScriptReceived()`
- Platform renderers: `OnElementChanged()`, `SetupElement()`

### Debugging JavaScript

**Chrome DevTools for Android**:
1. Enable USB debugging on Android device
2. Open Chrome and navigate to `chrome://inspect`
3. Find your app's WebView
4. Click "inspect" to open DevTools

**Safari Web Inspector for iOS**:
1. Enable Web Inspector on iOS device (Settings > Safari > Advanced)
2. Connect device to Mac
3. Open Safari > Develop > [Your Device] > [WebView]

### Logging

Add debug logging:

```csharp
// In plugin code
System.Diagnostics.Debug.WriteLine($"Navigation started: {uri}");

// In sample app
webView.OnNavigationStarted += (sender, e) => {
    System.Diagnostics.Debug.WriteLine($"Nav started: {e.Uri}");
};
```

View logs:
- **Android**: `adb logcat` or Visual Studio Output window
- **iOS**: Xcode Console or Visual Studio Output window

### Platform-Specific Debugging

**Android**:
```bash
# View all logs
adb logcat

# Filter by tag
adb logcat -s "Mono"

# Clear logs
adb logcat -c
```

**iOS**:
```bash
# View device logs
idevicesyslog

# Or use Xcode Devices window
```

---

## Common Development Tasks

### Adding a New Property

1. Add property to `ExtendedWebView`:
```csharp
// CustomWebview.cs
public bool MyNewProperty
{
    get => (bool)GetValue(MyNewPropertyProperty);
    set => SetValue(MyNewPropertyProperty, value);
}
```

2. Add bindable property to `WebView.Shared.cs`:
```csharp
// WebView.Shared.cs
public static readonly BindableProperty MyNewPropertyProperty = 
    BindableProperty.Create(nameof(MyNewProperty), typeof(bool), 
                            typeof(ExtendedWebView), false);
```

3. Handle property change in renderers:
```csharp
// Android/CustomWebviewRenderer.cs
void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
{
    if (e.PropertyName == nameof(Element.MyNewProperty))
    {
        // Handle on Android
    }
}

// iOS/CustomWebviewRenderer.cs (similar)
```

### Adding a New Event

1. Declare event in `ExtendedWebView`:
```csharp
public event EventHandler<MyEventArgs> OnMyEvent;
```

2. Add internal handler method:
```csharp
internal void HandleMyEvent(MyEventArgs args)
{
    OnMyEvent?.Invoke(this, args);
}
```

3. Trigger from platform renderers when appropriate

### Adding a New Method

1. Add to `ICustomWebview` interface:
```csharp
Task MyNewMethodAsync();
```

2. Implement in `ExtendedWebView`:
```csharp
public async Task MyNewMethodAsync()
{
    // Implementation
}
```

3. Add platform-specific logic in renderers if needed

### Supporting a New Content Type

1. Add enum value:
```csharp
// Enums/ContentType.cs
public enum WebViewContentType
{
    Internet = 0,
    StringData = 1,
    LocalFile = 2,
    NewType = 3  // Add this
}
```

2. Handle in renderers:
```csharp
void LoadContent(string content, WebViewContentType contentType)
{
    switch (contentType)
    {
        case WebViewContentType.NewType:
            // Handle new type
            break;
    }
}
```

---

## Troubleshooting

### Common Issues

#### Issue: WebView not rendering

**Symptoms**: Blank WebView in sample app

**Solutions**:
1. Check internet permission (Android)
2. Verify handler registration in `MauiProgram.cs`
3. Check Source property is set
4. Verify ContentType matches Source

#### Issue: JavaScript callbacks not working

**Symptoms**: JavaScript calls `csharp()` but C# callback not invoked

**Solutions**:
1. Ensure callback registered before content loads
2. Check callback name matches exactly
3. Verify JavaScript bridge injected (check OnContentLoaded fires)
4. Check data format: `{"action":"name","data":"base64"}`

#### Issue: Navigation events not firing

**Symptoms**: OnNavigationStarted not called

**Solutions**:
1. Verify event handler attached before navigation
2. Check Source property change triggers navigation
3. Ensure renderer properly set up

#### Issue: Platform-specific build errors

**Android**:
- Missing Android SDK: Install via Visual Studio or Android Studio
- API level mismatch: Check `SupportedOSPlatformVersion` in .csproj

**iOS**:
- Xcode not found: Install Xcode from App Store
- Provisioning issues: Configure signing in project properties

### Build Errors

#### Error: "Cannot resolve reference"

```bash
# Clear and restore
dotnet clean
dotnet restore
dotnet build
```

#### Error: "Target framework not found"

Ensure .NET 8.0 SDK is installed:
```bash
dotnet --list-sdks
```

### Runtime Errors

#### Error: "NotImplementedException"

This occurs when using explicit interface members. Use the class properties/methods directly, not through the interface:

```csharp
// DON'T
ICustomWebview webView = new ExtendedWebView();
webView.GoBack(); // Throws NotImplementedException

// DO
ExtendedWebView webView = new ExtendedWebView();
webView.GoBack(); // Works
```

---

## Best Practices

### Code Style

Follow existing code style:
- Use 4 spaces for indentation
- Opening braces on same line
- Use explicit types (not `var`) for clarity
- Add XML comments for public APIs

Example:
```csharp
/// <summary>
/// Navigates back in the WebView history
/// </summary>
public void GoBack()
{
    if (!CanGoBack)
    {
        return;
    }
    OnBackRequested?.Invoke(this, EventArgs.Empty);
}
```

### Testing Changes

1. **Test on both platforms**: Always test Android and iOS
2. **Test all content types**: Internet, StringData, LocalFile
3. **Test edge cases**: 
   - Empty Source
   - Invalid URLs
   - Large content
   - Rapid navigation changes
4. **Test cleanup**: Ensure proper disposal, no memory leaks

### Version Control

- Commit focused changes
- Write clear commit messages
- Don't commit generated files (bin/, obj/, etc.)
- Update version in .csproj for releases

### Documentation

- Update README.md for new features
- Add XML comments for public APIs
- Update sample app to demonstrate new features
- Add comments for complex logic

---

## Contributing Guidelines

### Workflow

1. **Fork the repository**
2. **Create a feature branch**:
   ```bash
   git checkout -b feature/my-new-feature
   ```
3. **Make changes and test thoroughly**
4. **Commit with clear messages**:
   ```bash
   git commit -m "Add feature X with Y capability"
   ```
5. **Push to your fork**:
   ```bash
   git push origin feature/my-new-feature
   ```
6. **Open a Pull Request** with:
   - Clear description of changes
   - Test results on both platforms
   - Screenshots if UI-related

### Pull Request Checklist

Before submitting:
- [ ] Code builds without warnings
- [ ] Tested on Android
- [ ] Tested on iOS (if possible)
- [ ] Sample app updated (if adding features)
- [ ] No breaking changes (or documented)
- [ ] Code follows existing style
- [ ] Commits are logical and clear

### Reporting Issues

When reporting bugs:
1. Use a clear title
2. Describe expected vs actual behavior
3. Provide reproduction steps
4. Include platform and version info
5. Add logs or screenshots if applicable

---

## Advanced Topics

### Custom Renderer Subclassing

For advanced customization:

```csharp
#if ANDROID
public class MyCustomRenderer : CustomWebviewRenderer
{
    public MyCustomRenderer(Context context) : base(context) { }
    
    protected override void SetupControl()
    {
        base.SetupControl();
        // Additional customization
        Control.Settings.SetGeolocationEnabled(true);
    }
}
#endif
```

Register in `MauiProgram.cs`:
```csharp
handlers.AddHandler(typeof(ExtendedWebView), typeof(MyCustomRenderer));
```

### Extending JavaScript Bridge

Add custom bridge methods:

```csharp
// Android Bridge.cs
[Export("customMethod")]
[JavascriptInterface]
public void CustomMethod(string param)
{
    // Handle custom method
}
```

Then call from JavaScript:
```javascript
bridge.customMethod("data");
```

### Performance Profiling

**Android**:
- Use Android Studio Profiler
- Monitor memory, CPU, network

**iOS**:
- Use Xcode Instruments
- Time Profiler, Allocations, Leaks

---

## Resources

### Documentation
- [.NET MAUI Documentation](https://learn.microsoft.com/en-us/dotnet/maui/)
- [Android WebView Reference](https://developer.android.com/reference/android/webkit/WebView)
- [iOS WKWebView Reference](https://developer.apple.com/documentation/webkit/wkwebview)

### Tools
- [Visual Studio](https://visualstudio.microsoft.com/)
- [Xcode](https://developer.apple.com/xcode/)
- [Android Studio](https://developer.android.com/studio)

### Community
- [GitHub Issues](https://github.com/mohammedmsadiq/Plugin.Maui.CustomWebview/issues)
- [.NET MAUI Community](https://dotnet.microsoft.com/apps/maui/community)

---

## Questions?

If you have questions or need help:
1. Check existing documentation
2. Search GitHub issues
3. Open a new issue with your question
4. Contact the maintainer

Happy coding! 🚀
