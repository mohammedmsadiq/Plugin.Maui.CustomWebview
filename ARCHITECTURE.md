# Plugin.Maui.CustomWebview - Architecture Deep Dive

## Overview

This document provides an in-depth technical analysis of the Plugin.Maui.CustomWebview architecture, implementation patterns, and internal mechanisms.

---

## Table of Contents

1. [High-Level Architecture](#high-level-architecture)
2. [Component Diagram](#component-diagram)
3. [Data Flow](#data-flow)
4. [JavaScript Bridge Implementation](#javascript-bridge-implementation)
5. [Platform-Specific Details](#platform-specific-details)
6. [Event Flow](#event-flow)
7. [Memory Management](#memory-management)
8. [Threading Model](#threading-model)

---

## High-Level Architecture

### Layered Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        Application Layer                         │
│                     (MAUI App / XAML / C#)                      │
└───────────────────────────┬─────────────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────────────┐
│                   Plugin Public API Layer                        │
│                                                                  │
│  ┌────────────────┐  ┌─────────────────┐  ┌─────────────────┐ │
│  │ ExtendedWebView│  │  ICustomWebview │  │ DecisionHandlers│ │
│  └────────────────┘  └─────────────────┘  └─────────────────┘ │
└───────────────────────────┬─────────────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────────────┐
│                   MAUI Handler Layer                             │
│                                                                  │
│  ┌──────────────────────────┐  ┌──────────────────────────┐   │
│  │ CustomWebviewRenderer    │  │ CustomWebviewRenderer    │   │
│  │     (Android)            │  │        (iOS)             │   │
│  └──────────────────────────┘  └──────────────────────────┘   │
└───────────────────────────┬─────────────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────────────┐
│                   Native Platform Layer                          │
│                                                                  │
│  ┌──────────────────────────┐  ┌──────────────────────────┐   │
│  │   Android.Webkit         │  │      WebKit              │   │
│  │      WebView             │  │     WKWebView            │   │
│  └──────────────────────────┘  └──────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

### Key Architectural Principles

1. **Separation of Concerns**: Clear distinction between platform-agnostic and platform-specific code
2. **Handler Pattern**: Follows .NET MAUI's handler-based rendering architecture
3. **Event-Driven**: Communication through events and callbacks
4. **Dependency Inversion**: Platform abstractions through interfaces
5. **Bridge Pattern**: JavaScript-C# bridge for cross-boundary communication

---

## Component Diagram

### Core Components and Their Relationships

```
┌─────────────────────────────────────────────────────────────────┐
│                       ExtendedWebView                            │
├─────────────────────────────────────────────────────────────────┤
│ Properties:                                                      │
│  - Source, ContentType, BaseUrl                                 │
│  - Navigating, CanGoBack, CanGoForward                         │
│  - EnableGlobalCallbacks, EnableGlobalHeaders                   │
│                                                                  │
│ Events:                                                          │
│  - OnNavigationStarted, OnNavigationCompleted                   │
│  - OnNavigationError, OnContentLoaded                           │
│                                                                  │
│ Internal Events:                                                 │
│  - OnBackRequested, OnForwardRequested, OnRefreshRequested      │
│  - OnJavascriptInjectionRequest, OnClearCookiesRequested        │
│                                                                  │
│ Collections:                                                     │
│  - LocalRegisteredCallbacks: Dictionary<string, Action<string>> │
│  - LocalRegisteredHeaders: Dictionary<string, string>           │
│  - GlobalRegisteredCallbacks (static)                           │
│  - GlobalRegisteredHeaders (static)                             │
└──────────────────────────┬──────────────────────────────────────┘
                           │
                           │ Handled by
                           │
        ┌──────────────────┴─────────────────────┐
        │                                        │
        ▼                                        ▼
┌─────────────────────┐              ┌──────────────────────┐
│ Android Handler     │              │   iOS Handler        │
├─────────────────────┤              ├──────────────────────┤
│ Components:         │              │ Components:          │
│  - Client           │              │  - NavigationDelegate│
│  - ChromeClient     │              │  - Content Controller│
│  - Bridge           │              │  - Script Handler    │
│  - ValueCallback    │              │                      │
│                     │              │                      │
│ Native:             │              │ Native:              │
│  - WebView          │              │  - WKWebView         │
│  - WebSettings      │              │  - WKConfiguration   │
│  - CookieManager    │              │  - WKCookieStore     │
└─────────────────────┘              └──────────────────────┘
```

---

## Data Flow

### 1. JavaScript-to-C# Call Flow

```
┌─────────────┐     1. Call csharp()     ┌──────────────────┐
│             │ ───────────────────────> │                  │
│  JavaScript │                          │  Bridge/Handler  │
│   Context   │                          │  (Native Layer)  │
│             │ <─────────────────────── │                  │
└─────────────┘     2. Serialize data    └────────┬─────────┘
                                                   │
                                                   │ 3. Route to handler
                                                   │
                                         ┌─────────▼─────────┐
                                         │                   │
                                         │  ExtendedWebView  │
                                         │  HandleScriptReceived()
                                         │                   │
                                         └─────────┬─────────┘
                                                   │
                                                   │ 4. Deserialize
                                                   │
                                         ┌─────────▼─────────┐
                                         │  ActionEventModel │
                                         │  { Action, Data } │
                                         └─────────┬─────────┘
                                                   │
                                                   │ 5. Lookup callback
                                                   │
                          ┌────────────────────────┴────────────────────┐
                          │                                             │
                          ▼                                             ▼
                ┌──────────────────┐                        ┌─────────────────┐
                │ Local Callbacks  │                        │ Global Callbacks│
                │ (Instance-level) │                        │  (App-level)    │
                └────────┬─────────┘                        └────────┬────────┘
                         │                                           │
                         └─────────────────┬─────────────────────────┘
                                           │
                                           │ 6. Invoke
                                           │
                                    ┌──────▼────────┐
                                    │  C# Action    │
                                    │  Execute      │
                                    └───────────────┘
```

### 2. C#-to-JavaScript Injection Flow

```
┌──────────────────┐
│  C# Application  │
│  Code            │
└────────┬─────────┘
         │ 1. Call InjectJavascriptAsync("script")
         │
         ▼
┌──────────────────┐
│ ExtendedWebView  │
│ InjectJavascriptAsync()
└────────┬─────────┘
         │ 2. Raise OnJavascriptInjectionRequest event
         │
         ▼
┌──────────────────────────┐
│  Platform Renderer       │
│  OnJavascriptInjectionRequest handler
└────────┬─────────────────┘
         │ 3. Platform-specific injection
         │
    ┌────┴─────┐
    │          │
    ▼          ▼
┌────────┐  ┌──────────┐
│Android │  │   iOS    │
│WebView │  │WKWebView │
│.eval   │  │.eval     │
└────┬───┘  └────┬─────┘
     │           │ 4. Execute JavaScript
     │           │
     └─────┬─────┘
           │ 5. Return result (if any)
           │
           ▼
    ┌────────────────┐
    │  ValueCallback │
    │  Complete Task │
    └────────┬───────┘
             │ 6. Return to C#
             ▼
    ┌─────────────────┐
    │ await result    │
    └─────────────────┘
```

### 3. Navigation Event Flow

```
┌──────────────────┐
│ User Action      │
│ (Click link)     │
└────────┬─────────┘
         │
         ▼
┌──────────────────────────┐
│ Native Navigation Event  │
│ (shouldStartLoad/         │
│  decidePolicyFor)        │
└────────┬─────────────────┘
         │ 1. Intercept
         │
         ▼
┌──────────────────────────┐
│ Platform Renderer        │
│ Navigation Delegate      │
└────────┬─────────────────┘
         │ 2. Create DecisionHandlers
         │
         ▼
┌──────────────────────────┐
│ ExtendedWebView          │
│ HandleNavigationStartRequest()
└────────┬─────────────────┘
         │ 3. Fire OnNavigationStarted event
         │
         ▼
┌──────────────────────────┐
│ Application Code         │
│ Event Handler            │
│ - Check URI              │
│ - Set Cancel flag        │
│ - Set OffloadOntoDevice  │
└────────┬─────────────────┘
         │ 4. Return decision
         │
         ▼
┌──────────────────────────┐
│ Platform Renderer        │
│ Apply decision:          │
│  - Allow/Cancel          │
│  - Open external         │
└──────────────────────────┘
```

---

## JavaScript Bridge Implementation

### Bridge Code Generation

The plugin injects a JavaScript function that serves as the bridge:

```javascript
// Android
function csharp(data) {
    bridge.invokeAction(data);
}

// iOS
function csharp(data) {
    window.webkit.messageHandlers.invokeAction.postMessage(data);
}
```

### Callback Registration

When you register a callback:

```csharp
webView.AddLocalCallback("myFunction", (data) => Console.WriteLine(data));
```

The plugin generates and injects additional JavaScript:

```javascript
function myFunction(str) {
    csharp('{"action":"myFunction","data":"' + window.btoa(str) + '"}');
}
```

### Data Serialization

**JavaScript Side**:
1. Data is Base64-encoded using `window.btoa()`
2. Wrapped in JSON structure with action name
3. Sent through platform bridge

**C# Side**:
```csharp
// Deserialize JSON
var action = JsonConvert.DeserializeObject<ActionEventModel>(data);

// Decode Base64
byte[] bytes = Convert.FromBase64String(action.Data);
action.Data = Encoding.UTF8.GetString(bytes);

// Route to callback
LocalRegisteredCallbacks[action.Action]?.Invoke(action.Data);
```

---

## Platform-Specific Details

### Android Implementation Details

**WebView Configuration**:
```csharp
WebSettings settings = Control.Settings;
settings.JavaScriptEnabled = true;
settings.DomStorageEnabled = true;
settings.AllowFileAccess = true;
settings.AllowContentAccess = true;
```

**JavaScript Bridge**:
```csharp
Bridge bridge = new Bridge(Element);
Control.AddJavascriptInterface(bridge, "bridge");
```

**Bridge Class** (with @JavascriptInterface):
```csharp
public class Bridge : Java.Lang.Object
{
    [Export("invokeAction")]
    [JavascriptInterface]
    public void InvokeAction(string data)
    {
        _webView?.HandleScriptReceived(data);
    }
}
```

**JavaScript Injection**:
```csharp
Control.EvaluateJavascript(script, _callback);
```

### iOS Implementation Details

**WKWebView Configuration**:
```csharp
_configuration = new WKWebViewConfiguration();
_contentController = new WKUserContentController();

// Register message handler
_contentController.AddScriptMessageHandler(this, "invokeAction");

_configuration.UserContentController = _contentController;
WKWebView webView = new WKWebView(Frame, _configuration);
```

**Message Handler**:
```csharp
public void DidReceiveScriptMessage(WKUserContentController controller, 
                                    WKScriptMessage message)
{
    if (message.Name == "invokeAction")
    {
        Element?.HandleScriptReceived(message.Body.ToString());
    }
}
```

**JavaScript Injection**:
```csharp
Control.EvaluateJavaScript(script, (result, error) => {
    // Handle result
});
```

---

## Event Flow

### Component Lifecycle

```
1. Application creates ExtendedWebView
   └─> ExtendedWebView constructor
       └─> Set HorizontalOptions, VerticalOptions

2. MAUI creates handler
   └─> CustomWebviewRenderer constructor
       └─> Platform-specific initialization

3. OnElementChanged event
   └─> SetupControl() - Create native WebView
   └─> SetupElement() - Wire up events
       ├─> element.PropertyChanged
       ├─> element.OnJavascriptInjectionRequest
       ├─> element.OnClearCookiesRequested
       ├─> element.OnBackRequested
       ├─> element.OnForwardRequested
       └─> element.OnRefreshRequested

4. Property changes (Source, ContentType, etc.)
   └─> OnPropertyChanged handler
       └─> Load content based on type
           ├─> LoadFromInternet()
           ├─> LoadFromString()
           └─> LoadFromLocalFile()

5. Navigation lifecycle
   └─> Native navigation starts
       └─> HandleNavigationStartRequest()
           └─> OnNavigationStarted event (app code)
       └─> Content loading
       └─> DOM ready
           └─> Inject JavaScript bridge
           └─> HandleContentLoaded()
               └─> OnContentLoaded event (app code)
       └─> Navigation complete
           └─> HandleNavigationCompleted()
               └─> OnNavigationCompleted event (app code)

6. Disposal
   └─> Dispose()
       ├─> Clear LocalRegisteredCallbacks
       └─> Clear LocalRegisteredHeaders
```

---

## Memory Management

### Potential Memory Leaks and Mitigations

**Event Handlers**:
- Events in renderers are wired to element events
- Properly unwired in `DestroyElement()` (iOS) or similar cleanup

**Dictionaries**:
- Local callbacks cleared in `Dispose()`
- Global callbacks: Application-lifetime (by design)

**Native Objects**:
- Android: WebView disposal handled by renderer
- iOS: WKWebView released when renderer disposed

**Best Practices**:
```csharp
// Always dispose when done
using (var webView = new ExtendedWebView())
{
    // Use webView
}

// Or explicitly
webView.Dispose();
```

---

## Threading Model

### Thread Considerations

**UI Thread Operations**:
- All WebView operations must be on UI thread
- MAUI handlers ensure proper thread marshaling
- Event callbacks invoked on UI thread

**JavaScript Execution**:
- `InjectJavascriptAsync()` is async but returns on UI thread
- JavaScript itself runs on WebView's JavaScript thread

**Callback Invocation**:
- JavaScript bridge messages processed on UI thread
- C# action callbacks executed on UI thread
- No explicit synchronization needed in app code

**Example Threading Flow**:
```
Background Thread                    UI Thread
─────────────────                    ─────────

    [Task.Run]
         │
         │ await InjectJavascriptAsync()
         │ ───────────────────────────────> [Marshal to UI]
         │                                         │
         │                                         │ Execute JS
         │                                         │
         │ <─────────────────────────────────────  │ Return result
         │                                    [Still on UI]
    [Continue on
     background]
```

---

## Extension Points

### Custom Renderer Events

For advanced scenarios:

```csharp
// Platform-specific code
CustomWebviewRenderer.OnControlChanged += (sender, webView) => {
    // Access native WebView directly
    // Android: android.webkit.WebView
    // iOS: WebKit.WKWebView
    
    #if ANDROID
    webView.Settings.CustomUserAgent = "MyApp/1.0";
    #elif IOS
    webView.CustomUserAgent = "MyApp/1.0";
    #endif
};
```

### Global Configuration

```csharp
// Set for all WebView instances
ExtendedWebView.AddGlobalCallback("analytics", (data) => {
    // Send to analytics
});

ExtendedWebView.GlobalRegisteredHeaders["X-Custom-Header"] = "value";
```

---

## Security Considerations

### JavaScript Execution Context

- JavaScript code runs in same context as web content
- Can access DOM and all page resources
- Use caution when injecting user-provided code

### SSL/TLS

Android allows global SSL bypass:
```csharp
CustomWebviewRenderer.IgnoreSSLGlobally = true; // NOT RECOMMENDED
```

**Recommendation**: Only use in development, never in production

### Data Sanitization

Always sanitize data passed to/from JavaScript:
```csharp
webView.AddLocalCallback("userInput", (data) => {
    // Validate and sanitize
    if (IsValidInput(data))
    {
        ProcessInput(data);
    }
});
```

---

## Performance Optimization

### JavaScript Injection

- Inject JavaScript during `OnContentLoaded` event (DOM ready)
- Batch multiple injections into single call
- Cache generated callback functions

### Callback Management

- Remove unused callbacks: `RemoveLocalCallback()`
- Clear all when done: `RemoveAllLocalCallbacks()`
- Use global callbacks for shared functionality

### Content Loading

- Use `LocalFile` for large static content
- Cache remote content when appropriate
- Set appropriate `BaseUrl` for relative resources

---

## Summary

The Plugin.Maui.CustomWebview architecture is well-designed for cross-platform WebView scenarios:

**Strengths**:
- ✅ Clean separation of concerns
- ✅ Platform-specific optimizations
- ✅ Event-driven architecture
- ✅ Extensible design
- ✅ Type-safe C# API

**Considerations**:
- ⚠️ Some interface implementations throw NotImplementedException (legacy code)
- ⚠️ Global callbacks are app-lifetime (by design)
- ⚠️ JavaScript execution context shares with web content

The plugin successfully abstracts platform differences while exposing platform capabilities through a unified API.
