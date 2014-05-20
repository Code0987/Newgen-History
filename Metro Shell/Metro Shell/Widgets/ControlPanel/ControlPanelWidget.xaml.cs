using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;

namespace ControlPanel
{
    /// <summary>
    /// Interaction logic for ControlPanelWidget.xaml
    /// </summary>
    public partial class ControlPanelWidget : UserControl
    {
        public ControlPanelWidget()
        {
            InitializeComponent();
        }

        private void UserControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("control.exe");
        }
    }
}