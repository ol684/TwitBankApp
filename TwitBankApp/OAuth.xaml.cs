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
using System.IO.IsolatedStorage;
using System.Threading.Tasks;

namespace TwitBankApp
{
    public partial class OAuth : PhoneApplicationPage
    {
        PinAuthorizer pinAuth;
        SingleUserAuthorizer suAuth;
        bool IsConnected;
        public OAuth()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(Page_Loaded);
            OAuthWebBrowser.LoadCompleted += new System.Windows.Navigation.LoadCompletedEventHandler(OAuthWebBrowser_LoadCompleted);
        }

        void OAuthWebBrowser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            EnterPinTextBlock.Visibility = Visibility.Visible;
            PinTextBox.IsEnabled = true;
            AuthenticateButton.IsEnabled = true;
        }

        async Task CheckConnect()
        {
            TwitterContext twitterCtx = null;
            twitterCtx = new TwitterContext(suAuth);

            try
            {


                twitterCtx.Search.Where(search =>
                              search.Type == SearchType.Search &&
                              search.Query == TwittingUtility.GeneralTwit &&
                              search.ResultType == ResultType.Mixed).MaterializedAsyncCallback<Search>(asyncResponse =>
                  Dispatcher.BeginInvoke(() =>
                  {
                      IsConnected = asyncResponse.Status == TwitterErrorStatus.Success;

                      if (!IsConnected)
                      {
                          AutorizeByPin();
                      }
                      else
                      {
                          SharedState.Authorizer = suAuth;
                          NavigationService.GoBack();
                      }

                  }));
            }
            catch (Exception ee)
            {
                IsConnected = false;
            }
        }

        void AutorizeByPin()
        {
            SharedState.Authorizer = null;
            //OAuthAuthorizer oa = new OAuthAuthorizer();
            this.pinAuth = new PinAuthorizer
            {
                Credentials = new InMemoryCredentials
                {
                    ConsumerKey = "zDx8TCYL9whO3lqCAWIMw",
                    ConsumerSecret = "NHfW1uavQKzyJivsVEEHoABofMhjWyGe9t0R9e5nE",
                    //AccessToken = "1552383054-YoXmKfmXuCxWs8HXxPj9jh8QwOz3KCRqbcU2Qmg",

                },
                UseCompression = true,
                //GetPin = () =>
                //    Dispatcher.BeginInvoke(() => )),
                GoToTwitterAuthorization = pageLink =>
                    Dispatcher.BeginInvoke(() => OAuthWebBrowser.Navigate(new Uri(pageLink, UriKind.Absolute))),
            };

            ContentPanel.Visibility = Visibility.Visible;

            this.pinAuth.BeginAuthorize(resp =>
                Dispatcher.BeginInvoke(() =>
                {
                    switch (resp.Status)
                    {
                        case TwitterErrorStatus.Success:
                            break;
                        case TwitterErrorStatus.TwitterApiError:
                        case TwitterErrorStatus.RequestProcessingException:
                            MessageBox.Show(
                                resp.Exception.ToString(),
                                resp.Message,
                                MessageBoxButton.OK);
                            break;
                    }
                }));
        }

        async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ContentPanel.Visibility = Visibility.Collapsed;
            try
            {
                if (IsolatedStorageSettings.ApplicationSettings["AccessToken"] != null)
                {
                    //this.pinAuth.Authorize();
                    suAuth = new SingleUserAuthorizer
                    {
                        Credentials = new InMemoryCredentials
                        {

                            ConsumerKey = "zDx8TCYL9whO3lqCAWIMw",
                            ConsumerSecret = "NHfW1uavQKzyJivsVEEHoABofMhjWyGe9t0R9e5nE",
                            OAuthToken = IsolatedStorageSettings.ApplicationSettings["OAuthToken"].ToString(),
                            AccessToken = IsolatedStorageSettings.ApplicationSettings["AccessToken"].ToString()
                        }
                    };
                    suAuth.Authorize();
                    await CheckConnect();
                }
            }
            catch (Exception ee)
            {
                ContentPanel.Visibility = Visibility.Visible;
                IsConnected = false;
                AutorizeByPin();
            }
        }

        private void AuthenticateButton_Click(object sender, RoutedEventArgs e)
        {
            pinAuth.CompleteAuthorize(
                PinTextBox.Text,

                completeResp => Dispatcher.BeginInvoke(() =>
                {
                    switch (completeResp.Status)
                    {
                        case TwitterErrorStatus.Success:
                            SharedState.Authorizer = pinAuth;
                            IsolatedStorageSettings.ApplicationSettings["AccessToken"] = pinAuth.Credentials.AccessToken;
                            IsolatedStorageSettings.ApplicationSettings["OAuthToken"] = pinAuth.Credentials.OAuthToken;
                            IsolatedStorageSettings.ApplicationSettings.Save();
                            NavigationService.GoBack();
                            break;
                        case TwitterErrorStatus.TwitterApiError:
                        case TwitterErrorStatus.RequestProcessingException:
                            MessageBox.Show(
                                completeResp.Exception.ToString(),
                                completeResp.Message,
                                MessageBoxButton.OK);
                            break;
                    }
                }));
        }
    }
}