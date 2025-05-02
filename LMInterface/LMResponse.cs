using System.Collections.Generic;
using Newtonsoft.Json;

namespace LMInterface
{
    public class LMResponse {

        public bool ToolCall => Choices[0].Message.ToolCalls != null;

        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("object")] public string Object { get; set; } = "chat.completion"; // must be chat.completion

        [JsonProperty("created")] public int Created { get; set; } // unix timestamp in s

        [JsonProperty("model")] public string Model { get; set; }

        [JsonProperty("choices")] public List<Choice> Choices { get; set; } // basically always take the first element

        [JsonProperty("usage")] public Usage Usage { get; set; } //some data about token usage

        [JsonProperty("stats")] public Stats Stats { get; set; }
        [JsonProperty("system_fingerprint")] public string SystemFingerprint { get; set; }
    }

    public class Choice {
        [JsonProperty("index")] public int Index { get; set; }

        [JsonProperty("logprobs")] public object Logprobs { get; set; }

        [JsonProperty("finish_reason")] public string FinishReason { get; set; }

        [JsonProperty("message")] public Message Message { get; set; }
    }

    public class Usage {
        [JsonProperty("prompt_tokens")] public int PromptTokens { get; set; }

        [JsonProperty("completion_tokens")] public int CompletionTokens { get; set; }

        [JsonProperty("total_tokens")] public int TotalTokens { get; set; }
    }

    public class Stats {

    }
}
