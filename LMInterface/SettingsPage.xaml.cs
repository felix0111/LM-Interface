using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Media;
using Colors = Microsoft.UI.Colors;
using LMInterface.Services;

namespace LMInterface {

    public sealed partial class SettingsPage : Page {

        //stores all available models
        private ObservableCollection<Model> _availableModels = new();

        public SettingsPage() {
            this.InitializeComponent();
        }

        //gets all available models
        private void RefreshModelsCollection() {
            RefreshButton.Visibility = Visibility.Collapsed;
            RefreshProgress.Visibility = Visibility.Visible;
            _availableModels.Clear();

            var dis = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            _ = LMStudioInterface.GetAvailableModels(result => {
                dis.TryEnqueue(() => {
                    foreach (var model in result.Data) {
                        _availableModels.Add(model);
                    }

                    RefreshProgress.Visibility = Visibility.Collapsed;
                    RefreshButton.Visibility = Visibility.Visible;
                });
            });
        }

        //reflect ModelSelection to SettingsService
        private void ModelSelection_Changed(object sender, RoutedEventArgs routedEventArgs) {
            var toggleButton = (RadioButton)sender;

            if (toggleButton.IsChecked!.Value && toggleButton.DataContext != null) {
                var data = (Model)toggleButton.DataContext;
                ServiceProvider.SettingsService.SelectedModel = data.Id;
            } else {
                ServiceProvider.SettingsService.SelectedModel = "";
            }
        }

        //reflect TextBox to SettingsService
        private void ApiUrl_Changed(object sender, TextChangedEventArgs e) {
            ApiUrlTextBox.BorderBrush = HttpHelper.ValidateUrl(ApiUrlTextBox.Text) ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
            ServiceProvider.SettingsService.ApiUrl = ApiUrlTextBox.Text;
        }

        private void RefreshButton_Clicked(object sender, RoutedEventArgs e) => RefreshModelsCollection();

        /// <summary>
        /// Initializes controls to reflect the parameters in SettingsService.
        /// </summary>
        private void SettingsPage_OnLoaded(object sender, RoutedEventArgs e) {
            ApiUrlTextBox.Text = ServiceProvider.SettingsService.ApiUrl;
            RefreshModelsCollection();
        }

        /// <summary>
        /// Checks if the Model which corresponds to this RadioButton is selected und thus needs to be checked.
        /// </summary>
        private void RadioButton_Loaded(object sender, RoutedEventArgs e) {
            var radiobutton = (RadioButton)sender;
            var model = (Model)radiobutton.DataContext;

            if(ServiceProvider.SettingsService.SelectedModel == model.Id) radiobutton.IsChecked = true;
        }
    }
}