using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Newtonsoft.Json;

namespace LMInterface.Services {
    public class ConversationService {

        [JsonProperty("conversations")] public required ObservableCollection<Conversation> Conversations { get; set; }
        [JsonProperty("current_conversation")] public int CurrentConversation { get; set; } = -1;

        [SetsRequiredMembers]
        public ConversationService() {
            Conversations = new();
        }

        /// <summary>
        /// Must be called in the UI thread!
        /// </summary>
        public Conversation NewConversation(string modelId) {
            int newId = Conversations.Count == 0 ? 0 : Conversations.Max(o => o.ConversationId) + 1;
            var conv = new Conversation() { ConversationId = newId, ModelId = modelId, Messages = new() };
            Conversations.Add(conv);
            return conv;
        }

        public Conversation? GetConversation() => Conversations.FirstOrDefault(o => o.ConversationId == CurrentConversation);

        /// <summary>
        /// Must be called in the UI thread!
        /// </summary>
        public void RemoveConversation(Conversation conversation) {
            Conversations.Remove(conversation);
        }

        /// <summary>
        /// New message are not automatically added to the conversation!
        /// </summary>
        public async IAsyncEnumerable<Message> WaitForResponse(bool think, bool allowTools) {
            //get chat completion for current conversation
            var conv = GetConversation() ?? throw new Exception("Could not find conversation!");
            var response = await LMStudioInterface.ChatCompletion(conv, think, allowTools).ConfigureAwait(false);

            //callback response message
            yield return response.Choices[0].Message;

            //if no tool call, break
            if (!response.Choices[0].Message.IsToolCall) yield break;

            //get tool results
            var toolResults = await LMHelper.GetToolResults(response.Choices[0].Message.ToolCalls!).ConfigureAwait(false);

            //add all tool results to conversation
            foreach (var result in toolResults) yield return result;

            //get chat completion for current conversation
            //no tools allowed to avoid repeating tool calls
            var toolResponse = await LMStudioInterface.ChatCompletion(conv, think, false).ConfigureAwait(false);

            //callback final response
            yield return toolResponse.Choices[0].Message;
        }
    }

    public class Conversation {
        [JsonProperty("conversation_id")] public int ConversationId { get; set; }
        [JsonProperty("model_id")] public required string ModelId { get; set; }
        [JsonProperty("messages")] public required ObservableCollection<Message> Messages { get; set; }

        [SetsRequiredMembers]
        public Conversation() {
            ModelId = "";
            Messages = new ();
        }

        public void AddMessage(Message msg) {
            Messages.Add(msg);
        }

        public Message SetSystemMessage(string message, out bool createdNew) {
            var newMsg = new Message() { Role = "system", Content = message };

            //if no system message exists, create new one
            if (Messages.Count == 0 || Messages[0].Role != "system") {
                Messages.Insert(0, newMsg);
                createdNew = true;
                return Messages[0];
            }

            Messages[0] = newMsg;
            createdNew = false;
            return Messages[0];
        }
    }
}