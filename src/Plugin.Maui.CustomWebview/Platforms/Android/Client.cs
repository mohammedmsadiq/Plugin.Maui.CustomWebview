using Android.Content;
using Android.Graphics;
using Android.Net;
using Android.Net.Http;
using Android.OS;
using Android.Runtime;
using Android.Webkit;
using Android.Widget;
using Plugin.Maui.CustomWebview.Implementations;
using AndroidWebView = Android.Webkit;
using Uri = Android.Net.Uri;

namespace Plugin.Maui.CustomWebview;

public class Client : WebViewClient
    {

        readonly WeakReference<CustomWebviewRenderer> Reference;

        public Client(CustomWebviewRenderer renderer)
        {
            Reference = new WeakReference<CustomWebviewRenderer>(renderer);
        }

        public override void OnReceivedHttpError(AndroidWebView.WebView view, IWebResourceRequest request, WebResourceResponse errorResponse)
        {
            if (Reference == null || !Reference.TryGetTarget(out CustomWebviewRenderer renderer)) return;
            if (renderer.Element == null) return;

            renderer.Element.HandleNavigationError(errorResponse.StatusCode);
            renderer.Element.HandleNavigationCompleted(request.Url.ToString());
            renderer.Element.Navigating = false;
        }

        public override void OnReceivedError(AndroidWebView.WebView view, IWebResourceRequest request, WebResourceError error)
        {
            if (Reference == null || !Reference.TryGetTarget(out CustomWebviewRenderer renderer)) return;
            if (renderer.Element == null) return;

            renderer.Element.HandleNavigationError((int)error.ErrorCode);
            renderer.Element.HandleNavigationCompleted(request.Url.ToString());
            renderer.Element.Navigating = false;
        }

        //For Android < 5.0
        [Obsolete]
        public override void OnReceivedError(AndroidWebView.WebView view, [GeneratedEnum] ClientError errorCode, string description, string failingUrl)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop) return;

            if (Reference == null || !Reference.TryGetTarget(out CustomWebviewRenderer renderer)) return;
            if (renderer.Element == null) return;

            renderer.Element.HandleNavigationError((int)errorCode);
            renderer.Element.HandleNavigationCompleted(failingUrl.ToString());
            renderer.Element.Navigating = false;
        }

        //For Android < 5.0
        [Obsolete]
        public override WebResourceResponse ShouldInterceptRequest(AndroidWebView.WebView view, string url)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop) goto EndShouldInterceptRequest;

            if (Reference == null || !Reference.TryGetTarget(out CustomWebviewRenderer renderer)) goto EndShouldInterceptRequest;
            if (renderer.Element == null) goto EndShouldInterceptRequest;

            var response = renderer.Element.HandleNavigationStartRequest(url);

            if (response.Cancel || response.OffloadOntoDevice)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    if (response.OffloadOntoDevice)
                        AttemptToHandleCustomUrlScheme(view, url);

                    view.StopLoading();
                });
            }

        EndShouldInterceptRequest:
            return base.ShouldInterceptRequest(view, url);
        }

        public override WebResourceResponse ShouldInterceptRequest(AndroidWebView.WebView view, IWebResourceRequest request)
        {
            if (Reference == null || !Reference.TryGetTarget(out CustomWebviewRenderer renderer)) goto EndShouldInterceptRequest;
            if (renderer.Element == null) goto EndShouldInterceptRequest;

            string url = request.Url.ToString();
            var response = renderer.Element.HandleNavigationStartRequest(url);

            if (response.Cancel || response.OffloadOntoDevice)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    if (response.OffloadOntoDevice)
                        AttemptToHandleCustomUrlScheme(view, url);

                    view.StopLoading();
                });
            }

        EndShouldInterceptRequest:
            return base.ShouldInterceptRequest(view, request);
        }

        void CheckResponseValidity(AndroidWebView.WebView view, string url)
        {
            if (Reference == null || !Reference.TryGetTarget(out CustomWebviewRenderer renderer)) return;
            if (renderer.Element == null) return;

            var response = renderer.Element.HandleNavigationStartRequest(url);

            if (response.Cancel || response.OffloadOntoDevice)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    if (response.OffloadOntoDevice)
                        AttemptToHandleCustomUrlScheme(view, url);

                    view.StopLoading();
                });
            }
        }

        public override void OnPageStarted(AndroidWebView.WebView view, string url, Bitmap favicon)
        {
            if (Reference == null || !Reference.TryGetTarget(out CustomWebviewRenderer renderer)) return;
            if (renderer.Element == null) return;

            renderer.Element.Navigating = true;
        }

        bool AttemptToHandleCustomUrlScheme(AndroidWebView.WebView view, string url)
        {
            if (url.StartsWith("mailto"))
            {
                MailTo emailData = MailTo.Parse(url);

                Intent email = new Intent(Intent.ActionSendto);

                email.SetData(Uri.Parse("mailto:"));
                email.PutExtra(Intent.ExtraEmail, new String[] { emailData.To });
                email.PutExtra(Intent.ExtraSubject, emailData.Subject);
                email.PutExtra(Intent.ExtraCc, emailData.Cc);
                email.PutExtra(Intent.ExtraText, emailData.Body);

                if (email.ResolveActivity(Platform.AppContext.PackageManager) != null)
                    Platform.AppContext.StartActivity(email);
                else
                {
                    // Handle case where no email client is available
                    Toast.MakeText(Platform.AppContext, "No email clients installed.", ToastLength.Short).Show();
                }

                return true;
            }

            if (url.StartsWith("http"))
            {
                Intent webPage = new Intent(Intent.ActionView,Uri.Parse(url));
                if (webPage.ResolveActivity(Platform.AppContext.PackageManager) != null)
                    Platform.AppContext.StartActivity(webPage);

                return true;
            }

            return false;
        }

        public override void OnReceivedSslError(AndroidWebView.WebView view, SslErrorHandler handler, SslError error)
        {
            if (Reference == null || !Reference.TryGetTarget(out CustomWebviewRenderer renderer)) return;
            if (renderer.Element == null) return;

            if (CustomWebviewRenderer.IgnoreSSLGlobally)
            {
                handler.Proceed();
            }

            else
            {
                handler.Cancel();
                renderer.Element.Navigating = false;
            }
        }

        public async override void OnPageFinished(AndroidWebView.WebView view, string url)
        {
            if (Reference == null || !Reference.TryGetTarget(out CustomWebviewRenderer renderer)) return;
            if (renderer.Element == null) return;

            // Add Injection Function
            await renderer.OnJavascriptInjectionRequest(ExtendedWebView.InjectedFunction);

            // Add Global Callbacks
            if (renderer.Element.EnableGlobalCallbacks)
                foreach (var callback in ExtendedWebView.GlobalRegisteredCallbacks)
                    await renderer.OnJavascriptInjectionRequest(ExtendedWebView.GenerateFunctionScript(callback.Key));

            // Add Local Callbacks
            foreach (var callback in renderer.Element.LocalRegisteredCallbacks)
                await renderer.OnJavascriptInjectionRequest(ExtendedWebView.GenerateFunctionScript(callback.Key));

            renderer.Element.CanGoBack = view.CanGoBack();
            renderer.Element.CanGoForward = view.CanGoForward();
            renderer.Element.Navigating = false;

            renderer.Element.HandleNavigationCompleted(url);
            renderer.Element.HandleContentLoaded();
        }
    }