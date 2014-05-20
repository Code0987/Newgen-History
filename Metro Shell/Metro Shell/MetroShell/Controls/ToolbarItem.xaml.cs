using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Ftware.Apps.MetroShell.Controls
{
    /// <summary>
    /// Interaction logic for ToolbarItem.xaml
    /// </summary>
    public partial class ToolbarItem : UserControl
    {
        private bool mousePressed;

        public ToolbarItem()
        {
            InitializeComponent();
        }

        public string Title
        {
            get { return TitleTextBlock.Text; }
            set { TitleTextBlock.Text = value; }
        }

        public ImageSource Icon
        {
            get { return IconImage.Source; }
            set { IconImage.Source = value; }
        }

        private void UserControlMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mousePressed = true;
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                BlurRadius = 8,
                ShadowDepth = 0,
                Opacity = 0.9
            };
        }

        private void UserControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mousePressed = false;
            Effect = null;
        }

        private void UserControlMouseEnter(object sender, MouseEventArgs e)
        {
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                BlurRadius = 15,
                ShadowDepth = 0,
                Opacity = 0.9
            };
        }

        private void UserControlMouseLeave(object sender, MouseEventArgs e)
        {
            Effect = null;
            if (mousePressed)
            {
                mousePressed = false;
            }
        }
    }
}