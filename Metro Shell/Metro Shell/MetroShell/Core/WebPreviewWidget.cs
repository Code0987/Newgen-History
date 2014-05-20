using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Windows.Media;
using Ftware.Apps.MetroShell.Base;
using Ftware.Apps.MetroShell.Base.Messaging;

namespace Ftware.Apps.MetroShell.Core
{
    public class MetroShellWebPreviewWidget : MetroShellWidget
    {
        private System.Windows.Controls.UserControl uc;
        private static WebBrowser browser;
        private string url;
        private string file;

        public override string Name
        {
            get { return string.Empty; }
        }

        public override System.Windows.FrameworkElement WidgetControl
        {
            get { return uc; }
        }

        public override Uri IconPath
        {
            get { return null; }
        }

        public override int ColumnSpan
        {
            get { return 1; }
        }

        public override void Load(string path)
        {
            url = path;

            uc = new System.Windows.Controls.UserControl();
            uc.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(PreviewControlMouseLeftButtonUp);
            RenderOptions.SetBitmapScalingMode(uc, BitmapScalingMode.HighQuality);
            uc.Background = new SolidColorBrush(Colors.Black);

            file = ConvertUrlToFileName(path) + ".webpreview";
            if (File.Exists(E.CacheRoot + file))
            {
                uc.Background = new ImageBrush()
                {
                    Stretch = Stretch.UniformToFill,
                    AlignmentX = AlignmentX.Left,
                    ImageSource = E.GetBitmap(E.CacheRoot + file)
                };
            }
            else
            {
                browser = new WebBrowser();
                browser.ScrollBarsEnabled = false;
                browser.ScriptErrorsSuppressed = true;
                browser.DocumentCompleted += BrowserDocumentCompleted;
                browser.Width = 1024;
                browser.Height = 768;
                browser.Navigate(path);
            }
        }

        public override void Refresh()
        {
            browser = new WebBrowser();
            browser.ScrollBarsEnabled = false;
            browser.ScriptErrorsSuppressed = true;
            browser.DocumentCompleted += BrowserDocumentCompleted;
            browser.Width = 1024;
            browser.Height = 768;
            browser.Navigate(url);
        }

        private void PreviewControlMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MessagingHelper.SendMessageToMetroShell("URL", url);
        }

        private void BrowserDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (browser.ReadyState != WebBrowserReadyState.Complete) return;
            browser.DocumentCompleted -= BrowserDocumentCompleted;
            var bitmap = new Bitmap(browser.Width, browser.Height);
            browser.DrawToBitmap(bitmap, new System.Drawing.Rectangle(0, 0, browser.Width, browser.Height));
            try
            {
                uc.Background = new SolidColorBrush(Colors.Black);
                if (File.Exists(E.CacheRoot + file)) File.Delete(E.CacheRoot + file);
                bitmap.Save(E.CacheRoot + file, ImageFormat.Png);
            }
            catch { }
            if (File.Exists(E.CacheRoot + file))
            {
                uc.Background = new ImageBrush()
                {
                    Stretch = Stretch.UniformToFill,
                    AlignmentX = AlignmentX.Left,
                    ImageSource = E.GetBitmap(E.CacheRoot + file)
                };
            }
            bitmap.Dispose();
            browser.Dispose();
        }

        public override void Unload()
        {
            if (browser != null && !browser.IsDisposed) browser.Dispose();

            uc.MouseLeftButtonUp -= PreviewControlMouseLeftButtonUp;
        }

        private static string ConvertUrlToFileName(string url)
        {
            return Path.GetFileName(Uri.UnescapeDataString(url).Replace("/", "\\").Replace("?", "-").Replace(":", "-"));
        }
    }
}