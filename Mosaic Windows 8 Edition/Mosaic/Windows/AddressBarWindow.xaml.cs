﻿using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Mosaic.Windows
{
    /// <summary>
    /// Interaction logic for AddressBoxWindow.xaml
    /// </summary>
    public partial class AddressBarWindow : Window
    {
        public AddressBarWindow()
        {
            InitializeComponent();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Top = -60;
            this.Left = 0;
            AddressBox.Focus();
            AddressBox.CaretIndex = AddressBox.Text.Length;
            OpenAnim();
        }

        private void WindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                AddressBox.Text = string.Empty;
                CloseAnim();
            }
        }

        private void AddressBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                CloseAnim();
        }

        private void AddressBarCloseAnimCompleted(object sender, EventArgs e)
        {
            this.Close();
        }

        private void OpenAnim()
        {
            var s = (Storyboard)Resources["OpenAnim"];
            s.Begin();
        }

        private void CloseAnim()
        {
            var s = (Storyboard)Resources["CloseAnim"];
            s.Begin();
        }
    }
}