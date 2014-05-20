/*
File:           Helper.cs
Version:        1.1
Last changed:   2012-08-06
Author:         Neeraj Durgapal
Copyright:      © NS 2012 , © nscoder 2012
*/

using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Microsoft.Win32;

namespace Ftware.Apps.MetroShell.Base
{
    public static partial class Helper
    {
        #region DoubleAnimation

        public static void Animate(DependencyObject dependencyobject, DependencyProperty dependencyproperty, int duration, double to, bool usedlbonly = false)
        {
            Animate(dependencyobject, dependencyproperty, duration, null, to, null, 0.0, 0.0, false, 1.0, null, FillBehavior.HoldEnd, null, null, usedlbonly);
        }

        public static void Animate(DependencyObject dependencyobject, DependencyProperty dependencyproperty, int duration, double to, double accelerationratio, double deaccelerationratio, bool usedlbonly = false)
        {
            Animate(dependencyobject, dependencyproperty, duration, null, to, null, accelerationratio, deaccelerationratio, false, 1.0, null, FillBehavior.HoldEnd, null, null, usedlbonly);
        }

        public static void Animate(DependencyObject dependencyobject, DependencyProperty dependencyproperty, int duration, double @from, double to, bool usedlbonly = false)
        {
            Animate(dependencyobject, dependencyproperty, duration, @from, to, null, 0.0, 0.0, false, 1.0, null, FillBehavior.HoldEnd, null, null, usedlbonly);
        }

        public static void Animate(DependencyObject dependencyobject, DependencyProperty dependencyproperty, int duration, double @from, double to, double accelerationratio, double deaccelerationratio, bool usedlbonly = false)
        {
            Animate(dependencyobject, dependencyproperty, duration, @from, to, null, accelerationratio, deaccelerationratio, false, 1.0, null, FillBehavior.HoldEnd, null, null, usedlbonly);
        }

        public static void Animate(DependencyObject dependencyobject, DependencyProperty dependencyproperty, int duration, double @from, double to, EventHandler callback, bool usedlbonly = false)
        {
            Animate(dependencyobject, dependencyproperty, duration, @from, to, null, 0.0, 0.0, false, 1.0, null, FillBehavior.HoldEnd, callback, null, usedlbonly);
        }

        public static void Animate(DependencyObject dependencyobject,
      DependencyProperty dependencyproperty,
      double duration,
      System.Nullable<double> @from,
      System.Nullable<double> to,
      System.Nullable<double> by,
      double accelerationratio,
      double deaccelerationratio,
      bool additive,
      double speedratio,
      IEasingFunction easing,
      FillBehavior fillbehavior,
      EventHandler callback,
      System.Nullable<int> framerate,
      bool usedlbonly = false)
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation()
            {
                From = @from,
                To = to,
                Duration = new Duration(TimeSpan.FromMilliseconds(duration)),
                EasingFunction = easing,
                AccelerationRatio = accelerationratio,
                DecelerationRatio = deaccelerationratio,
                FillBehavior = fillbehavior,
                SpeedRatio = speedratio,
                IsAdditive = additive,
                By = by
            };

            if (!usedlbonly)
            {
                Storyboard storyboard = new Storyboard();
                Storyboard.SetTarget(doubleAnimation, dependencyobject);
                Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(dependencyproperty));
                storyboard.Children.Add(doubleAnimation);
                Storyboard.SetDesiredFrameRate(storyboard, framerate);
                if (callback != null) { storyboard.Completed += callback; }
                storyboard.Begin();
            }
            else
            {
                try { ((IAnimatable)dependencyobject).BeginAnimation(dependencyproperty, doubleAnimation); }
                catch { }
            }
        }

        #endregion DoubleAnimation

        #region Timer

        /// <summary>
        /// Runs specified <see cref="Action"/> for given count with timeframe as tick time (duration/delay) for each count. Use -1 for infinite. Although you can control timer manually.
        /// </summary>
        /// <param name="work">The work.</param>
        /// <param name="count">The count.</param>
        /// <param name="timeframe">The timeframe.</param>
        /// <returns></returns>
        public static DispatcherTimer RunFor(Action work, int count, double timeframe)
        {
            DispatcherTimer timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(timeframe)
            };
            int rcount = 0;
            timer.Tick += (o, e) =>
            {
                if (rcount <= count || count == -1)
                {
                    work.Invoke();
                }
                else
                {
                    timer.Stop();
                    timer = null;
                }

                rcount++;
            };

            timer.Start();
            return timer;
        }

        /// <summary>
        /// Delays the specified work.
        /// </summary>
        /// <param name="work">The work.</param>
        /// <param name="time">The time.</param>
        public static void Delay(Action work, double time)
        {
            DispatcherTimer timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(time)
            };
            timer.Tick += (o, e) =>
            {
                work.Invoke();
                timer.Stop();
                timer = null;
            };
            timer.Start();
        }

        #endregion Timer

        #region En/De

        private static bool useHashing = true;
        private static string CryptKey = "iFr.Helper";

        public static string Encrypt(string toEncrypt) { if (string.IsNullOrEmpty(toEncrypt)) { return ""; } byte[] keyArray; byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt); System.Configuration.AppSettingsReader settingsReader = new AppSettingsReader(); string key = CryptKey; if (useHashing) { MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider(); keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key)); hashmd5.Clear(); } else { keyArray = UTF8Encoding.UTF8.GetBytes(key); } TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider(); tdes.Key = keyArray; tdes.Mode = CipherMode.ECB; tdes.Padding = PaddingMode.PKCS7; ICryptoTransform cTransform = tdes.CreateEncryptor(); byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length); tdes.Clear(); return System.Convert.ToBase64String(resultArray, 0, resultArray.Length); }

        public static string Decrypt(string cipherString) { if (string.IsNullOrEmpty(cipherString)) { return ""; } byte[] keyArray; byte[] toEncryptArray = System.Convert.FromBase64String(cipherString); System.Configuration.AppSettingsReader settingsReader = new AppSettingsReader(); string key = CryptKey; if (useHashing) { MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider(); keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key)); hashmd5.Clear(); } else { keyArray = UTF8Encoding.UTF8.GetBytes(key); } TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider(); tdes.Key = keyArray; tdes.Mode = CipherMode.ECB; tdes.Padding = PaddingMode.PKCS7; ICryptoTransform cTransform = tdes.CreateDecryptor(); byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length); tdes.Clear(); return UTF8Encoding.UTF8.GetString(resultArray); }

        #endregion En/De

        #region Other

        public static void RunMethodAsyncThreadSafe(Action method)
        {
            Application.Current.Dispatcher.BeginInvoke(method, DispatcherPriority.Background, null);
        }

        public static DispatcherOperation RunMethodAsync(Action method)
        {
            DispatcherOperation op = null;

            ThreadStart start = delegate()
            {
                try
                {
                    method.Invoke();
                }
                catch { }
            };

            new Thread(start).Start();

            return op;
        }

        public static void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "// Metro Shell / : Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        public static void ShowInfoMessage(string message)
        {
            MessageBox.Show(message, "// Metro Shell / : Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void ProcessUnhandledException(Exception e)
        {
            try
            {
                Exception ex = e;
                StringBuilder sb = new StringBuilder();
                sb.Append("Timestamp:" + DateTime.Now.ToString() + Environment.NewLine);
                try
                {
                    sb.Append(string.Format("UserName:{0} | .Net:{1} | OS:{2} | Cores:{3}" + Environment.NewLine,
                    Environment.UserName, Environment.Version, Environment.OSVersion + ";" + (Environment.Is64BitOperatingSystem ? "x64" : "x86"), Environment.ProcessorCount));
                }
                catch { }
                sb.Append("Message:" + ex.Message + Environment.NewLine);
                if (ex.StackTrace != null)
                {
                    sb.Append("Stack Trace:" + Environment.NewLine);
                    sb.AppendLine(ex.StackTrace);
                }

                if (ex.InnerException != null && ex.InnerException.Message != null)
                {
                    sb.Append("Inner Exception:" + Environment.NewLine);
                    sb.AppendLine(ex.InnerException.Message);
                }
                sb.Append(Environment.NewLine);
                File.AppendAllText(E.ErrorLog, sb.ToString().Normalize());
            }
            catch { }
        }

        public static void UpdateIESettings()
        {
            try
            {
                RegistryKey feature = Registry.LocalMachine.OpenSubKey("Software").OpenSubKey("Microsoft").OpenSubKey("Internet Explorer").OpenSubKey("MAIN").OpenSubKey("FeatureControl");

                RegistryKey FEATURE_BROWSER_EMULATION = null;
                FEATURE_BROWSER_EMULATION = feature.OpenSubKey("FEATURE_BROWSER_EMULATION", true);
                FEATURE_BROWSER_EMULATION.SetValue("Ftware.Apps.MetroShell.exe", 9000, RegistryValueKind.DWord);
                FEATURE_BROWSER_EMULATION.Close();

                RegistryKey FEATURE_GPU_RENDERING = null;
                FEATURE_GPU_RENDERING = feature.OpenSubKey("FEATURE_GPU_RENDERING", true);
                FEATURE_GPU_RENDERING.SetValue("Ftware.Apps.MetroShell.exe", 00000001, RegistryValueKind.DWord);
                FEATURE_GPU_RENDERING.Close();

                RegistryKey FEATURE_DISABLE_NAVIGATION_SOUNDS = null;
                FEATURE_DISABLE_NAVIGATION_SOUNDS = feature.OpenSubKey("FEATURE_DISABLE_NAVIGATION_SOUNDS", true);
                FEATURE_DISABLE_NAVIGATION_SOUNDS.SetValue("Ftware.Apps.MetroShell.exe", 00000001, RegistryValueKind.DWord);
                FEATURE_DISABLE_NAVIGATION_SOUNDS.Close();

                RegistryKey FEATURE_TABBED_BROWSING = null;
                FEATURE_TABBED_BROWSING = feature.OpenSubKey("FEATURE_TABBED_BROWSING", true);
                FEATURE_TABBED_BROWSING.SetValue("Ftware.Apps.MetroShell.exe", 00000001, RegistryValueKind.DWord);
                FEATURE_TABBED_BROWSING.Close();

                RegistryKey FEATURE_ADDON_MANAGEMENT = null;
                FEATURE_ADDON_MANAGEMENT = feature.OpenSubKey("FEATURE_ADDON_MANAGEMENT", true);
                FEATURE_ADDON_MANAGEMENT.SetValue("Ftware.Apps.MetroShell.exe", 00000001, RegistryValueKind.DWord);
                FEATURE_ADDON_MANAGEMENT.Close();
            }
            catch { }
        }

        public static bool CheckIESettingsEnabled()
        {
            try
            {
                using (var FEATURE_BROWSER_EMULATION = Registry.LocalMachine.OpenSubKey("Software", RegistryKeyPermissionCheck.ReadSubTree).OpenSubKey("Microsoft").OpenSubKey("Internet Explorer").OpenSubKey("MAIN").OpenSubKey("FeatureControl").OpenSubKey("FEATURE_BROWSER_EMULATION"))
                {
                    var v = FEATURE_BROWSER_EMULATION.GetValue("Ftware.Apps.MetroShell.exe");
                    FEATURE_BROWSER_EMULATION.Close();
                    if (v == null)
                        return false;
                    return true;
                }
            }
            catch { }
            return false;
        }

        #endregion Other
    }
}