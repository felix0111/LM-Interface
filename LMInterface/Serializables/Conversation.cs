using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace LMInterface.Serializables {
    public class Conversation {
        [JsonProperty("conversation_id")] public int ConversationId { get; set; }
        [JsonProperty("model_id")] public required string ModelId { get; set; }
        [JsonProperty("max_tokens")] public long MaxTokens { get; set; }
        [JsonProperty("no_reasoning_token")] public required string NoReasoningToken { get; set; }
        [JsonProperty("messages")] public required ObservableCollection<ApiMessage> Messages { get; set; }

        [SetsRequiredMembers]
        public Conversation() {
            ModelId = "";
            MaxTokens = 4096;
            NoReasoningToken = "/no_think";
            Messages = new();
        }

        public void AddMessage(ApiMessage msg) {
            Messages.Add(msg);
        }

        public ApiMessage SetSystemMessage(string message, out bool createdNew) {
            var newMsg = new ApiMessage() { Role = "system", Content = message };

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

        public ApiMessage? GetSystemMessage() {
            if (Messages.Count == 0 || Messages[0].Role != "system") return null;

            return Messages[0];
        }
    }
}