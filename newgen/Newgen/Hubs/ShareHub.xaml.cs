using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Newgen.Controls;
using Social.Base;

namespace Newgen.Windows
{
    /// <summary>
    /// Interaction logic for ShareHub.xaml
    /// </summary>
    public partial class ShareHub : Window
    {
        private SocialProvider socialProvider;

        public ShareHub()
        {
            InitializeComponent();
        }

        private void WindowKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
        }

        private void WindowSourceInitialized(object sender, EventArgs e)
        {
            this.Left = 0;
            this.Top = 0;
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;

            socialProvider = new SocialProvider();
            socialProvider.SignedIn += SocialProviderSignedIn;
            ThreadStart threadStarter = delegate
                                            {
                                                socialProvider.SignIn();
                                            };
            var thread = new Thread(threadStarter);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            iFr.Helper.Animate(this, OpacityProperty, 500, 0, 1);
        }

        private void SocialProviderSignedIn(object sender, EventArgs e)
        {
            socialProvider.SignedIn -= SocialProviderSignedIn;
            ThreadStart threadStarter = delegate
            {
                this.Dispatcher.BeginInvoke((Action)delegate
                {
                    var friends = socialProvider.GetFriends();
                    foreach (var friend in friends)
                    {
                        var item = new ShareItem();
                        item.Friend = friend;
                        var loadedFriend = App.WidgetManager.Widgets.Find(x => x.Path == friend.Id);
                        if (loadedFriend != null)
                        {
                            item.IsChecked = true;
                        }
                        item.MouseLeftButtonUp += ItemMouseLeftButtonUp;
                        SharePanel.Children.Add(item);
                    }
                });
            };
            var thread = new Thread(threadStarter);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private void ItemMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var item = (ShareItem)sender;
            if (App.WidgetManager.IsWidgetLoaded(item.Friend.Name))
                return;
            var widget = App.WidgetManager.CreateFriendWidget(item.Friend.Id, item.Friend.Name);
            App.WidgetManager.LoadWidget(widget);
        }

        private void BackButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            iFr.Helper.Animate(this, OpacityProperty, 250, 0);

            foreach (ShareItem item in SharePanel.Children)
            {
                item.MouseLeftButtonUp -= ItemMouseLeftButtonUp;
            }
        }
    }
}