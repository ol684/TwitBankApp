using LinqToTwitter;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using TwitBankApp.Resources;
using System.Linq;
using TwitBankApp.Utility;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using System.Reactive.Linq;

namespace TwitBankApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            this.Items = new ObservableCollection<PushpinDummyViewModel>();
        }

        /// <summary>
        /// A collection for ItemViewModel objects.
        /// </summary>
        public ObservableCollection<PushpinDummyViewModel> Items { get; private set; }

        //private string _sampleProperty = "Sample Runtime Property Value";
        ///// <summary>
        ///// Sample ViewModel property; this property is used in the view to display its value using a Binding
        ///// </summary>
        ///// <returns></returns>
        //public string SampleProperty
        //{
        //    get
        //    {

        //        return _sampleProperty;
        //    }
        //    set
        //    {
        //        if (value != _sampleProperty)
        //        {
        //            _sampleProperty = value;
        //            NotifyPropertyChanged("SampleProperty");
        //        }
        //    }
        //}

        /// <summary>
        /// Sample property that returns a localized string
        /// </summary>
        public string LocalizedSampleProperty
        {
            get
            {
                return AppResources.SampleProperty;
            }
        }

        public bool IsDataLoaded
        {
            get;
            set;
        }
        public void LoadData()
        {

        }

        //public void LoadDataAsync(Dispatcher Dispatcher)
        //{
        //    Observable.ToAsync<Dispatcher>(LoadData)(Dispatcher).Subscribe();
        //}

        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public void LoadData(Dispatcher Dispatcher)
        {
            ITwitterAuthorizer auth = SharedState.Authorizer;
            TwitterContext twitterCtx = null;
            if (auth != null)
                twitterCtx = new TwitterContext(auth);

            if (twitterCtx != null)
            {
                try
                {
                    this.Items.Clear();
                    twitterCtx.Search.Where(search =>
                                   search.Type == SearchType.Search &&
                                   search.Query == TwittingUtility.GeneralTwit &&
                                   search.ResultType == ResultType.Mixed).MaterializedAsyncCallback<Search>(asyncResponse =>
                       Dispatcher.BeginInvoke(() =>
                       {
                           if (asyncResponse.Status != TwitterErrorStatus.Success)
                           {
                               return;
                           }

                           foreach (Status item in asyncResponse.State.SingleOrDefault().Statuses)
                           {
                             
                               PushpinDummyViewModel exist = this.Items
                                   .Where(p => p.Position.Latitude == item.Coordinates.Latitude
                                   && p.Position.Longitude == item.Coordinates.Longitude).FirstOrDefault();
                               if (exist != null)
                               {
                                   exist.Text += "\r\n " + TwittingUtility.GetBankInfo(item.Text);
                               }
                               else
                               {
                                   this.Items.Add(new PushpinDummyViewModel()
                                   {
                                       Text = TwittingUtility.GetBankInfo(item.Text),
                                       ContentVisibility = Visibility.Collapsed,
                                       State = Yandex.Maps.PushPinState.Expanded,
                                       Position = new Yandex.Positioning.GeoCoordinate(item.Coordinates.Latitude, item.Coordinates.Longitude)
                                   });
                               }
                           }

                           //SocialListBox.ItemsSource = social.IDs;
                       }));


                    this.IsDataLoaded = true;
                }
                catch (Exception ee)
                {

                }
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}