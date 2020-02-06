using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.Internals;
using CefSharp.OffScreen;

namespace CefSharpOffscreenRepro
{
    public static class Program
    {
        // Limit operations to prevent rogue CefHelper processes from hanging around.
        static readonly TimeSpan pageLoadTimeout = TimeSpan.FromMilliseconds(120000);
        static readonly TimeSpan javascriptTimeout = TimeSpan.FromMilliseconds(10000);

        public static int Main(string[] args)
        {
            Log("CefHelper run.");

            // Ensure unicode characters don't get garbled in transmission.
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Read the input without interruption to avoid deadlocks.
            //string url = Console.ReadLine();
            //string waitForSelector = Console.ReadLine();
            //string waitForTimeoutInMillisecondsRaw = Console.ReadLine();
            Thread.Sleep(new Random().Next(1000));
            string url = "https://sheldonbrown.com/web_sample1.html";
            string waitForSelector = "";
            string waitForTimeoutInMillisecondsRaw = "";

            // Initialize Cef.
            CefSettings settings = new CefSettings()
            {
                // Use incognito mode.
                CachePath = null,
                LogSeverity = LogSeverity.Info,
                PersistSessionCookies = false,
                PersistUserPreferences = false,
                // Block certificate errors to avoid man-in-the-middle attacks.
                IgnoreCertificateErrors = false,
            };

            Cef.Initialize(settings);

            try
            {
                Task<string> t = GetPageSource(url);
                t.Wait();
                Console.Write(t.Result);
                return 0;
            }
            catch (Exception ex)
            {
                Log("Exception: " + ex.ToString());
                Console.Write(ex.Message);
                return -1;
            }
            finally
            {
                // Clean up Chromium objects.  You need to call this in your application otherwise
                // you will get a crash (or hang) when closing.
                Cef.Shutdown();
            }
        }

        public static void Log(string message)
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string filePath = Path.Combine(folder, "debug.log");
            File.AppendAllText(filePath, DateTime.Now + ": " + message + (message.EndsWith(Environment.NewLine) ? "" : Environment.NewLine));
        }

        static async Task<string> GetPageSource(string url)
        {
            BrowserSettings browserSettings = new BrowserSettings
            {
                FileAccessFromFileUrls = CefState.Disabled,
                JavascriptAccessClipboard = CefState.Disabled,
                JavascriptCloseWindows = CefState.Disabled,
                JavascriptDomPaste = CefState.Disabled,
                Plugins = CefState.Disabled,
                UniversalAccessFromFileUrls = CefState.Disabled,
                WebSecurity = CefState.Enabled,
            };

            RequestContextSettings requestContextSettings = new RequestContextSettings
            {
                CachePath = null,
                IgnoreCertificateErrors = false,
                PersistSessionCookies = false,
                PersistUserPreferences = false,
            };

            // RequestContext can be shared between browser instances and allows for custom settings
            // e.g. CachePath
            using (RequestContext requestContext = new RequestContext(requestContextSettings))
            using (ChromiumWebBrowser browser = new ChromiumWebBrowser(url, browserSettings, requestContext))
            {
                await LoadPageAsync(browser);
                return "Hello World!!\n";
            }
        }

        static async Task<bool> LoadPageAsync(IWebBrowser browser)
        {
            // Based on https://github.com/cefsharp/CefSharp/blob/master/CefSharp.OffScreen.Example/Program.cs
            // There are two notable differences from the example, which appear to be .NET 3.5 vs. 4.6 related.
            // See the following comment for a description of these differences.

            // If using .Net 4.6 then use TaskCreationOptions.RunContinuationsAsynchronously
            // and switch to tcs.TrySetResult below - no need for the custom extension method
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>(false).WithTimeout(pageLoadTimeout);

            EventHandler<LoadingStateChangedEventArgs> handler = null;
            handler = (sender, args) =>
            {
                // Wait for while page to finish loading not just the first frame
                if (!args.IsLoading)
                {
                    browser.LoadingStateChanged -= handler;

                    // This is required when using a standard TaskCompletionSource
                    // Extension method found in the CefSharp.Internals namespace
                    tcs.TrySetResultAsync(true);
                }
            };

            browser.LoadingStateChanged += handler;

            return await tcs.Task;
        }
    }
}
