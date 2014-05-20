using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Ftware.Apps.MetroShell.Base;
using Microsoft.WindowsAPICodePack.Shell;
using Path = System.IO.Path;

namespace Video
{
    /// <summary>
    /// Interaction logic for Hub.xaml
    /// </summary>
    public partial class Hub : UserControl
    {
        private bool isplaying = false;
        private List<Category> categories;
        private string[] knownExts = new string[] { ".avi", ".wmv", ".mp4" };
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

                VideoPlayer_Player.MediaFailed += new EventHandler<ExceptionRoutedEventArgs>(VideoPlayer_Player_MediaFailed);
                VideoPlayer_Player.MediaEnded += new RoutedEventHandler(VideoPlayer_Player_MediaEnded);
                VideoPlayer_Player.MediaOpened += new RoutedEventHandler(VideoPlayer_Player_MediaOpened);
            }), 500);
        }

        private void VideoPlayer_Player_MediaOpened(object sender, RoutedEventArgs e)
        {
            VideoPlayer_PnPButton.Source = new BitmapImage(new Uri("/Video;component/Resources/pause.png", UriKind.Relative));
            isplaying = true;
        }

        private void VideoPlayer_Player_MediaEnded(object sender, RoutedEventArgs e)
        {
            try
            {
                isplaying = false;
                VideoPlayer_Player.Play();
                isplaying = true;
            }
            catch { }
        }

        private void VideoPlayer_Player_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            isplaying = false;
            MessageBox.Show("An error occurred. Cannot play Video !", "// MetroShell / : Error", MessageBoxButton.YesNo);
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

        internal void PlayVideo(Uri source)
        {
            try
            {
                VideoPlayer.Visibility = Visibility.Visible;
                VideoPlayer_Player.Source = source;
                VideoPlayer_Player.Play();
                isplaying = true;
            }
            catch { }
        }

        private void VideoPlayer_BackButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                VideoPlayer.Visibility = Visibility.Collapsed;
                VideoPlayer_Player.Stop();
                VideoPlayer_Player.Source = null;
            }
            catch { }
        }

        private void VideoPlayer_PnPButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (VideoPlayer_Player.CanPause && isplaying)
                {
                    VideoPlayer_Player.Pause();
                    VideoPlayer_PnPButton.Source = new BitmapImage(new Uri("/Video;component/Resources/play.png", UriKind.Relative));
                    isplaying = false;
                    return;
                }
                else if (VideoPlayer_Player.CanPause && !isplaying)
                {
                    VideoPlayer_Player.Play();
                    VideoPlayer_PnPButton.Source = new BitmapImage(new Uri("/Video;component/Resources/pause.png", UriKind.Relative));
                    isplaying = true;
                    return;
                }
            }
            catch { }
        }

        private void VideoPlayer_BkButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                for (int i = 0; i < categories.Count; i++)
                {
                    for (int io = 0; io < categories[i].Files.Count; io++)
                    {
                        if (VideoPlayer_Player.Source.OriginalString == categories[i].Files[io])
                        {
                            PlayVideo(new Uri(categories[i].Files[(io - 1 == -1) ? 0 : io - 1], UriKind.Absolute));
                        }
                    }
                }
            }
            catch { }
        }

        private void VideoPlayer_NextButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                for (int i = 0; i < categories.Count; i++)
                {
                    for (int io = 0; io < categories[i].Files.Count; io++)
                    {
                        if (VideoPlayer_Player.Source.OriginalString == categories[i].Files[io])
                        {
                            PlayVideo(new Uri(categories[i].Files[(io + 1 == categories[i].Files.Count) ? io : io + 1], UriKind.Absolute));
                        }
                    }
                }
            }
            catch { }
        }
    }
}