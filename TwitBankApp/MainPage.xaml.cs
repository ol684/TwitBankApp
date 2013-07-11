using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using TwitBankApp.Resources;
using LinqToTwitter;
using TwitBankApp.Utility;
using System.Text;
using System.IO.IsolatedStorage;
using Windows.Devices.Geolocation;
using System.Threading.Tasks;
using TwitBankApp.ViewModels;
using Yandex.Maps;
using Yandex.Maps.Events;
using System.Windows.Input;

namespace TwitBankApp
{
    public partial class MainPage : PhoneApplicationPage
    {

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(Page_Loaded);
            MapGlobalSettings.Instance.EnableLocationService = true;
            //Yandex.Positioning.GeoPositionStatus status = map.JumpToCurrentLocation();
            DataContext = App.ViewModel;
            //App.Current.
        }

        void Page_Loaded(object sender, RoutedEventArgs e)
        {
            BindData();
        }

        void BindData()
        {
            auth = SharedState.Authorizer;
            if (auth == null || !auth.IsAuthorized)
            {
                NavigationService.Navigate(new Uri("/OAuth.xaml", UriKind.Relative));
                return;
            }

            try
            {
                Yandex.Positioning.GeoPositionStatus status = map.JumpToCurrentLocation();
            }
            catch { }
            //if (status != Yandex.Positioning.GeoPositionStatus.Ready && status != Yandex.Positioning.GeoPositionStatus.Initializing)
            //{
            //    //may be default location    
            //    //await GetCurrentLocation();
            //    //if (geo != null)
            //    //  map.Center = new Yandex.Positioning.GeoCoordinate(geo.Coordinate.Latitude, geo.Coordinate.Longitude);
            //}

            if (!App.ViewModel.IsDataLoaded)
            {
                auth = SharedState.Authorizer;
                App.ViewModel.LoadData(Dispatcher);
            }
        }

        // Load data for the ViewModel Items
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //BindData();
            //Yandex.Positioning.GeoPositionStatus status = map.JumpToCurrentLocation();
        }

        ITwitterAuthorizer auth;



        #region private additional methods

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


        #endregion

        #region map
        /// <summary>Zoom in to closest integer zoom level.
        /// </summary>
        private void Button1Tap(object sender, EventArgs eventArgs)
        {
            map.ZoomIn();
        }

        private void Button4Tap(object sender, EventArgs eventArgs)
        {
            NavigationService.Navigate(new Uri("/Add.xaml", UriKind.Relative));
        }

        private void Button5Tap(object sender, EventArgs eventArgs)
        {
            mapItemsControl.ItemsSource = App.ViewModel.Items;
            //mapItemsControl.SetBinding(
        }

        /// <summary>Zoom out to closest integer zoom level.
        /// </summary>
        private void Button2Tap(object sender, EventArgs eventArgs)
        {
            map.ZoomOut();
        }

        /// <summary>Jump to user's location.
        /// </summary>
        /// <remarks>Map's UseLocation property should be true.</remarks>
        private void Button3Tap(object sender, EventArgs eventArgs)
        {
            Yandex.Positioning.GeoPositionStatus status = map.JumpToCurrentLocation();
            switch (status)
            {
                case Yandex.Positioning.GeoPositionStatus.Disabled:
                    MessageBox.Show("Disabled");
                    break;
                case Yandex.Positioning.GeoPositionStatus.NoData:
                    MessageBox.Show("NoData");
                    break;
                case Yandex.Positioning.GeoPositionStatus.Initializing:
                    MessageBox.Show("Initializing");
                    break;
            }
        }


        /// <summary>Update Map's ContentPadding property that specifies map border offset when positioning controls.
        /// </summary>
        private void ContentPanelLayoutUpdated(object sender, EventArgs e)
        {
            map.ContentPadding = new Thickness(24, 24, SecondColumn.ActualWidth + 24, 24);
        }

        private void MapTap(object sender, GestureEventArgs e)
        {

            Point position = e.GetPosition(map);
            Yandex.Positioning.GeoCoordinate coordinates = map.ViewportPointToCoordinates(new Yandex.Media.Point(position.X, position.Y));
            //ProgressIndicator.Text = coordinates.ToHumanReadableString();

            // example of how to determine viewport point by coordinates
            Yandex.Media.Point point = map.CoordinatesToViewportPoint(coordinates);
            // point is relative to map.Viewport
            int x = Convert.ToInt32(point.X - map.Viewport.X),
                y = Convert.ToInt32(point.Y - map.Viewport.Y);
            //Debug.Assert(x == Convert.ToInt32(position.X) && y == Convert.ToInt32(position.Y));
        }

        /// <summary>Dispose map explicitly when navigating from the page.
        /// </summary>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (e.NavigationMode == NavigationMode.Back)
                map.Dispose();
        }

        /// <summary>Indication of map data load status.
        /// </summary>
        private void MapOperationStatusChanged(object sender, OperationStatusChangedEventArgs e)
        {
            switch (e.OperationStatus)
            {
                case OperationStatus.Idle:
                    StatusTextBlock.Text = "Status: Idle";
                    break;
                case OperationStatus.Normal:
                case OperationStatus.Busy:
                    StatusTextBlock.Text = "Status: Busy";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion
    }
}