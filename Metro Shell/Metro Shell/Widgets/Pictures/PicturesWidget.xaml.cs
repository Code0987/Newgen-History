using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Ftware.Apps.MetroShell.Base;
using Microsoft.WindowsAPICodePack.Shell;

namespace Pictures
{
    /// <summary>
    /// Interaction logic for PicturesWidget.xaml
    /// </summary>
    public partial class PicturesWidget : UserControl
    {
        private string path = Environment.GetFolderPath(Environment.SpecialFolder.Windows) + @"\Web\Wallpaper";
        private List<string> pictures = new List<string>();
        private Random random;
        private DispatcherTimer timer;
        private HubWindow hub;
        private Hub hubContent;

        public PicturesWidget()
        {
            InitializeComponent();
        }

        public void Load()
        {
            try
            {
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(10);
                timer.Tick += new EventHandler(TimerTick);
                timer.Start();

                pictures = new List<string>();
                if (!ShellLibrary.IsPlatformSupported)
                {
                    if (Directory.Exists(path))
                    {
                        var pics = from x in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                                   where x.ToLower().EndsWith("jpg") || x.ToLower().EndsWith("png")
                                   select x;
                        foreach (var p in pics)
                        {
                            pictures.Add(p);
                        }
                    }
                }
                else
                {
                    var lib = ShellLibrary.Load(KnownFolders.PicturesLibrary, true);
                    foreach (var l in lib)
                    {
                        pictures.AddRange(from x in Directory.GetFiles(l.Path, "*.*", SearchOption.AllDirectories) where x.ToLower().EndsWith("jpg") || x.ToLower().EndsWith("png") select x);
                    }
                    lib.Dispose();
                }

                random = new Random(Environment.TickCount);
                if (pictures.Count > 0)
                {
                    //Picture.Source = new BitmapImage(new Uri(pictures[random.Next(0, pictures.Count - 1)]));
                    LoadPicture(pictures[random.Next(0, pictures.Count - 1)], Picture);
                }
            }
            catch { }
        }

        private void LoadPicture(string path, Image image)
        {
            try
            {
                var bi = new BitmapImage();
                bi.BeginInit();

                bi.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);

                bi.DecodePixelWidth = (int)E.MinTileWidth * 2;
                bi.EndInit();

                image.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate()
                {
                    try
                    {
                        image.Source = bi;
                    }
                    catch
                    {
                    }
                });
            }
            catch { }
        }

        private void TimerTick(object sender, EventArgs e)
        {
            try
            {
                if (pictures.Count <= 0) return;
                LoadPicture(pictures[random.Next(0, pictures.Count - 1)], PictureBg);

                var s = (Storyboard)Resources["SwitchPictureAnim"];
                s.Begin();
            }
            catch { }
        }

        private void SwitchAnimationCompleted(object sender, EventArgs e)
        {
            try
            {
                Picture.Source = PictureBg.Source;
                PictureBg.Source = null;
            }
            catch { }
        }

        private void UserControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (hub != null && hub.IsVisible)
            {
                hub.Activate();
                return;
            }

            hub = new HubWindow();
            hub.AllowsTransparency = true;
            hubContent = new Hub();
            hub.Content = hubContent;
            hubContent.Close += HubContentClose;

            hub.ShowDialog();
        }

        private void HubContentClose(object sender, EventArgs e)
        {
            hubContent.Close -= HubContentClose;
            hub.Close();
        }
    }
}