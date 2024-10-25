using Android.Webkit;

namespace Plugin.Maui.CustomWebView;

public class ChromeClient: WebChromeClient
    {

        readonly WeakReference<CustomWebviewRenderer> Reference;

        public ChromeClient(CustomWebviewRenderer renderer)
        {
            Reference = new WeakReference<CustomWebviewRenderer>(renderer);
        }

    }