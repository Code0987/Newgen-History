using System.Windows.Controls;

namespace Internet
{
    /// <summary>
    /// Interaction logic for MetroShellWidget.xaml
    /// </summary>
    public partial class WidgetControl : UserControl
    {
        //let's add a hub. You can remove this variables if you don't want to make a hub for this widget
        private InternetBrowser hub; //a window with hub

        public WidgetControl()
        {
            InitializeComponent();
        }

        //   public void InternetTextMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        // {
        // }
        public void Load()
        {
            //this is used to make widget use same language as MetroShell. If you are not planning to add localization support you can remove this lines

            //place here all initializations
        }

        public void Unload()
        {
            //release resources here
        }

        public void Navigate(string url)
        {
            if (hub != null && hub.IsVisible)
            {
                hub.Activate();
            }
            else
            {
                hub = new InternetBrowser();
                hub.Topmost = true;
                hub.AllowsTransparency = false;
                hub.Show();
            }

            hub.Navigate(url);
        }

        private void UserControlMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (hub != null && hub.IsVisible)
            {
                //hub is already loaded so just bring it to the front
                hub.Activate();
                return;
            }

            hub = new InternetBrowser();
            hub.Topmost = true;
            hub.AllowsTransparency = false;
            hub.Show();
        }
    }
}