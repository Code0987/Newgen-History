using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
            try
            {
                DesktopPreview.Source = null;

                if (!File.Exists(wallpaperPath))
                { return; }

                DesktopPreview.Source = Mosaic.Base.E.GetBitmap(wallpaperPath);
            }
            catch (Exception)
            {
            }
        }
    }
}