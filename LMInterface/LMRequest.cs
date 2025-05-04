using System.Collections.Generic;
using Newtonsoft.Json;

namespace LMInterface
{
    public class LMRequest {
        [JsonProperty("model")] public required string Model { get; set; }
        [JsonProperty("messages")] public required List<Message> Messages { get; set; }
        [JsonProperty("tools")] public List<Tool>? Tools { get; set; }
        [JsonProperty("tool_choice")] public string? ToolChoice { get; set; } // Can be string ("auto", "none", "required")
        [JsonProperty("max_tokens")] public long MaxTokens { get; set; }
        [JsonProperty("temperature")] public double Temperature { get; set; }
        [JsonProperty("top_p")] public double TopP { get; set; }
        [JsonProperty("top_k")] public double TopK { get; set; }
        [JsonProperty("stream")] public bool Stream { get; set; }
    }

    public partial class Message {

        public Message Clone() {
            return JsonConvert.DeserializeObject<Message>(JsonConvert.SerializeObject(this))!;
        }

        [JsonProperty("role")] public required string Role { get; set; } // "system", "user", "assistant"
        [JsonProperty("content")] public required string Content { get; set; } // the actual message
    }

    //used when responding to tool calls
    public class ToolCallResponse : Message {
        [JsonProperty("tool_call_id")] public required string ToolCallId { get; set; }
    }

    public class Tool {
        [JsonProperty("type")] private string Type => "function"; //must be function
        [JsonProperty("function")] public required ToolFunction Function { get; set; } //defines function of tool
    }

    public class ToolFunction {
        [JsonProperty("name")] public required string Name { get; set; } //name of tool
        [JsonProperty("description")] public string? Description { get; set; } //description of tool
        [JsonProperty("parameters")] public Parameters? Parameters { get; set; } //defines all parameters
    }

    public class Parameters {
        [JsonProperty("type")] private string Type => "object"; //must be object
        [JsonProperty("properties")] public required Dictionary<string, PropertyAnnotations> Properties { get; set; }
        [JsonProperty("required")] public required List<string> Required { get; set; } //defines the properties that have to be used by the model
    }

    public class PropertyAnnotations {
        [JsonProperty("type")] public required string Type { get; set; } //integer, string, object etc.. (can also be ["string", "null"] to allow null as option)
        [JsonProperty("description")] public string? Description { get; set; }
        [JsonProperty("enum")] public List<string>? Enum { get; set; } //used to define possible values of the property
        //[JsonProperty("exclusiveMinimum")] public double ExclusiveMinimum { get; set; } //used to define the minimum value of the property
    }
}
