using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace LMInterface.Serializables
{
    public class LMResponse {
        [JsonProperty("id")] public required string Id { get; set; }
        [JsonProperty("object")] public string Object => "chat.completion"; //must be chat.completion
        [JsonProperty("created")] public int Created { get; set; } //unix timestamp in s
        [JsonProperty("model")] public required string Model { get; set; }
        [JsonProperty("choices")] public required List<Choice> Choices { get; set; } //basically always take the first element
        [JsonProperty("usage")] public required Usage Usage { get; set; } //some data about token usage
        [JsonProperty("stats")] public Stats? Stats { get; set; }
        [JsonProperty("system_fingerprint")] public required string SystemFingerprint { get; set; }
    }

    public class Choice {
        [JsonProperty("index")] public int Index { get; set; }
        [JsonProperty("logprobs")] public object? Logprobs { get; set; }
        [JsonProperty("finish_reason")] public required string FinishReason { get; set; }
        [JsonProperty("message")] public required ApiMessage Message { get; set; }
    }

    public class ToolCall {
        [JsonProperty("id")] public required string Id { get; set; }
        [JsonProperty("type")] private string Type => "function"; //must be function
        [JsonProperty("function")] public required ToolCallArguments ToolCallArguments { get; set; } //contains the function arguments

        [SetsRequiredMembers]
        public ToolCall() {
            Id = "";
            ToolCallArguments = new();
        }
    }

    public class ToolCallArguments {
        [JsonProperty("name")] public required string Name { get; set; } //the name of the tool
        [JsonProperty("arguments")] public required string Arguments { get; set; } //defines all arguments in json format

        [SetsRequiredMembers]
        public ToolCallArguments() {
            Name = "";
            Arguments = "";
        }
    }
    

    public class Usage {
        [JsonProperty("prompt_tokens")] public int PromptTokens { get; set; }

        [JsonProperty("completion_tokens")] public int CompletionTokens { get; set; }

        [JsonProperty("total_tokens")] public int TotalTokens { get; set; }
    }

    public class Stats {

    }
}
