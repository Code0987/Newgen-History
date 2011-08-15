using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newgen.Base;
using Image = System.Windows.Controls.Image;

namespace Newgen.Core
{
    public class NewgenAppWidget : NewgenWidget
    {
        private string file;
        private Grid root;
        private Image icon;
        private TextBlock title;

        public override string Name
        {
            get { return null; }
        }

        public override FrameworkElement WidgetControl
        {
            get { return root; }
        }

        public override Uri IconPath
        {
            get { return null; }
        }

        public override int ColumnSpan
        {
            get { return 1; }
        }

        public override void Load(string path)
        {
            if (!File.Exists(path))
                return;
            file = path;
            root = new Grid();
            icon = new Image();
            icon.Width = 32;
            icon.Height = 32;

            //ImageBrush ib = new ImageBrush();
            //ib.ImageSource = new BitmapImage(new Uri(E.Root + "\\AppIcons\\BG.png"));
            //var bgBrush = new Image();
            /* bgBrush.StartPoint = new Point(0, 0);
            bgBrush.EndPoint = new Point(1, 0);
            bgBrush.GradientStops.Add(new GradientStop((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#280a37"), 0));
            bgBrush.GradientStops.Add(new GradientStop((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2f0d40"), 1)); */
            //root.Background = ib;

            var source = GetIcon(file);
            icon.Source = source;
            icon.VerticalAlignment = VerticalAlignment.Bottom;
            icon.HorizontalAlignment = HorizontalAlignment.Left;
            icon.Margin = new Thickness(12);
            RenderOptions.SetBitmapScalingMode(icon, BitmapScalingMode.HighQuality);
            root.Children.Add(icon);

            root.Background = new SolidColorBrush(CalcAverageColor(source));

            var f = FileVersionInfo.GetVersionInfo(file);
            title = new TextBlock();
            title.Foreground = System.Windows.Media.Brushes.White;
            title.Margin = new Thickness(12, 20, 12, 40);
            title.VerticalAlignment = VerticalAlignment.Top;
            title.HorizontalAlignment = HorizontalAlignment.Left;
            title.Text = f.FileDescription;
            title.FontSize = 24;
            title.FontWeight = FontWeights.Light;
            title.TextWrapping = TextWrapping.WrapWithOverflow;
            title.TextTrimming = TextTrimming.CharacterEllipsis;
            root.Children.Add(title);

            root.MouseLeftButtonUp += RootMouseLeftButtonUp;
        }

        private void RootMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                WinAPI.ShellExecute(IntPtr.Zero, "open", file, null, null, 3);
            }
            catch
            {
            }
        }

        private MemoryStream iconStream;

        private BitmapSource GetIcon(string file)
        {
            if (File.Exists(E.Root + "\\AppIcons\\" + Path.GetFileNameWithoutExtension(file) + ".png"))
            {
                return new BitmapImage(new Uri(E.Root + "\\AppIcons\\" + Path.GetFileNameWithoutExtension(file) + ".png"));
            }

            Icon icon = null;
            Icon[] splitIcons = null;

            using (var extractor = new IconExtractor(file))
            {
                icon = extractor.GetIcon(0);
            }
            splitIcons = IconExtractor.SplitIcon(icon);
            if (splitIcons == null)
                return null;
            foreach (var splitIcon in splitIcons)
            {
                if (splitIcon.Width == 256 && splitIcon.Height == 256)
                {
                    icon = splitIcon;
                    continue;
                }
                if (icon == null && splitIcon.Width == 32 && splitIcon.Height == 32)
                {
                    icon = splitIcon;
                    continue;
                }
                splitIcon.Dispose();
            }
            iconStream = new MemoryStream();
            icon.Save(iconStream);
            iconStream.Seek(0, SeekOrigin.Begin);
            return BitmapFrame.Create(iconStream);
        }

        public Bitmap ToBitmap(BitmapSource source)
        {
            var bmp = new Bitmap(source.PixelWidth, source.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            var data = bmp.LockBits(new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            source.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
            bmp.UnlockBits(data);
            return bmp;
        }

        public override void Unload()
        {
            if (iconStream != null)
                iconStream.Dispose();

            root.MouseLeftButtonUp -= RootMouseLeftButtonUp;
        }

        private static System.Windows.Media.Color CalcAverageColor(BitmapSource image)
        {
            var c = CalcAverageColor(BitmapSourceToBitmap(image));
            return System.Windows.Media.Color.FromArgb(Math.Max(c.A, (byte)150), c.R, c.G, c.B);
        }

        private static System.Drawing.Color CalcAverageColor(Bitmap image)
        {
            var bmp = new Bitmap(1, 1);
            var orig = image;
            using (var g = Graphics.FromImage(bmp))
            {
                // the Interpolation mode needs to be set to
                // HighQualityBilinear or HighQualityBicubic or this method
                // doesn't work at all.  With either setting, the results are
                // slightly different from the averaging method.
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(orig, new System.Drawing.Rectangle(0, 0, 1, 1));
            }
            var pixel = bmp.GetPixel(0, 0);
            orig.Dispose();
            bmp.Dispose();
            return pixel;
        }

        public static System.Drawing.Bitmap BitmapSourceToBitmap(BitmapSource bitmapSource)
        {
            int width = bitmapSource.PixelWidth;
            int height = bitmapSource.PixelHeight;
            int stride = width * ((bitmapSource.Format.BitsPerPixel + 7) / 8);
            byte[] bits = new byte[height * stride];
            bitmapSource.CopyPixels(bits, stride, 0);
            unsafe
            {
                fixed (byte* pBits = bits)
                {
                    IntPtr ptr = new IntPtr(pBits);
                    System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(width, height, stride, System.Drawing.Imaging.PixelFormat.Format32bppPArgb, ptr);
                    return bitmap;
                }
            }
        }
    }
}