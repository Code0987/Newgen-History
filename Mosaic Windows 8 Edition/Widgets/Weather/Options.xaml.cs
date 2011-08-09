using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Mosaic.Base;

namespace Weather
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class Options : Window
    {
        public event EventHandler UpdateSettings;
        private List<LocationData> locations;
        private LocationData currentLocation;

        public Options()
        {
            InitializeComponent();
        }

        private void WindowSourceInitialized(object sender, EventArgs e)
        {
            this.Top = 0;
            this.Left = 0;
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;

            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var fileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
            BuildTag.Text = version + ".alpha." + fileInfo.LastWriteTimeUtc.ToString("yyMMdd-HHmm");

            ShowFeelsLikeCheckBox.IsChecked = Widget.Settings.ShowFeelsLike;
            if (Widget.Settings.TempScale == TemperatureScale.Celsius)
                CelsiusRadioButton.IsChecked = true;

            WeatherIntervalSlider.Value = Widget.Settings.RefreshInterval;
        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            ApplySettings();
            this.Close();
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ApplyButtonClick(object sender, RoutedEventArgs e)
        {
            ApplySettings();
        }

        private void SiteLinkMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            WinAPI.ShellExecute(IntPtr.Zero, "open", "http://mosaicproject.codeplex.com", string.Empty, string.Empty, 0);
        }

        private void ApplySettings()
        {
            if (currentLocation != null)
            {
                Widget.Settings.LocationCode = currentLocation.Code;
            }
            Widget.Settings.ShowFeelsLike = (bool) ShowFeelsLikeCheckBox.IsChecked;
            Widget.Settings.TempScale = (bool)CelsiusRadioButton.IsChecked ? TemperatureScale.Celsius : TemperatureScale.Fahrenheit;

            Widget.Settings.Save(E.WidgetsRoot + "\\Weather\\Weather.config");
            if (UpdateSettings != null)
            {
                UpdateSettings(null, EventArgs.Empty);
            }
        }

        private void CheckBoxClick(object sender, RoutedEventArgs e)
        {

        }

        private void SearchBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (SearchBox.Foreground == Brushes.Gray)
            {
                SearchBox.Text = "";
                SearchBox.FontStyle = FontStyles.Normal;
                SearchBox.Foreground = Brushes.Black;
            }
            if (e.Key == Key.Enter && !string.IsNullOrEmpty(SearchBox.Text) && SearchBox.Text.Length > 2)
            {

                SearchPopup.IsOpen = true;
                SearchResultBox.Items.Clear();
                var query = SearchBox.Text;
                ThreadStart threadStarter =
                    () => GetLocations(query);
                var thread = new Thread(threadStarter);
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                //SearchButton.Source = new BitmapImage(new Uri("/Resources/Weather/SearchCancelIcon.png", UriKind.Relative));
            }
        }

        private void SearchBoxIsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                if (string.IsNullOrEmpty(SearchBox.Text))
                {
                    SearchBox.Text = Properties.Resources.OptionsSearchBox;
                    SearchBox.FontStyle = FontStyles.Italic;
                    SearchBox.Foreground = Brushes.Gray;
                }
            }
            else
            {
                if (SearchBox.Text == Properties.Resources.OptionsSearchBox)
                {
                    SearchBox.Text = "";
                    SearchBox.FontStyle = FontStyles.Normal;
                    SearchBox.Foreground = Brushes.Black;
                }
            }
        }

        private void SearchResultBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchResultBox.SelectedIndex == -1)
                return;
            if (SearchResultBox.SelectedIndex > locations.Count)
                currentLocation = locations[locations.Count - 1];
            else
                currentLocation = locations[SearchResultBox.SelectedIndex];
            SearchBox.Text = currentLocation.City;
            SearchPopup.IsOpen = false;
            SearchResultBox.Items.Clear();
        }

        private void GetLocations(string query)
        {
            locations = WeatherWidget.WeatherProvider.GetLocations(query, CultureInfo.GetCultureInfo(E.Language));
            if (locations != null && locations.Count > 0)
            {
                foreach (var location in locations)
                {
                    SearchResultBox.Dispatcher.Invoke((Action)delegate
                    {
                        SearchResultBox.Items.Add(location);
                    });
                }
            }
            else
            {
                SearchPopup.Dispatcher.Invoke((Action)delegate
                {
                    SearchPopup.IsOpen = false;
                });
            }
        }

        private void WeatherIntervalSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (WeatherIntervalSlider.Value < 60)
            {
                WeatherIntervalValueTextBlock.Text = WeatherIntervalSlider.Value + " " + Properties.Resources.OptionsIntervalMinutes;
            }
            else if (WeatherIntervalSlider.Value == 60)
            {
                WeatherIntervalValueTextBlock.Text = 1 + " " + Properties.Resources.OptionsIntervalHours;
            }
            else
            {
                WeatherIntervalValueTextBlock.Text = string.Format("{0} {1} {2} {3}", Math.Truncate(WeatherIntervalSlider.Value / 60), Properties.Resources.OptionsIntervalHours,
                    Math.Abs(Math.IEEERemainder(WeatherIntervalSlider.Value, 60)), Properties.Resources.OptionsIntervalMinutes);
            }
        }

        private void BackButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
    }
}
