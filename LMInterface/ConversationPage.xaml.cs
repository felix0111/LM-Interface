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

        public ObservableCollection<Message> Conversation = new();
        
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

        /// <summary>
        /// Executed when DataContext of message textblock is set.
        /// </summary>
        private void MessageTextBox_ContextChanged(object sender, DataContextChangedEventArgs e) {
            var rtb = sender as MarkdownTextBlock;

            if (rtb.DataContext == null) return;
            rtb.Text = LMHelper.FormatRawText((Message)rtb.DataContext);

            //scroll to last message if not manually scrolled up
            Border? border = VisualTreeHelper.GetChild(MessagesList, 0) as Border;
            ScrollViewer? scrollViewer = VisualTreeHelper.GetChild(border, 0) as ScrollViewer;
            if (scrollViewer?.VerticalOffset < scrollViewer?.ScrollableHeight - 100) return;

            //scroll down to last message 
            var lastItem = MessagesList.Items[^1];
            MessagesList.UpdateLayout();
            MessagesList.ScrollIntoView(lastItem);
        }
    }

    public partial class Message {
        public HorizontalAlignment MessageAlignment => Role == "assistant" ? HorizontalAlignment.Left : HorizontalAlignment.Right;
        public Thickness MessageMargin => Role == "assistant" ? new Thickness(0, 20, 50, 20) : new Thickness(50, 20, 0, 20);
    }
}
