using System;
using System.Collections.Generic;
using System.IO;
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
using Microsoft.WindowsAPICodePack.Shell;
using Path = System.IO.Path;

namespace Video
{
    /// <summary>
    /// Interaction logic for Hub.xaml
    /// </summary>
    public partial class Hub : UserControl
    {
        private List<Category> categories;
        private string[] knownExts = new string[] { ".avi", ".wmv" };
        public event EventHandler Close;

        public Hub()
        {
            InitializeComponent();
        }

        private void UserControlLoaded(object sender, RoutedEventArgs e)
        {
            categories = new List<Category>();

            if (!ShellLibrary.IsPlatformSupported)
            {
                FindFiles(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos));
            }
            else
            {
                var lib = ShellLibrary.Load(KnownFolders.VideosLibrary, true);
                foreach (var l in lib)
                {
                    FindFiles(l.Path);
                }
                lib.Dispose();
            }

            foreach (var category in categories)
            {
                var control = new VideoCategoryControl();
                control.Initialize(category);
                VideosPanel.Children.Add(control);
            }
        }

        private void BackButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Close(this, EventArgs.Empty);
        }

        private void FindFiles(string path)
        {
            foreach (var file in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
            {
                if (!knownExts.Contains(Path.GetExtension(file)))
                    continue;
                var f = System.IO.Path.GetFileNameWithoutExtension(file);
                var cat = categories.Find(x => x.Title == f[0]);
                if (cat == null)
                {
                    var newCat = new Category();
                    newCat.Title = f[0];
                    newCat.Files.Add(file);
                    categories.Add(newCat);
                }
                else
                {
                    cat.Files.Add(file);
                }
            }
        }
    }
}
