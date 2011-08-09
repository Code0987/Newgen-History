using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
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
            wallpaperPath = wpReg.GetValue("WallPaper").ToString();
            wpReg.Close();
            UpdateWallpaper();
            iFr.Helper.RunFor(new Action(UpdateWallpaper), -1, 2000);
        }

        private void UserControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var mosaicWindow = (Window)(Application.Current.MainWindow); //lol
            mosaicWindow.Hide();
        }

        private string wallpaperPath;

        private void UpdateWallpaper()
        {
            DesktopPreview.Source = null;

            if (!File.Exists(wallpaperPath))
            { return; }

            //var bi = new BitmapImage();
            //bi.BeginInit();
            //bi.CacheOption = BitmapCacheOption.OnLoad;
            //bi.UriSource = new Uri(wallpaperPath, UriKind.Absolute);
            //bi.EndInit();

            MemoryStream ms = new MemoryStream();
            BitmapImage bi = new BitmapImage();
            byte[] bytArray = File.ReadAllBytes(wallpaperPath);
            ms.Write(bytArray, 0, bytArray.Length); ms.Position = 0;
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();

            DesktopPreview.Source = bi;
        }
    }
}