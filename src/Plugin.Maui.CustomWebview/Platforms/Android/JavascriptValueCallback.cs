using Android.Webkit;

namespace Plugin.Maui.CustomWebView;

public class JavascriptValueCallback : Java.Lang.Object, IValueCallback
{
    public Java.Lang.Object Value { get; private set; }

    readonly WeakReference<CustomWebviewRenderer> Reference;

    public JavascriptValueCallback(CustomWebviewRenderer renderer)
    {
        Reference = new WeakReference<CustomWebviewRenderer>(renderer);
    }

    public void OnReceiveValue(Java.Lang.Object value)
    {
        if (Reference == null || !Reference.TryGetTarget(out CustomWebviewRenderer renderer)) return;
        Value = value;
    }

    public void Reset()
    {
        Value = null;
    }
}