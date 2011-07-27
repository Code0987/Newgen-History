using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using Mosaic.Base;

namespace Mosaic.Core
{
    public class WindowManager
    {
        private Rect region;
        public MosaicMatrix Matrix { get; private set; }

        public Rect Region { get { return region; } }
        
        public void Initialize()
        {
            region = new Rect();

            region.Height = SystemParameters.PrimaryScreenHeight - E.Margin.Top - E.Margin.Bottom;
            region.Width = SystemParameters.PrimaryScreenWidth;
            E.RowsCount = (int)(SystemParameters.PrimaryScreenHeight / (E.MinTileHeight - E.TileSpacing * 2));
            //E.MinTileHeight = region.Height / E.RowsCount;
            //E.MinTileWidth = E.MinTileHeight;
            region.Y = E.Margin.Top;
            region.X = E.Margin.Left;
            E.ColumnsCount = (int)Math.Round(region.Width * 2 / E.MinTileWidth);

            Matrix = new MosaicMatrix(E.ColumnsCount, E.RowsCount);
            Matrix.ZeroMatrix();
        }
    }
}
