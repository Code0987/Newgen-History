using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ftware.Apps.MetroShell.Base;
using Microsoft.WindowsAPICodePack.Shell;
using Path = System.IO.Path;

namespace Pictures
{
    /// <summary>
    /// Interaction logic for Hub.xaml
    /// </summary>
    public partial class Hub : UserControl
    {
        private List<Category> categories;
        private string[] knownExts = new string[] { ".jpg", ".jpeg", ".png" };
        public event EventHandler Close;

        public Hub()
        {
            InitializeComponent();
        }

        private void UserControlLoaded(object sender, RoutedEventArgs e)
        {
            Helper.Delay(new Action(() =>
               {
                   categories = new List<Category>();

                   if (!ShellLibrary.IsPlatformSupported)
                   {
                       FindFiles(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
                   }
                   else
                   {
                       var lib = ShellLibrary.Load(KnownFolders.PicturesLibrary, true);
                       foreach (var l in lib)
                       {
                           FindFiles(l.Path);
                       }
                       lib.Dispose();
                   }

                   foreach (var category in categories)
                   {
                       var control = new PicturesCategoryControl();
                       control.Initialize(category);
                       PicturessPanel.Children.Add(control);
                   }
               }), 500);
        }

        private void BackButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Close(this, EventArgs.Empty);
        }

        private void FindFiles(string path)
        {
            foreach (var file in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
            {
                FileInfo fi = new FileInfo(file);
                if (!knownExts.Contains(Path.GetExtension(file)))
                    continue;
                var f = fi.CreationTimeUtc.Year;
                var cat = categories.Find(x => x.Title.ToString().ToUpper() == f.ToString().ToUpper());
                if (cat == null)
                {
                    var newCat = new Category();
                    newCat.Title = f.ToString().ToUpper();
                    newCat.Files.Add(file);
                    categories.Add(newCat);
                }
                else
                {
                    cat.Files.Add(file);
                }
            }
        }

        private string currentimg;

        private void FWButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                for (int i = 0; i < categories.Count; i++)
                {
                    for (int io = 0; io < categories[i].Files.Count; io++)
                    {
                        if (currentimg == categories[i].Files[io])
                        {
                            OpenImg(new Uri(categories[i].Files[(io - 1 == -1) ? 0 : io - 1], UriKind.Absolute));
                        }
                    }
                }
            }
            catch { }
        }

        private void BkButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                for (int i = 0; i < categories.Count; i++)
                {
                    for (int io = 0; io < categories[i].Files.Count; io++)
                    {
                        if (currentimg == categories[i].Files[io])
                        {
                            OpenImg(new Uri(categories[i].Files[(io + 1 == categories[i].Files.Count) ? io : io + 1], UriKind.Absolute));
                        }
                    }
                }
            }
            catch { }
        }

        private void BackVButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ImgV.Visibility = Visibility.Collapsed;
            }
            catch { }
        }

        internal void OpenImg(Uri source)
        {
            try
            {
                currentimg = source.OriginalString;
                ImgV.Visibility = Visibility.Visible;
                Vw_Img.Source = new System.Windows.Media.Imaging.BitmapImage(source);
            }
            catch { }
        }
    }
}