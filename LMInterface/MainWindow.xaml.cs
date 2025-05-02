using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace LMInterface {

    public sealed partial class MainWindow : Window {

        public static MainWindow Instance;

        public LMStudioInterface LMStudio = new();

        public MainWindow() {
            this.InitializeComponent();
            Instance = this;

            //init menu
            NavigationView.SelectedItem = HomeMenuEntry;

            //start tick function
            InitTick();
        }

        private void InitTick() {
            var dispatcherTimer = new DispatcherTimer {
                Interval = TimeSpan.FromSeconds(5)
            };
            dispatcherTimer.Tick += Tick;
            dispatcherTimer.Start();
        }

        private void Tick(object? sender, object e) {
            //
        }

        private void NavigationView_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args) {
            
            if(args.IsSettingsSelected) ContentFrame.Navigate(typeof(SettingsPage));

            var selectedItem = args.SelectedItem as NavigationViewItem;
            if (selectedItem == null) return;

            switch (selectedItem.Tag as string) {
                case "home":
                    ContentFrame.Navigate(typeof(HomePage));
                    break;
                case "conversation":
                    ContentFrame.Navigate(typeof(ConversationPage));
                    break;
            }
        }
    }
}