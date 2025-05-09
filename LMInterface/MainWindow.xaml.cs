using System;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace LMInterface {

    public sealed partial class MainWindow : Window {

        public static MainWindow Instance;

        public Page? CurrentPage => NavigationView.Content as Page;

        public MainWindow() {
            this.InitializeComponent();
            Instance = this;
        }

        /// <summary>
        /// Navigates to the specified page by tag.
        /// </summary>
        public void ShowPageByTag(string tag) {

            //get the requested page (including settings page)
            var page = NavigationView.MenuItems
                .Append(NavigationView.SettingsItem)
                .OfType<NavigationViewItem>()
                .FirstOrDefault(o => (string)o.Tag == tag);

            //if page could not be found
            if (page == null) return;

            //change the selected item if it's not already selected
            if (NavigationView.SelectedItem == null || (string)((NavigationViewItem)NavigationView.SelectedItem).Tag != tag) {
                NavigationView.SelectedItem = page;
            }

            //navigate to the page
            ContentFrame.Navigate(Type.GetType($"{tag}"));
        }

        /// <summary>
        /// Navigates to the page when a new navigation item is selected.
        /// </summary>
        private void NavigationView_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args) {
            ShowPageByTag((string)((NavigationViewItem)args.SelectedItem).Tag);
        }

        /// <summary>
        /// Sets the correct tag for the settings navigation item and shows first page when loaded.
        /// </summary>
        private void NavigationView_OnLoaded(object sender, RoutedEventArgs e) {
            ((NavigationViewItem)NavigationView.SettingsItem).Tag = "LMInterface.SettingsPage";
            
            //open conversation page at start
            NavigationView.SelectedItem = NavigationView.MenuItems[0];
        }

        /// <summary>
        /// Loads all services when the window gets opened.
        /// </summary>
        private async void MainWindow_Activated(object sender, WindowActivatedEventArgs args) => await ServiceProvider.LoadServices().ConfigureAwait(false);

        /// <summary>
        /// Save all stuff before closing application.
        /// </summary>
        private async void MainWindow_OnClosed(object sender, WindowEventArgs args) => await ServiceProvider.SaveServices().ConfigureAwait(false);
    }
}