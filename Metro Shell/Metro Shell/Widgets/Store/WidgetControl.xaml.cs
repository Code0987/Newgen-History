using System.Windows.Controls;

using Ftware.Apps.MetroShell.Base;

namespace Store
{
    public partial class WidgetControl : UserControl
    {
        private HubWindow hub;
        private HubControl hubContent;

        public WidgetControl()
        {
            InitializeComponent();
        }

        public void Load()
        {
        }

        public void Unload()
        {
        }

        private void UserControlMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (hub != null && hub.IsVisible)
            {
                hub.Activate();
                return;
            }

            hub = new HubWindow();
            hub.Topmost = false;
            hub.AllowsTransparency = false;
            hubContent = new HubControl();
            hubContent.Closing += HubContentClosing;
            hub.Content = hubContent;
            hub.Show();
        }

        private void HubContentClosing(object sender, System.EventArgs e)
        {
            hub.Close();
        }
    }
}