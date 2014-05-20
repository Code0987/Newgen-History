using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Linq;
using Ftware.Apps.MetroShell.Base;
using mshtml;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace Ftware.Apps.MetroShell.Core
{
    public class MetroShellHtmlWidget : MetroShellWidget
    {
        private string rootPath;
        private string name;
        private int colSpan;
        private string contentPath;
        private string contentimgPath;
        private string optionsContentPath;
        private string hubContentPath;
        private string iconPath;
        private int mouseX, mouseY;
        private ContextMenu contextMenu;
        private MenuItem unlockItem;
        private MenuItem refreshItem;
        private MenuItem optionsItem;
        private Window optionsWindow;
        private WebBrowser optionsBrowser;
        private HubWindow hub;
        private WebBrowser hubBrowser;
        private DispatcherTimer timer;
        private Grid contentr = new Grid();
        private Image fallbackimg = new Image();
        private WebBrowser browser;

        public MetroShellHtmlWidget(string rootpath)
        {
            rootPath = rootpath;
            if (!File.Exists(rootPath + "\\Widget.Description.xml"))
            {
                throw new FileNotFoundException("HTML Widget Description file " + rootPath + "\\Widget.Description.xml not found.");
            }
            try
            {
                XElement xml = XElement.Load(rootPath + "\\Widget.Description.xml");

                try { name = xml.Element("Name").Value; }
                catch { throw new Exception("Error while allocating widget 'Name'."); }
                try { colSpan = int.Parse(xml.Element("CSpan").Value); }
                catch { throw new Exception("Error while allocating widget 'CSpan'."); }
                try { contentPath = xml.Element("Content").Value; }
                catch { throw new Exception("Error while allocating widget 'Content'."); }
                try { contentimgPath = xml.Element("Content.FallbackImage").Value; }
                catch { throw new Exception("Error while allocating widget 'Content.Fallback'."); }

                if (!File.Exists(rootPath + "\\" + contentPath))
                { throw new FileNotFoundException("Content file " + rootPath + "\\" + contentPath + " not found."); }
                if (xml.Element("Icon") != null) { iconPath = rootPath + "\\" + xml.Element("Icon").Value; }
                if (xml.Element("Options") != null)
                {
                    optionsContentPath = xml.Element("Options").Value;
                    if (!File.Exists(rootPath + "\\" + optionsContentPath)) { throw new FileNotFoundException("Options file " + rootPath + "\\" + optionsContentPath + " not found."); }
                }
                if (xml.Element("Hub") != null)
                {
                    hubContentPath = xml.Element("Hub").Value;
                    if (!File.Exists(rootPath + "\\" + hubContentPath)) { throw new FileNotFoundException("Hub content file " + rootPath + "\\" + hubContentPath + " not found."); }
                }
            }
            catch (Exception ex)
            {
                Helper.ShowErrorMessage("Invalid HTML Widget. Please contact widget provider to get more information.");
                throw ex;
            }
        }

        public override string Name
        {
            get { return name; }
        }

        public override FrameworkElement WidgetControl
        {
            get { return contentr; }
        }

        public override Uri IconPath
        {
            get
            {
                if (string.IsNullOrEmpty(iconPath))
                    return null;
                return new Uri(iconPath);
            }
        }

        public override int ColumnSpan
        {
            get { return colSpan; }
        }

        public override int X
        {
            get { return mouseX; }
        }

        public override int Y
        {
            get { return mouseY; }
        }

        public override void Load()
        {
            contentr.Background = new SolidColorBrush(Colors.Black);
            contentr.Children.Add(new System.Windows.Shapes.Rectangle()
            {
                Stroke = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#71FFFFFF")),
                StrokeThickness = 1
            });

            browser = new WebBrowser();
            browser.Navigate(rootPath + "\\" + contentPath);
            browser.Navigated += BrowserNavigated;

            contextMenu = new ContextMenu();
            unlockItem = new MenuItem();
            unlockItem.Header = "UnLock";
            unlockItem.IsCheckable = true;
            contextMenu.Items.Add(unlockItem);

            refreshItem = new MenuItem();
            refreshItem.Header = "Refresh";
            refreshItem.Click += RefreshItemClick;
            contextMenu.Items.Add(refreshItem);

            browser.ContextMenu = contextMenu;

            if (!string.IsNullOrEmpty(optionsContentPath))
            {
                optionsItem = new MenuItem();
                optionsItem.Header = "Options";
                optionsItem.Click += new RoutedEventHandler(OptionsItemClick);
                contextMenu.Items.Insert(0, optionsItem);
            }
            if (!string.IsNullOrEmpty(hubContentPath))
            {
                hub = new HubWindow();
            }

            timer = new DispatcherTimer(); //we need to refresh html widgets to resolve rendering bugs when moving widgets
            timer.Interval = TimeSpan.FromSeconds(3);
            timer.Tick += new EventHandler(TimerTick);
            timer.Start();

            try
            {
                fallbackimg.Stretch = Stretch.Fill;
                fallbackimg.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(rootPath + "\\" + contentimgPath));
            }
            catch { }
            contentr.Children.Insert(0, browser);
            contentr.Children.Insert(0, fallbackimg);
        }

        private void TimerTick(object sender, EventArgs e)
        {
            try
            {
                browser.Refresh();
            }
            catch
            {
            }
        }

        private void RefreshItemClick(object sender, RoutedEventArgs e)
        {
            try
            {
                browser.Refresh();
            }
            catch
            {
            }
        }

        private void OptionsItemClick(object sender, RoutedEventArgs e)
        {
            if (optionsWindow != null && optionsWindow.IsVisible)
            {
                optionsWindow.Activate();
                return;
            }

            optionsWindow = new Window();
            optionsWindow.Width = 380;
            optionsWindow.Height = 410;
            optionsWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            optionsBrowser = new WebBrowser();
            optionsBrowser.Navigating += OptionsBrowserNavigating;
            optionsWindow.Content = optionsBrowser;

            optionsBrowser.Navigate(rootPath + "\\" + optionsContentPath);
            optionsWindow.ShowDialog();
            optionsBrowser.Navigating -= OptionsBrowserNavigating;
            optionsBrowser.Dispose();
            browser.Refresh();
        }

        private void OptionsBrowserNavigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (e.Uri.OriginalString == "javascript:window.close()")
            {
                e.Cancel = true;
                optionsWindow.Close();
            }
        }

        private bool isAssigned;
        HTMLDocumentEvents2_Event iEvent;

        private void BrowserNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (browser.Document == null || isAssigned)
                return;
            var document = (HTMLDocument)browser.Document;

            iEvent = (HTMLDocumentEvents2_Event)document;
            iEvent.onclick += EventOnclick;
            iEvent.onmousedown += EventOnmousedown;
            iEvent.onmouseup += EventOnmouseup;
            iEvent.onmousemove += EventOnmousemove;
            iEvent.oncontextmenu += EventOncontextmenu;
            isAssigned = true;
        }

        private bool EventOncontextmenu(IHTMLEventObj pEvtObj)
        {
            contextMenu.IsOpen = true;
            return false;
        }

        private int downX, downY;

        private void EventOnmousemove(IHTMLEventObj pEvtObj)
        {
            if (unlockItem.IsChecked)
                return;

            mouseX = pEvtObj.clientX;
            mouseY = pEvtObj.clientY;
            browser.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, Environment.TickCount)
            {
                RoutedEvent = Mouse.MouseMoveEvent,
                Source = browser,
            });
        }

        private void EventOnmouseup(IHTMLEventObj pEvtObj)
        {
            if (pEvtObj.button == 1 && downX == pEvtObj.clientX && downY == pEvtObj.clientY && hub != null)
            {
                downX = -999;
                downY = -999;
                Clicked();
            }
            if (pEvtObj.button != 1 || unlockItem.IsChecked)
                return;
            browser.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
            {
                RoutedEvent = Mouse.MouseUpEvent,
                Source = browser,
            });
        }

        private void EventOnmousedown(IHTMLEventObj pEvtObj)
        {
            downX = pEvtObj.clientX;
            downY = pEvtObj.clientY;

            //if (pEvtObj.button == 1)
            //    browser.Refresh();

            if (pEvtObj.button != 1 || unlockItem.IsChecked)
                return;
            browser.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
            {
                RoutedEvent = Mouse.MouseDownEvent,
                Source = browser,
            });
        }

        private bool EventOnclick(IHTMLEventObj pEvtObj)
        {
            Keyboard.Focus(browser);
            return true;
        }

        public override void Unload()
        {
            timer.Stop();
            if (iEvent != null)
            {
                iEvent.onclick -= EventOnclick;
                iEvent.onmousedown -= EventOnmousedown;
                iEvent.onmouseup -= EventOnmouseup;
                iEvent.onmousemove -= EventOnmousemove;
                iEvent.oncontextmenu -= EventOncontextmenu;
            }

            if (optionsItem != null)
                optionsItem.Click -= OptionsItemClick;

            refreshItem.Click -= RefreshItemClick;

            if (browser != null)
                browser.Dispose();
        }

        private void Clicked()
        {
            if (hub != null && hub.IsVisible)
            {
                hub.Activate();
                return;
            }

            hub = new HubWindow();
            hub.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            hubBrowser = new WebBrowser();
            hubBrowser.Navigating += HubBrowserNavigating;
            hub.Content = hubBrowser;

            hubBrowser.Navigate(rootPath + "\\" + hubContentPath);
            hub.ShowDialog();
            hubBrowser.Navigating -= HubBrowserNavigating;
            hubBrowser.Dispose();
        }

        private void HubBrowserNavigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (e.Uri.OriginalString == "javascript:window.close()")
            {
                e.Cancel = true;
                hub.Close();
            }
        }
    }
}