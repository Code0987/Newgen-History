using System.Windows;
using Newgen.Base;

namespace Newgen.Core
{
    public class WindowManager
    {
        private Rect region;

        public NewgenMatrix Matrix { get; private set; }

        public Rect Region { get { return region; } }

        public void Initialize(int c, int r)
        {
            region = new Rect();

            region.Height = SystemParameters.PrimaryScreenHeight - E.Margin.Top - E.Margin.Bottom;
            region.Width = SystemParameters.PrimaryScreenWidth;
            region.Y = E.Margin.Top;
            region.X = E.Margin.Left;

            E.ColumnsCount = c;//(int)Math.Round(region.Width * 2 / E.MinTileWidth);
            E.RowsCount = r;//(int)(SystemParameters.PrimaryScreenHeight / (E.MinTileHeight - E.TileSpacing * 2));

            Matrix = new NewgenMatrix(E.ColumnsCount, E.RowsCount);
            Matrix.ZeroMatrix();
        }
    }
}