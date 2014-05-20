using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using Ftware.Apps.MetroShell.Base;

namespace Ftware.Apps.MetroShell.Controls
{
    /// <summary>
    /// Interaction logic for NotesPane.xaml
    /// </summary>
    public partial class NotesPane : UserControl
    {
        private string deffile = @"\\Notes.txt";

        public NotesPane()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            InitFiles();

            try
            {
                TextBox_Note.Text = File.ReadAllText(E.CacheRoot + deffile);
            }
            catch { }

            Helper.Animate(this, OpacityProperty, 150, 0.5);
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            Helper.Animate(this, OpacityProperty, 150, 0.5, 1);
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            Helper.Animate(this, OpacityProperty, 150, 0.5);
        }

        private void TextBox_Note_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                InitFiles();
                File.WriteAllText(E.CacheRoot + deffile, TextBox_Note.Text);
            }
            catch { }
        }

        private void InitFiles()
        {
            try
            {
                if (!File.Exists(E.CacheRoot + deffile))
                {
                    File.Create(E.CacheRoot + deffile);
                    File.WriteAllText(E.CacheRoot + deffile, "Notes\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n");
                }
            }
            catch { }
        }
    }
}