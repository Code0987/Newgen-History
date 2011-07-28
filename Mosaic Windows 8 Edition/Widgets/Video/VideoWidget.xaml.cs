using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Mosaic.Base;

namespace Video
{
    /// <summary>
    /// Interaction logic for VideoWidget.xaml
    /// </summary>
    public partial class VideoWidget : UserControl
    {
        private HubWindow hub;
        private Hub hubContent;

        public VideoWidget()
        {
            InitializeComponent();
        }

        private void UserControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            /*var file = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Microsoft\\Windows\\Libraries\\Videos.library-ms";
            WinAPI.ShellExecute(IntPtr.Zero, "open", file, null, null, 1);*/
            if (hub != null && hub.IsVisible)
            {
                hub.Activate();
                return;
            }

            hub = new HubWindow();
            //hub.Topmost = true;
            hub.AllowsTransparency = true;
            hubContent = new Hub();
            hub.Content = hubContent;
            hubContent.Close += HubContentClose;

            if (E.Language == "he-IL" || E.Language == "ar-SA")
            {
                hub.FlowDirection = System.Windows.FlowDirection.RightToLeft;
            }
            else
            {
                hub.FlowDirection = System.Windows.FlowDirection.LeftToRight;
            }

            hub.ShowDialog();
        }

        void HubContentClose(object sender, EventArgs e)
        {
            hubContent.Close -= HubContentClose;
            hub.Close();
        }
    }
}
