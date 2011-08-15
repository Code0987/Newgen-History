using System.Windows.Controls;
using System.Windows.Media;
using Mosaic.Base;

namespace Applications
{
    /// <summary>
    /// Interaction logic for MosaicWidget.xaml
    /// </summary>
    public partial class WidgetControl : UserControl
    {
        //let's add a hub. You can remove this variables if you don't want to make a hub for this widget
        private HubWindow hub; //a window with hub
        private HubControl hubContent; //hub control

        public WidgetControl()
        {
            InitializeComponent();
        }

        public void Load()
        {
            //this is used to make widget use same language as Mosaic. If you are not planning to add localization support you can remove this lines
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(E.Language);
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(E.Language);

            //place here all initializations
        }

        public void Unload()
        {
            //release resources here
        }

        private void UserControlMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (hub != null && hub.IsVisible)
            {
                //hub is already loaded so just bring it to the front
                hub.Activate();
                return;
            }

            //initalize hub
            hub = new HubWindow();
            hub.Topmost = true;
            hub.AllowsTransparency = true;
            hubContent = new HubControl();
            hubContent.Closing += HubContentClosing;
            hub.Content = hubContent;
            hub.Show();
        }

        void HubContentClosing(object sender, System.EventArgs e)
        {
            hub.Close();
        }
    }
}
