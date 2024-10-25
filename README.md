# Plugin.Maui.CustomWebview

`Plugin.Maui.CustomWebview` Lightweight cross platform WebView designed to leverage the native WebView components in Android, iOS to provide enhanced functionality over the base control.

## Install Plugin


Available on [NuGet](http://www.nuget.org/packages/Plugin.Maui.CustomWebview).

Install with the dotnet CLI: `dotnet add package Plugin.Maui.CustomWebview`, or through the NuGet Package Manager in Visual Studio.

### Supported Platforms

| Platform | Minimum Version Supported |
|----------|---------------------------|
| iOS      | 11+                       |
| Android  | 5.0 (API 21)              |

## Setup

```csharp
    .ConfigureMauiHandlers(handlers =>
    {
#if ANDROID || IOS
	    handlers.AddHandler(typeof(ExtendedWebView), typeof(CustomWebviewRenderer));
#endif
	});
```

## API Usage
```csharp
/// <summary>
/// Bind an action to a Javascript function
/// </summary>
ExtendedWebView WebView = new ExtendedWebView();
WebView.AddLocalCallback("test", (str) => Debug.WriteLine(str));
WebView.RemoveLocalCallback("test");
```
```csharp
/// <summary>
/// Initialize the WebView, Navigation will occur when the Source is changed so make sure to set the BaseUrl and ContentType prior.
/// </summary>
ExtendedWebView WebView = new ExtendedWebView() {
    ContentType = WebContentType.Internet,
    Source = "http://www.somewebsite.com"
}
```
```csharp
/// <summary>
/// If you wish to further modify the native control, then you can bind to these events in your platform specific code.
/// These events will be called when the control is preparing and ready.
/// </summary>
CustomWebviewRenderer.OnControlChanged += ModifyControlAfterReady;
```
```csharp
/// <summary>
/// Attach events using a instance of the WebView.
/// </summary>
WebView.OnNavigationStarted += OnNavigationStarted;
WebView.OnNavigationCompleted += OnNavigationComplete;
WebView.OnContentLoaded += OnContentLoaded;
```
```csharp
/// <summary>
/// You can cancel a URL from being loaded by returning a delegate with the cancel boolean set to true.
/// </summary>
private void OnNavigationStarted(NavigationRequestedDelegate eventObj)
{
    if (eventObj.Source == "www.somebadwebsite.com")
        eventObj.Cancel = true;
}
```
```csharp
/// <summary>
/// To return a string to c#, simple invoke the csharp(str) method.
/// </summary>
private void OnNavigationComplete(NavigationCompletedDelegate eventObj)
{
    System.Diagnostics.Debug.WriteLine(string.Format("Load Complete: {0}", eventObj.Sender.Source));
}

/// <summary>
/// RUN ALL JAVASCRIPT HERE
/// </summary>
private void OnContentLoaded(ContentLoadedDelegate eventObj)
{
    System.Diagnostics.Debug.WriteLine(string.Format("DOM Ready: {0}", eventObj.Sender.Source));
    eventObj.Sender.InjectJavascript("csharp('Testing');");
}
```
### Permissions

Before you can start using Feature, you will need to request the proper permissions on each platform.

#### iOS

No permissions are needed for iOS.

#### Android

No permissions are needed for Android.


# Acknowledgements

This project could not have came to be without these projects and people, thank you! <3
