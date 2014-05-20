using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Ftware.Apps.MetroShell.Base;

namespace Weather
{
    /// <summary>
    /// Interaction logic for WeatherWidget.xaml
    /// </summary>
    public partial class WeatherWidget : UserControl
    {
        public static WeatherProvider WeatherProvider;
        private WeatherData currentWeather;
        private LocationData currentLocation;
        private Options optionsWindow;
        private DispatcherTimer weatherTimer;
        private DispatcherTimer tileAnimTimer;

        public WeatherWidget()
        {
            InitializeComponent();
        }

        public void Load()
        {
            WeatherProvider = new WeatherProvider();
            currentWeather = (WeatherData)XmlSerializable.Load(typeof(WeatherData), E.WidgetsRoot + "\\Weather\\Weather.data") ?? new WeatherData();
            currentLocation = new LocationData();
            currentLocation.Code = Widget.Settings.LocationCode;
            UpdateWeatherUI();

            weatherTimer = new DispatcherTimer();
            weatherTimer.Interval = TimeSpan.FromMinutes(Widget.Settings.RefreshInterval);
            weatherTimer.Tick += WeatherTimerTick;
            weatherTimer.Start();

            tileAnimTimer = new DispatcherTimer();
            tileAnimTimer.Interval = TimeSpan.FromSeconds(15);
            tileAnimTimer.Tick += TileAnimTimerTick;

            tileAnimTimer.Start();

            if (!string.IsNullOrEmpty(Widget.Settings.LocationCode))
                RefreshWeather();
        }

        private void TileAnimTimerTick(object sender, EventArgs e)
        {
            var s = (Storyboard)Resources["TileAnim"];
            s.Begin();
        }

        private void WeatherTimerTick(object sender, EventArgs e)
        {
            RefreshWeather();
        }

        private void RefreshWeather()
        {
            ThreadStart threadStarter = () =>
            {
                var w = WeatherProvider.GetWeatherReport(CultureInfo.CurrentCulture, currentLocation, Widget.Settings.TempScale);
                if (w != null)
                {
                    currentWeather = w;
                    this.Dispatcher.BeginInvoke((Action)UpdateWeatherUI);
                }

                //currentWeather.Save(E.WidgetsRoot + "\\Weather\\Weather.data");
            };
            var thread = new Thread(threadStarter);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private void UpdateWeatherUI()
        {
            Temperture.Text = currentWeather.Temperature + "° " + currentWeather.Curent.Text;
            if (currentWeather.ForecastList.Count < 3)
                return;
            FirstDay.Text = currentWeather.ForecastList[0].Day + ": " + currentWeather.ForecastList[0].HighTemperature + "° " + currentWeather.ForecastList[0].Text;
            SecondDay.Text = currentWeather.ForecastList[1].Day + ": " + currentWeather.ForecastList[1].HighTemperature + "° " + currentWeather.ForecastList[1].Text;
            ThirdDay.Text = currentWeather.ForecastList[2].Day + ": " + currentWeather.ForecastList[2].HighTemperature + "° " + currentWeather.ForecastList[2].Text;
            WeatherIcon.Source = new BitmapImage(new Uri(string.Format("/Weather;Component/Resources/weather_{0}.png", currentWeather.Curent.SkyCode), UriKind.Relative));
            Location.Text = currentWeather.Location.City;
        }

        public void Unload()
        {
            currentWeather.Save(E.WidgetsRoot + "\\Weather\\Weather.data");
            weatherTimer.Tick -= WeatherTimerTick;
            weatherTimer.Stop();
            tileAnimTimer.Tick -= TileAnimTimerTick;
            tileAnimTimer.Stop();
        }

        private void OptionsItemClick(object sender, RoutedEventArgs e)
        {
            if (optionsWindow != null && optionsWindow.IsVisible)
            {
                optionsWindow.Activate();
                return;
            }

            optionsWindow = new Options();
            optionsWindow.UpdateSettings += OptionsWindowUpdateSettings;

            optionsWindow.ShowDialog();
        }

        private void OptionsWindowUpdateSettings(object sender, EventArgs e)
        {
            optionsWindow.UpdateSettings -= OptionsWindowUpdateSettings;
            currentLocation.Code = Widget.Settings.LocationCode;
            RefreshWeather();

            weatherTimer.Stop();
            weatherTimer.Interval = TimeSpan.FromMinutes(Widget.Settings.RefreshInterval);
            weatherTimer.Start();
        }

        private void RefreshItemClick(object sender, RoutedEventArgs e)
        {
            RefreshWeather();
        }
    }
}