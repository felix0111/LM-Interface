using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.System;
using Windows.UI.Core;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace LMInterface
{
    public sealed partial class ConversationPage : Page {

        public ObservableCollection<Message> Conversation = new() {new Message() {Role = "system", Content = ""}};

        public ConversationPage() {
            this.InitializeComponent();
        }

        //should probably be executed in the UI thread
        public void AddMessageToConversation(Message message) {

            //determine if the listview gets automatically scrolled down
            Border? border = VisualTreeHelper.GetChild(MessagesList, 0) as Border;
            ScrollViewer? scrollViewer = VisualTreeHelper.GetChild(border, 0) as ScrollViewer;
            bool scroll = scrollViewer?.VerticalOffset > scrollViewer?.ScrollableHeight - 100;

            Conversation.Add(message);

            if(scroll) ScrollToLastMessage();
        }

        /// <summary>
        /// Executed when user presses Enter on TextBlock.
        /// </summary>
        private void SendMessage_InputBox(object sender, KeyRoutedEventArgs keyRoutedEventArgs) {
            //if not enter
            if (keyRoutedEventArgs.Key != VirtualKey.Enter) return;

            //if shift pressed, then dont send
            if (InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down)) {
                keyRoutedEventArgs.Handled = false;
                return;
            }

            // Block the Enter key from creating a new line
            keyRoutedEventArgs.Handled = true;

            //pre-format raw text and clear textbox
            var textBox = (TextBox)sender;
            var rawText = textBox.Text.Replace('\r', '\n').Trim();
            textBox.Text = "";

            //if empty then return
            if (rawText == "") return;

            //show message on page
            AddMessageToConversation(new Message() { Role = "user", Content = rawText });

            //send message to language model (async)
            var dis = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            _ = MainWindow.Instance.LMStudio.ChatCompletion(Conversation.ToList(), ThinkButton.IsChecked.Value, ToolButton.IsChecked.Value, response => {
                //add response to conversation in UI thread!!!
                dis.TryEnqueue(() => AddMessageToConversation(response.Choices[0].Message));
            });
        }

        private void ScrollToLastMessage() {
            MessagesList.UpdateLayout();
            MessagesList.SmoothScrollIntoViewWithIndexAsync(MessagesList.Items.Count-1, ScrollItemPlacement.Top, false, false);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e) {
            SettingsPopup.IsOpen = true;
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e) {
            Conversation[0] = new Message() { Role = "system", Content = SystemPromptTextBox.Text };
            SettingsPopup.IsOpen = false;
        }

        //changes some expander properties that are not directly accessible in xaml
        private void ThoughtsExpander_Loaded(object sender, RoutedEventArgs e) {
            var expander = (Expander)sender;
            expander.ApplyTemplate();

            var expanderGrid = VisualTreeHelper.GetChild(expander, 0) as Grid;
            var toggle = VisualTreeHelper.GetChild(expanderGrid, 0) as ToggleButton;

            //change header color
            UIExtensions.StringToColor("#2a2a2a", out Windows.UI.Color c);
            toggle.Background = new SolidColorBrush(c);
            toggle.BorderThickness = new Thickness(0);

            //disable border
            var contentBorderClip = VisualTreeHelper.GetChild(expanderGrid, 1) as Border;
            var contentBorder = VisualTreeHelper.GetChild(contentBorderClip, 0) as Border;
            contentBorder.BorderThickness = new Thickness(0);

            //scroll when expander enlarges
            expander.SizeChanged += (o, args) => {
                if(expander.IsExpanded) MessagesList.SmoothScrollIntoViewWithItemAsync(expander.DataContext);
            };
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

        public Visibility MainContentVisible => MainContent.Trim('\n') != "" ? Visibility.Visible : Visibility.Collapsed;

        public string Thoughts => Content == null ? "" : Content.GetTag("think");

        public string MainContent => Content == null ? "" : Content.RemoveTag("think", out _);

        public Message WithoutThinkSection() {
            Message clone = Clone();
            clone.Content = clone.Content.RemoveTag("think", out _);

            return clone;
        }
    }
}
