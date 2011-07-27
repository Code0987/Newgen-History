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
using Microsoft.Win32;

namespace Desktop
{
    /// <summary>
    /// Interaction logic for DesktopWidget.xaml
    /// </summary>
    public partial class DesktopWidget : UserControl
    {
        public DesktopWidget()
        {
            InitializeComponent();
        }

        public void Load()
        {
            var wpReg = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", false);
            var wallpaperPath = wpReg.GetValue("WallPaper").ToString();
            wpReg.Close();

            if (!File.Exists(wallpaperPath))
                return;

            var bi = new BitmapImage();
            bi.BeginInit();
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.UriSource = new Uri(wallpaperPath, UriKind.Absolute);

            bi.EndInit();
            DesktopPreview.Source = bi;
        }

        private void UserControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var mosaicWindow = (Window)(((FrameworkElement)((FrameworkElement)((FrameworkElement)((FrameworkElement)((FrameworkElement)this.Parent).Parent).Parent).Parent).Parent).Parent); //lol
            mosaicWindow.Hide();
        }
    }
}
