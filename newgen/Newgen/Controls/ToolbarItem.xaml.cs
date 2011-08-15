﻿using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Newgen.Controls
{
    /// <summary>
    /// Interaction logic for ToolbarItem.xaml
    /// </summary>
    public partial class ToolbarItem : UserControl
    {
        private bool mousePressed;

        public ToolbarItem()
        {
            InitializeComponent();
        }

        public string Title
        {
            get { return TitleTextBlock.Text; }
            set { TitleTextBlock.Text = value; }
        }

        public ImageSource Icon
        {
            get { return IconImage.Source; }
            set { IconImage.Source = value; }
        }

        private void UserControlMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mousePressed = true;
            Background = Brushes.Gray;
        }

        private void UserControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mousePressed = false;
            Background = Brushes.Transparent;
        }

        private void UserControlMouseLeave(object sender, MouseEventArgs e)
        {
            if (mousePressed)
            {
                mousePressed = false;
                Background = Brushes.Transparent;
            }
        }
    }
}