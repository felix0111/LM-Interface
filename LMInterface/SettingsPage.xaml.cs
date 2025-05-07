using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;

namespace LMInterface {

    public sealed partial class SettingsPage : Page {

        //stores all available models
        private ObservableCollection<Model> _availableModels = new();

        public SettingsPage() {
            this.InitializeComponent();
        }

        private void RefreshModelsCollection() {
            RefreshButton.Visibility = Visibility.Collapsed;
            RefreshProgress.Visibility = Visibility.Visible;

            var dis = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            _ = LMStudioInterface.GetAvailableModels(result => {
                dis.TryEnqueue(() => {
                    _availableModels.Clear();
                    foreach (var model in result.Data) {
                        _availableModels.Add(model);
                    }

                    RefreshProgress.Visibility = Visibility.Collapsed;
                    RefreshButton.Visibility = Visibility.Visible;
                });
            });
        }

        private void ModelsList_Loaded(object sender, RoutedEventArgs e) {
            //api url might not be set at this point
            ServiceProvider.Settings.ApiUrl = ApiUrlTextBox.Text;

            //fetch all available models
            RefreshModelsCollection();
        }

        private void ModelSelection_Changed(object sender, RoutedEventArgs routedEventArgs) {
            var toggleButton = (RadioButton)sender;

            if (toggleButton.IsChecked!.Value && toggleButton.DataContext != null) {
                var data = (Model)toggleButton.DataContext;
                ServiceProvider.Settings.SelectedModel = data.Id;
            } else {
                ServiceProvider.Settings.SelectedModel = "";
            }
        }

        private void ApiUrl_Changed(object sender, TextChangedEventArgs e) => ServiceProvider.Settings.ApiUrl = ApiUrlTextBox.Text;

        private void RefreshButton_Clicked(object sender, RoutedEventArgs e) => RefreshModelsCollection();
    }
}