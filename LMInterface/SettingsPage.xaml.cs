using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LMInterface {
    public sealed partial class SettingsPage : Page {

        //stores all available models
        private ObservableCollection<Model> _availableModels = new();

        public static string ApiUrl = "";
        public static string SelectedModel = "";


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
            ApiUrl = ApiUrlTextBox.Text;

            //fetch all available models
            RefreshModelsCollection();
        }

        private void ModelSelection_Changed(object sender, RoutedEventArgs routedEventArgs) {
            var toggleButton = (RadioButton)sender;

            if (toggleButton.IsChecked!.Value && toggleButton.DataContext != null) {
                var data = (Model)toggleButton.DataContext;
                SelectedModel = data.Id;
            } else {
                SelectedModel = "";
            }
        }

        private void ApiUrl_Changed(object sender, TextChangedEventArgs e) => ApiUrl = ApiUrlTextBox.Text;

        private void RefreshButton_Clicked(object sender, RoutedEventArgs e) => RefreshModelsCollection();
    }
}