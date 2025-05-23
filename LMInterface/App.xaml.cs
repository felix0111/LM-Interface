﻿using LMInterface.Services;
using Microsoft.UI.Xaml;

namespace LMInterface {
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application {

        private Window _mainWindow;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App() {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args) {
            await ServiceProvider.LoadServices();
            await PythonHelper.RunServer();

            _mainWindow = new MainWindow();
            _mainWindow.Activate();
        }
    }
}