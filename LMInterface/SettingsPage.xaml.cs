using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Colors = Microsoft.UI.Colors;
using LMInterface.Services;

namespace LMInterface {

    public sealed partial class SettingsPage : Page {

        public SettingsPage() {
            this.InitializeComponent();
        }

        //reflect TextBox to SettingsService
        private void ApiUrl_Changed(object sender, TextChangedEventArgs e) {
            ApiUrlTextBox.BorderBrush = HttpHelper.ValidateUrl(ApiUrlTextBox.Text) ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
            ServiceProvider.SettingsService.ApiUrl = ApiUrlTextBox.Text;
        }

        /// <summary>
        /// Initializes controls to reflect the parameters in SettingsService.
        /// </summary>
        private void SettingsPage_OnLoaded(object sender, RoutedEventArgs e) {
            ApiUrlTextBox.Text = ServiceProvider.SettingsService.ApiUrl;
        }
    }
}