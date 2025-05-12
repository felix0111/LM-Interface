using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Controls.Primitives;
using LMInterface.Services;
using WinRT;

namespace LMInterface
{
    public sealed partial class ConversationPage : Page {

        public ObservableCollection<Conversation> Conversations => ServiceProvider.ConversationService.Conversations;

        //this should normally never be null
        private TabViewItem? _currentTab;

        public ConversationPage() {
            this.InitializeComponent();
        }

        /// <summary>
        /// Adds message to the end of the list view and may scroll down.
        /// </summary>
        public void AddMessageWithScroll(Conversation conv, Message message) {

            //determine if the listview gets automatically scrolled down
            //must be checked before adding message
            bool scroll = !_currentTab!.Content.As<ListView>().IsScrolledUp(100);

            conv.AddMessage(message);

            if (scroll) _currentTab!.Content.As<ListView>().ScrollToLastItem();
        }

        /// <summary>
        /// Selects the last opened conversation. Creates new if nothing selected.
        /// </summary>
        private void ConversationTabs_OnLoaded(object sender, RoutedEventArgs e) {
            Conversation conv = ServiceProvider.ConversationService.GetConversation() ??
                                ServiceProvider.ConversationService.Conversations.FirstOrDefault() ??
                                ServiceProvider.ConversationService.NewConversation("");

            ConversationTabs.SelectedItem = conv;
        }

        /// <summary>
        /// Updates the current selected conversation.
        /// </summary>
        private void ConversationTabs_OnSelectionChanged(object o, SelectionChangedEventArgs selectionChangedEventArgs) {

            //get the id and TabViewItem of the newly selected tab/conversation
            int newId;
            if (selectionChangedEventArgs.AddedItems.Count == 0) {
                newId = -1;
                _currentTab = null;
            } else {
                newId = selectionChangedEventArgs.AddedItems[0].As<Conversation>().ConversationId;

                //must update layout because the TabViewItem may not be created at this point
                ConversationTabs.UpdateLayout();
                _currentTab = (TabViewItem)ConversationTabs.ContainerFromItem(selectionChangedEventArgs.AddedItems[0].As<Conversation>());
                _currentTab.Content.As<ListView>().ScrollToLastItem();
            }
            
            ServiceProvider.ConversationService.CurrentConversation = newId;
        }

        /// <summary>
        /// Deletes the conversation and may create a new one.
        /// </summary>
        private void ConversationTabs_OnTabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args) {
            ServiceProvider.ConversationService.RemoveConversation((Conversation)args.Item);

            //create new conversation if no conversation left over
            if (sender.TabItems.Count == 0) {
                ConversationTabs.SelectedItem = ServiceProvider.ConversationService.NewConversation("");
            }
        }

        /// <summary>
        /// Creates a new conversation and selects it.
        /// </summary>
        private void ConversationTabs_OnAddTabButtonClick(TabView sender, object args) {
            string model = _currentTab!.DataContext.As<Conversation>().ModelId;
            ConversationTabs.SelectedItem = ServiceProvider.ConversationService.NewConversation(model);
        }

        /// <summary>
        /// Processes user message.
        /// </summary>
        private async void SendMessage_InputBox(object sender, KeyRoutedEventArgs keyRoutedEventArgs) {
            //if not enter OR
            //if shift pressed, then dont send
            if (keyRoutedEventArgs.Key != VirtualKey.Enter || InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down)) {
                keyRoutedEventArgs.Handled = false;
                return;
            }

            // Block the Enter key from creating a new line
            keyRoutedEventArgs.Handled = true;

            //get current conversation
            //must be stored in this function in case the user switches tabs/conversations before this function finishes
            var conv = _currentTab!.DataContext.As<Conversation>();

            //check if a model is selected
            if (conv.ModelId == "") {
                SettingsPopup.ShowAsync();
                ModelSelector.Focus(FocusState.Keyboard);
                return;
            }

            //pre-format raw text and clear textbox
            var textBox = (TextBox)sender;
            var rawText = textBox.Text.Replace('\r', '\n').Trim();
            textBox.Text = "";

            //if empty then return
            if (rawText == "") return;

            //add message to conversation
            AddMessageWithScroll(conv, new Message() { Role = "user", Content = rawText });

            //disable textbox
            InputBox.IsEnabled = false;

            //show all new messages
            await foreach (var newMsg in ServiceProvider.ConversationService.WaitForResponse(ThinkButton.IsChecked!.Value, ToolButton.IsChecked!.Value)) {
                AddMessageWithScroll(conv, newMsg);
            }

            //enable textbox and focus again
            InputBox.IsEnabled = true;
            InputBox.Focus(FocusState.Pointer);
        }

        /// <summary>
        /// Opens the settings dialog for the conversation.
        /// </summary>
        private void SettingsButton_Click(object sender, RoutedEventArgs e) => SettingsPopup.ShowAsync();

        /// <summary>
        /// Synchronizes SettingsPopup to the current settings.
        /// </summary>
        private void SettingsPopup_OnOpened(ContentDialog sender, ContentDialogOpenedEventArgs args) {
            var conv = _currentTab!.DataContext.As<Conversation>();

            ModelSelector.SelectedItem = conv.ModelId;

            Message? systemMessage = conv.GetSystemMessage();
            SystemPromptTextBox.Text = systemMessage == null ? "" : systemMessage.Content;

            MaxTokens.Value = conv.MaxTokens;

            NoReasoningTokenTextBox.Text = conv.NoReasoningToken;
        }

        /// <summary>
        /// Applies settings when dialog closes.
        /// </summary>
        private void SubmitButton_Click(ContentDialog contentDialog, ContentDialogButtonClickEventArgs args) {
            var conv = _currentTab!.DataContext.As<Conversation>();

            conv.ModelId = (string)ModelSelector.SelectedItem;
            conv.MaxTokens = (long)MaxTokens.Value;
            conv.NoReasoningToken = NoReasoningTokenTextBox.Text;
            conv.SetSystemMessage(SystemPromptTextBox.Text, out _);
        }

        /// <summary>
        /// Changes some expander properties that are not directly accessible in xaml.
        /// </summary>
        private void MessageExpander_Loaded(object sender, RoutedEventArgs e) {
            var expander = (Expander)sender;
            expander.ApplyTemplate();

            var expanderGrid = VisualTreeHelper.GetChild(expander, 0) as Grid;
            var toggle = VisualTreeHelper.GetChild(expanderGrid, 0) as ToggleButton;

            //change header color
            UIExtensions.StringToColor("#2a2a2a", out Windows.UI.Color c);
            toggle!.Background = new SolidColorBrush(c);
            toggle.BorderThickness = new Thickness(0);

            //disable border
            var contentBorderClip = VisualTreeHelper.GetChild(expanderGrid, 1) as Border;
            var contentBorder = VisualTreeHelper.GetChild(contentBorderClip, 0) as Border;
            contentBorder!.BorderThickness = new Thickness(0);

            //scroll when expander enlarges
            expander.SizeChanged += (o, args) => {
                if(expander.IsExpanded) _currentTab!.Content.As<ListView>().SmoothScrollIntoViewWithItemAsync(expander.DataContext);
            };
        }

        private async void ModelSelector_OnLoaded(object sender, RoutedEventArgs e) {
            var conv = _currentTab!.DataContext.As<Conversation>();

            if (ModelSelector.Items.Count == 0) await FetchModels();
            if (ModelSelector.Items.Count != 0) ModelSelector.SelectedItem = ModelSelector.Items.Cast<string>().FirstOrDefault(o => o == conv.ModelId, (string)ModelSelector.Items[0]);
        }

        private async void FetchModelsButton_OnClick(object sender, RoutedEventArgs e) {
            await FetchModels();
        }

        private async Task FetchModels() {
            FetchModelsButton.Visibility = Visibility.Collapsed;
            FetchModelsProgress.Visibility = Visibility.Visible;

            ModelsResponse? mr = await LMStudioInterface.GetAvailableModels();

            //if no models found, then empty list as source
            ModelSelector.ItemsSource = mr != null ? mr.Data.Select(o => o.Id).ToList() : new List<string>();

            FetchModelsProgress.Visibility = Visibility.Collapsed;
            FetchModelsButton.Visibility = Visibility.Visible;
        }
    }

    /// <summary>
    /// Holds all properties and functions used for UI.
    /// </summary>
    public partial class Message {
        public HorizontalAlignment MessageAlignment {
            get {
                if (Role == "system") return HorizontalAlignment.Center;
                return Role == "assistant" ? HorizontalAlignment.Left : HorizontalAlignment.Right;
            }
        }

        public Thickness MessageMargin {
            get {
                if (Role == "system") return new Thickness(60, 15, 60, 15);
                return Role == "assistant" ? new Thickness(0, 15, 50, 15) : new Thickness(50, 20, 0, 20);
            }
        }

        public Visibility ThoughtsVisible => Thoughts.Trim('\n') != "" ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ToolCallVisible => ToolCall.Trim('\n') != "" ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ToolCallResultVisible => ToolCallResult.Trim('\n') != "" ? Visibility.Visible : Visibility.Collapsed;
        public Visibility MainContentVisible => MainContent.Trim('\n') != "" ? Visibility.Visible : Visibility.Collapsed;

        public string ToolCall => ToolCalls != null ? ToolCalls!.Select(o => $"{o.ToolCallArguments.Name} : {o.ToolCallArguments.Arguments}").Aggregate((s, s1) => $"{s}  \n{s1}") : "";
        public string ToolCallResult => ToolCallId != null ? Content! : "";

        public string Thoughts => Content == null ? "" : Content.GetTag("think");
        public string MainContent => Content == null || ToolCallId != null ? "" : Content.RemoveTag("think", out _).RemoveTag("tool", out _);

        [SetsRequiredMembers]
        public Message() {
            Role = "";
        }

        public Message WithoutThinkSection() {
            Message clone = Clone();
            clone.Content = clone.Content.RemoveTag("think", out _);

            return clone;
        }
    }
}
