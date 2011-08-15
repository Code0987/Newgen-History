using System;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Newgen.Controls
{
    /// <summary>
    /// Interaction logic for BottomToolbar.xaml
    /// </summary>
    public partial class BottomToolbar : UserControl
    {
        public BottomToolbar()
        {
            InitializeComponent();
        }

        public void OpenToolbar()
        {
            var s = (Storyboard)Resources["OpenAnim"];
            s.Begin();
        }

        public void CloseToolbar()
        {
            var s = (Storyboard)Resources["CloseAnim"];
            s.Begin();
        }

        private void CloseAnimCompleted(object sender, EventArgs e)
        {
            ((Canvas)Parent).Children.Remove(this);
        }
    }
}