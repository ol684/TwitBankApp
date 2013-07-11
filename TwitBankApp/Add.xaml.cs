using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using LinqToTwitter;
using TwitBankApp.Utility;
using Windows.Devices.Geolocation;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using TwitBankApp.ViewModels;

namespace TwitBankApp
{
    public partial class Add : PhoneApplicationPage
    {
        public Add()
        {
            InitializeComponent();
        }

        ITwitterAuthorizer auth;

        private void CancelButton_Click(object sender, EventArgs e) {
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }

        private async void TweetButton_Click(object sender, EventArgs e)
        {
            
            if (string.IsNullOrWhiteSpace(TwitText.Text))
            {
                MessageBox.Show("Сообщение не должно быть пустым");
                return;
            }

            auth = SharedState.Authorizer;

            if (auth == null || !auth.IsAuthorized)
            {
                NavigationService.Navigate(new Uri("/OAuth.xaml", UriKind.Relative));
            }
            else
            {
                var twitterCtx = new TwitterContext(auth);

                string Twit = TwittingUtility.GenerateMessage(
                    TwitText.Text,
                    TwitBank.Text,
                    TwitInline.Text,
                    TwitTimeHour.Text,
                    TwitTimeMins.Text,
                    TwitOpen.Text,
                    TwitClosed.Text,
                    TwitAngry.IsChecked ?? false,
                    TwitChairs.IsChecked ?? false,
                    TwitUssr.IsChecked ?? false);

                await GetCurrentLocation();

                bool IsSend = false;
                if (geo != null)
                {
                    IsSend = TwittingUtility.SendMessage(twitterCtx, Twit, geo, ResponseAction);
                    msg = "Сообщение отправлено";
                }
                else
                {
                    IsSend = TwittingUtility.SendMessage(twitterCtx, Twit, ResponseAction);
                    msg = "Не удалось определить ваще местоположение. Сообщение отправлено.";
                }
                if (IsSend)
                {
                    //foreach (PushpinDummyViewModel push in App.ViewModel.Items) {
                    //    push.Text = "new text";
                    //}
                    
                    App.ViewModel.LoadData(Dispatcher);
                    TwitInline.Text = TwitTimeHour.Text = TwitTimeMins.Text = TwitOpen.Text = TwitClosed.Text = TwitBank.Text = TwitText.Text = "";
                    TwitUssr.IsChecked = TwitChairs.IsChecked = TwitAngry.IsChecked = false;
                }
            }
        }

        private Geoposition geo;

        private async Task GetCurrentLocation()
        {
            IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = true;

            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 50;
            try
            {
                Geoposition geoposition = await geolocator.GetGeopositionAsync(
                    maximumAge: TimeSpan.FromMinutes(5),
                    timeout: TimeSpan.FromSeconds(10)
                    );
                geo = geoposition;
            }
            catch (Exception ex)
            {
                if ((uint)ex.HResult == 0x80004004)
                {
                    MessageBox.Show("location  is disabled in phone settings.");
                }
            }
        }

        string msg;

        private void ResponseAction(TwitterAsyncResponse<Status> updateResp)
        {
            Dispatcher.BeginInvoke(() =>
            {
                switch (updateResp.Status)
                {
                    case TwitterErrorStatus.Success:
                        //Status tweet = updateResp.State;
                        //User user = tweet.User;
                        //UserIdentifier id = user.Identifier;
                        MessageBox.Show(msg, "", MessageBoxButton.OK);
                        break;
                    case TwitterErrorStatus.TwitterApiError:
                    case TwitterErrorStatus.RequestProcessingException:
                        //MessageBox.Show(
                        //    updateResp.Exception.Message.ToString(),
                        //    updateResp.Message,
                        //    MessageBoxButton.OK);
                        MessageBox.Show("Ошибка при отправке сообщения", "", MessageBoxButton.OK);
                        break;
                }
            });
        }
    }
}