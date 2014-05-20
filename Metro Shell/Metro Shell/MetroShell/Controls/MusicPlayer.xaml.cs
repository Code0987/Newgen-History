using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Ftware.Apps.MetroShell.Base;

namespace Ftware.Apps.MetroShell.Controls
{
    /// <summary>
    /// Interaction logic for MusicPlayer.xaml
    /// </summary>
    public partial class MusicPlayer : UserControl
    {
        private MediaPlayer player;
        private bool isplaying;
        private List<string> musiclist;
        private int mediaindex;

        public MusicPlayer()
        {
            InitializeComponent();

            player = new MediaPlayer();
            isplaying = false;
            player.Volume = 1.0;

            musiclist = new List<string>();

            mediaindex = 0;
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation()
            {
                From = this.ActualWidth,
                To = -this.ActualWidth,
                Duration = new Duration(TimeSpan.FromMilliseconds(10000)),
                RepeatBehavior = RepeatBehavior.Forever
            };
            Storyboard storyboard = new Storyboard();
            Storyboard.SetTarget(doubleAnimation, TextBlock_MediaInfo);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(Canvas.LeftProperty));
            storyboard.Children.Add(doubleAnimation);
            storyboard.Begin();

            Helper.Animate(TextBlock_MediaInfo, OpacityProperty, 150, 0.5);

            Helper.RunMethodAsync(() =>
            {
                WMPLib.WindowsMediaPlayerClass pl = new WMPLib.WindowsMediaPlayerClass();
                WMPLib.IWMPPlaylistArray playlist = pl.playlistCollection.getAll();

                for (int ip = 0; ip < playlist.count; ip++)
                {
                    var list = playlist.Item(ip);
                    for (int il = 0; il < list.count; il++)
                    {
                        var item = list.get_Item(il);
                        Helper.RunMethodAsync(() => musiclist.Add(item.sourceURL));
                    }
                }
            });
        }

        private void Image_PlayPause_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isplaying)
            {
                Image_PlayPause.Source = new BitmapImage(new Uri("/Resources/play.png", UriKind.Relative));
                player.Pause();
                if (player.Source != null)
                {
                    TextBlock_MediaInfo.Text = "Paused - " + (new FileInfo(player.Source.OriginalString)).Name;
                }
                else
                {
                    TextBlock_MediaInfo.Text = "Paused";
                }
                isplaying = false;
            }
            else
            {
                Image_PlayPause.Source = new BitmapImage(new Uri("/Resources/pause.png", UriKind.Relative));
                if (musiclist.Count != 0)
                {
                    player.Open(new Uri(musiclist[mediaindex]));
                    player.Play();
                    isplaying = true;

                    TextBlock_MediaInfo.Text = "Playing - " + (new FileInfo(player.Source.OriginalString)).Name;
                }
                else
                {
                    TextBlock_MediaInfo.Text = "No file to play.";
                }
            }
        }

        private void Image_Back_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((mediaindex - 1) > -1)
            {
                player.Open(new Uri(musiclist[(mediaindex - 1)]));
                mediaindex--;
                player.Play();
                isplaying = true;

                TextBlock_MediaInfo.Text = "Playing - " + (new FileInfo(player.Source.OriginalString)).Name;
            }
        }

        private void Image_Next_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((mediaindex + 1) < musiclist.Count)
            {
                player.Open(new Uri(musiclist[(mediaindex + 1)]));
                mediaindex++;
                player.Play();
                isplaying = true;

                TextBlock_MediaInfo.Text = "Playing - " + (new FileInfo(player.Source.OriginalString)).Name;
            }
        }

        private void TextBlock_MediaInfo_MouseEnter(object sender, MouseEventArgs e)
        {
            Helper.Animate(TextBlock_MediaInfo, OpacityProperty, 150, 0.5, 1);
        }

        private void TextBlock_MediaInfo_MouseLeave(object sender, MouseEventArgs e)
        {
            Helper.Animate(TextBlock_MediaInfo, OpacityProperty, 150, 0.5);
        }
    }
}