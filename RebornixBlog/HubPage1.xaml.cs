﻿using RebornixBlog.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using HtmlAgilityPack;

// The Hub Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=321224

namespace RebornixBlog
{
    public static class Concurrency
    {
        public static HubSection GotoSection { get; set; }
    }

    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class HubPage1 : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public HubPage1()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        }


        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // TODO: Assign a collection of bindable groups to this.DefaultViewModel["Groups"]
            FeedDataSource feedDataSource = (FeedDataSource)App.Current.Resources["feedDataSource"];

            if (feedDataSource != null)
            {
                this.DefaultViewModel["LatestItems"] = feedDataSource.Feeds.First().Items.GetRange(0, 6);
                this.DefaultViewModel["StarredItems"] = feedDataSource.Feeds.First().Items.GetRange(0, 1);

                GetPreviewImage(feedDataSource.Feeds.First().Items.GetRange(0, 6));
            }
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (Concurrency.GotoSection != null)
                this.currentHub.ScrollToSection(Concurrency.GotoSection);
            Concurrency.GotoSection = null;

            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Concurrency.GotoSection = this.currentHub.SectionsInView.Last();
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void BlogItem_Click(object sender, ItemClickEventArgs e)
        {
            //Selector list = sender as Selector;
            FeedItem selectedItem = e.ClickedItem as FeedItem;

            if (selectedItem != null && this.Frame != null)
            {
                string itemTitle = selectedItem.Title;
                this.Frame.Navigate(typeof(DetailPage), itemTitle);
            }
        }

        private void SectionHeader_Click(object sender, HubSectionHeaderClickEventArgs e)
        {
            switch (e.Section.Header.ToString())
            {
                case "Latest":
                    this.Frame.Navigate(typeof(LatestBlogs));
                    break;
                case "Starred":
                    break;
                default:
                    break;
            }
        }

        private void LatestItem_Click(object sender, ItemClickEventArgs e)
        {
            FeedItem selectedItem = e.ClickedItem as FeedItem;

            if (selectedItem != null && this.Frame != null)
            {
                string itemTitle = selectedItem.Title;
                this.Frame.Navigate(typeof(DetailPage), itemTitle);
            }
        }

        private async void GetPreviewImage(List<FeedItem> feedItems)
        {
            
            foreach (var item in feedItems)
            {
                if (item.Image == null)
                {
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(item.Content);
                    var imageNode = doc.DocumentNode.Descendants("img");

                    if (imageNode.Count() != 0)
                    {
                        var uri = imageNode.First().GetAttributeValue("src", "https://avatars3.githubusercontent.com/u/876920?s=460");
                        item.Image = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(uri));
                    }
                    else
                    {
                        item.Image = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri("ms-appx:///Assets/200.png"));
                    }
                }
            }

        }
    }
}
