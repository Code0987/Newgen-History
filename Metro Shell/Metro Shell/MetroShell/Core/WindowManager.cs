using System.Windows;
using Ftware.Apps.MetroShell.Base;

namespace Ftware.Apps.MetroShell.Core
{
    public class WindowManager
    {
        private Rect region;

        public MetroShellMatrix Matrix { get; private set; }

        public Rect Region { get { return region; } }

        public void Initialize(int c, int r)
        {
            region = new Rect();

            region.Height = SystemParameters.PrimaryScreenHeight - E.Margin.Top - E.Margin.Bottom;
            region.Width = SystemParameters.PrimaryScreenWidth;
            region.Y = E.Margin.Top;
            region.X = E.Margin.Left;

            E.ColumnsCount = c = (c == 0 || c < 5) ? 14 : c; //(int)Math.Round(region.Width * 2 / E.MinTileWidth);
            E.RowsCount = r = (r == 0 || r < 3) ? 140 : r; //(int)(SystemParameters.PrimaryScreenHeight / (E.MinTileHeight - E.TileSpacing * 2));

            Matrix = new MetroShellMatrix(E.ColumnsCount, E.RowsCount);
            Matrix.ZeroMatrix();
        }
    }
}