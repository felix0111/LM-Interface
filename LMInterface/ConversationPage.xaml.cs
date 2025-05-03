using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Input;
using Newtonsoft.Json;

namespace LMInterface
{
    public sealed partial class ConversationPage : Page {

        public ObservableCollection<Message> Conversation = new() {new Message() {Role = "system", Content = ""}};
        
        public ConversationPage()
        {
            this.InitializeComponent();
        }

        public void ProcessUserMessage(TextBox textBlock) {
            string rawText = textBlock.Text.Replace("\r", "  \n");

            //add user message to conversation history and clear input box
            Conversation.Add(new () {Role = "user", Content = rawText, Think = ThinkButton.IsChecked!.Value});
            textBlock.Text = "";

            //send message to language model (async)
            var dis = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            Task task = MainWindow.Instance.LMStudio.ChatCompletion(Conversation.ToList(), ThinkButton.IsChecked.Value, response => {
                //add message in UI thread!
                dis.TryEnqueue(() => { Conversation.Add(response.Choices[0].Message); });
            });
        }

        /// <summary>
        /// Checks when user presses Enter on TextBlock.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="keyRoutedEventArgs"></param>
        public void SendMessage_InputBox(object sender, KeyRoutedEventArgs keyRoutedEventArgs) {
            if (keyRoutedEventArgs.Key == VirtualKey.Enter) {
                if (InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down)) {
                    keyRoutedEventArgs.Handled = false;
                    return;
                }

                // Block the Enter key from creating a new line
                keyRoutedEventArgs.Handled = true;

                var textBlock = (TextBox)sender;
                if (textBlock.Text.Trim() == String.Empty) return;
                ProcessUserMessage(textBlock);
            }
        }

        private void ScrollToLastMessage(bool force) {
            //if not forced, it may return when user manually scrolled up by some amount
            if (!force) {
                Border? border = VisualTreeHelper.GetChild(MessagesList, 0) as Border;
                ScrollViewer? scrollViewer = VisualTreeHelper.GetChild(border, 0) as ScrollViewer;
                if (scrollViewer?.VerticalOffset < scrollViewer?.ScrollableHeight - 100) return;
            }

            //scroll down to last message 
            var lastItem = MessagesList.Items[^1];
            MessagesList.UpdateLayout();
            MessagesList.ScrollIntoView(lastItem);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e) {
            SettingsPopup.IsOpen = true;
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e) {
            Conversation[0] = new Message() { Role = "system", Content = SystemPromptTextBox.Text };
            SettingsPopup.IsOpen = false;
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
                if (Role == "system") return new Thickness(0, 20, 20, 0);
                return Role == "assistant" ? new Thickness(0, 20, 50, 20) : new Thickness(50, 20, 0, 20);
            }
        }

        public Visibility ThoughtsVisible => Thoughts != "" ? Visibility.Visible : Visibility.Collapsed;

        public string Thoughts => Content.GetTag("think");

        public string MainContent => Content.RemoveTag("think", out _);

        public Message WithoutThinkSection() {
            Message clone = Clone();
            clone.Content = clone.Content.RemoveTag("think", out _);

            return clone;
        }
    }
}
