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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.WindowsAPICodePack.Shell;
using Wrappers.WindowsMediaPlayer;

namespace Music
{
    /// <summary>
    /// Interaction logic for MusicWidget.xaml
    /// </summary>
    public partial class MusicWidget : UserControl
    {
        private string path = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        private WmpControl mediaPlayer;
        private List<string> albumArts;
        private Random r;
        private DispatcherTimer updateTimer;
        private int lastIndex;

        public MusicWidget()
        {
            InitializeComponent();
        }

        public void Load()
        {
            albumArts = new List<string>();
            r = new Random(Environment.TickCount);

            if (ShellLibrary.IsPlatformSupported)
            {
                var lib = ShellLibrary.Load(KnownFolders.MusicLibrary, true);
                foreach (var l in lib)
                {
                    foreach (var file in Directory.GetFiles(l.Path, "*.jpg", SearchOption.AllDirectories))
                    {
                        albumArts.Add(file);
                    }
                }
                lib.Dispose();
            }
            else
            {
                if (Directory.Exists(path))
                {
                    foreach (var f in Directory.GetFiles(path, "*.jpg", SearchOption.AllDirectories))
                    {
                        albumArts.Add(f);
                    }
                }
            }

            mediaPlayer = new WmpControl();
            mediaPlayer.CurrentMediaChanged += MediaPlayerCurrentMediaChanged;
            mediaPlayer.PlayStateChanged += MediaPlayerPlayStateChanged;

            if (mediaPlayer.CurrentMedia != null)
            {
                Artist.Text = mediaPlayer.CurrentMedia.Artist;
                SongTitle.Text = mediaPlayer.CurrentMedia.Title;

                var s = (Storyboard)Resources["PopupOpenAnim"];
                s.Begin();

                if (mediaPlayer.CurrentMedia.Picture != null)
                    AlbumArt.Source = mediaPlayer.CurrentMedia.Picture;
                else
                {
                    AlbumArt.Source = new BitmapImage(new Uri("Resources/zune_icon.png", UriKind.Relative));
                }

                if (mediaPlayer.IsPlaying)
                    PlayPause.IsChecked = true;
            }
            else
            {
                if (albumArts.Count > 0)
                    AlbumArt.Source = new BitmapImage(new Uri(albumArts[r.Next(albumArts.Count)]));
            }

            if (albumArts.Count > 0)
            {
                AlbumArt1.Source = new BitmapImage(new Uri(albumArts[r.Next(albumArts.Count)]));
                AlbumArt2.Source = new BitmapImage(new Uri(albumArts[r.Next(albumArts.Count)]));
                AlbumArt3.Source = new BitmapImage(new Uri(albumArts[r.Next(albumArts.Count)]));
                AlbumArt4.Source = new BitmapImage(new Uri(albumArts[r.Next(albumArts.Count)]));
            }

            updateTimer = new DispatcherTimer();
            updateTimer.Interval = TimeSpan.FromSeconds(15);
            updateTimer.Tick += new EventHandler(UpdateTimerTick);
            updateTimer.Start();
        }

        void UpdateTimerTick(object sender, EventArgs e)
        {
            if (albumArts.Count > 0)
            {
                var s = (Storyboard)Resources["AlbumArtFadeOut"];
                lastIndex = r.Next(4);
                switch (lastIndex)
                {
                    case 0:
                        s.Begin(AlbumArt1);
                        //AlbumArt1.Source = new BitmapImage(new Uri(albumArts[r.Next(albumArts.Count)]));
                        break;
                    case 1:
                        s.Begin(AlbumArt2);
                        //AlbumArt2.Source = new BitmapImage(new Uri(albumArts[r.Next(albumArts.Count)]));
                        break;
                    case 2:
                        s.Begin(AlbumArt3);
                        //AlbumArt3.Source = new BitmapImage(new Uri(albumArts[r.Next(albumArts.Count)]));
                        break;
                    case 3:
                        s.Begin(AlbumArt4);
                        //AlbumArt4.Source = new BitmapImage(new Uri(albumArts[r.Next(albumArts.Count)]));
                        break;
                }
            }
        }

        void MediaPlayerPlayStateChanged(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer.IsPlaying)
                PlayPause.IsChecked = true;
            else
                PlayPause.IsChecked = false;
        }

        void MediaPlayerCurrentMediaChanged(object sender, RoutedPropertyChangedEventArgs<WmpMediaItem> e)
        {
            if (mediaPlayer.CurrentMedia != null)
            {
                var s = (Storyboard)Resources["PopupOpenAnim"];
                s.Begin();
                if (mediaPlayer.CurrentMedia.Picture != null)
                {
                    AlbumArt.Source = mediaPlayer.CurrentMedia.Picture;
                }
                else
                {
                    AlbumArt.Source = new BitmapImage(new Uri("Resources/zune_icon.png", UriKind.Relative));
                }
                Artist.Text = mediaPlayer.CurrentMedia.Artist;
                SongTitle.Text = mediaPlayer.CurrentMedia.Title;
            }
        }

        public void Unload()
        {
            mediaPlayer.CurrentMediaChanged -= MediaPlayerCurrentMediaChanged;
            mediaPlayer.PlayStateChanged -= MediaPlayerPlayStateChanged;
            updateTimer.Stop();
        }

        private void PopupOpenAnimCompleted(object sender, EventArgs e)
        {
            var s = (Storyboard)Resources["PopupCloseAnim"];
            s.Begin();
        }

        private void UserControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mediaPlayer.SwitchToPlayerApplication();
        }

        private void UserControlMouseEnter(object sender, MouseEventArgs e)
        {
            if (mediaPlayer.CurrentMedia != null)
            {
                var s = (Storyboard)Resources["ShowControlsPanelAnim"];
                s.Begin();
            }
        }

        private void UserControlMouseLeave(object sender, MouseEventArgs e)
        {
            if (mediaPlayer.CurrentMedia != null)
            {
                var s = (Storyboard)Resources["HideControlsPanelAnim"];
                s.Begin();
            }
        }

        private void PrevClick(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer.CurrentMedia != null)
            {
                mediaPlayer.Previous();
            }
        }

        private void NextClick(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer.CurrentMedia != null)
            {
                mediaPlayer.Next();
            }
        }

        private void PlayPauseClick(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer.CurrentMedia != null)
            {
                if (mediaPlayer.IsPlaying)
                    mediaPlayer.Pause();
                else
                    mediaPlayer.Play();
            }
        }

        private void StoryboardCompleted(object sender, EventArgs e)
        {
            var s = (Storyboard)Resources["AlbumArtFadeIn"];
            switch (lastIndex)
            {
                case 0:
                    AlbumArt1.Source = new BitmapImage(new Uri(albumArts[r.Next(albumArts.Count)]));
                    s.Begin(AlbumArt1);
                    break;
                case 1:
                    AlbumArt2.Source = new BitmapImage(new Uri(albumArts[r.Next(albumArts.Count)]));
                    s.Begin(AlbumArt2);
                    break;
                case 2:
                    AlbumArt3.Source = new BitmapImage(new Uri(albumArts[r.Next(albumArts.Count)]));
                    s.Begin(AlbumArt3);
                    break;
                case 3:
                    AlbumArt4.Source = new BitmapImage(new Uri(albumArts[r.Next(albumArts.Count)]));
                    s.Begin(AlbumArt4);
                    break;
            }
        }
    }
}
