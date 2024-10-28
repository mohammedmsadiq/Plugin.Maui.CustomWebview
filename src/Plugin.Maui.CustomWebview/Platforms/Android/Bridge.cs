using Android.Webkit;
using Java.Interop;

namespace Plugin.Maui.CustomWebView.Platforms.Android;

public class Bridge : Java.Lang.Object
    {

        readonly WeakReference<CustomWebviewRenderer> Reference;

        public Bridge(CustomWebviewRenderer renderer)
        {
            Reference = new WeakReference<CustomWebviewRenderer>(renderer);
        }

        [JavascriptInterface]
        [Export("invokeAction")]
        public void InvokeAction(string data)
        {
            if (Reference == null || !Reference.TryGetTarget(out CustomWebviewRenderer renderer)) return;
            if (renderer.Element == null) return;

            renderer.Element.HandleScriptReceived(data);
        }
    }