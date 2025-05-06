using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace LMInterface {

    public sealed partial class MainWindow : Window {

        public static MainWindow Instance;

        public MainWindow() {
            this.InitializeComponent();
            Instance = this;

            //open conversation page at start
            NavigationView.SelectedItem = ConversationPage;
        }

        public void SwitchFrame(Type page) {
            ContentFrame.Navigate(page);
        }

        private void NavigationView_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args) {
            
            if(args.IsSettingsSelected) ContentFrame.Navigate(typeof(SettingsPage));

            var selectedItem = args.SelectedItem as NavigationViewItem;
            if (selectedItem == null) return;

            switch (selectedItem.Tag as string) {
                case "conversation":
                    ContentFrame.Navigate(typeof(ConversationPage));
                    break;
            }
        }
    }
}