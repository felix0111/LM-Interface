using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LMInterface.Services;
using Newtonsoft.Json;

namespace LMInterface
{
    public static class LMStudioInterface {

        private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromHours(1) };

        private static string ChatCompletionsUrl => ServiceProvider.Settings.ApiUrl.TrimEnd('/') + "/chat/completions";
        private static string ModelsUrl => ServiceProvider.Settings.ApiUrl.TrimEnd('/') + "/models";

        public static JsonSerializerSettings JsonSettings = new() { NullValueHandling = NullValueHandling.Ignore};

        private static bool _clientInUse;

        public static async Task ChatCompletion(List<Message> conversation, bool think, bool allowTools, Action<LMResponse> responseHandling) {
            if (_clientInUse) return;
            _clientInUse = true;

            //make request object
            LMRequest request = LMHelper.MakeApiRequest(ServiceProvider.Settings.SelectedModel, conversation, think, allowTools ? new List<Tool>() { new WebTool() } : null, allowTools ? "auto" : "none");

            //convert request object to json
            var json = JsonConvert.SerializeObject(request, JsonSettings);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            //send content and wait for response
            HttpResponseMessage response = await HttpClient.PostAsync(ChatCompletionsUrl, content);
            response.EnsureSuccessStatusCode();

            //read response as json and convert to response object
            string responseContent = await response.Content.ReadAsStringAsync();
            LMResponse modelResponse = JsonConvert.DeserializeObject<LMResponse>(responseContent, JsonSettings) ?? throw new Exception("JSON could not deserialize 'chat/completions' response!");

            _clientInUse = false;
            responseHandling.Invoke(modelResponse);
        }

        public static async Task SendToolResults(List<Message> conversation, bool think, Action<List<Message>>toolResultsMessages, Action<LMResponse> actualResponse) {
            if (_clientInUse || !conversation[^1].IsToolCall) return; //shouldnt ever happen
            _clientInUse = true;

            //do not give tools in toolcall response to avoid repeating tool calls
            LMRequest toolCallReponse = LMHelper.MakeApiRequest(ServiceProvider.Settings.SelectedModel, conversation, think, null, "none");
            //re-add the last toolcall message because it's normally sorted out
            toolCallReponse.Messages.Add(conversation[^1]);
            
            //TODO might need to add /no_think to the end of each response when !think
            //get all the tool results
            List<Message> toolResults = await LMHelper.GetToolResults(conversation[^1].ToolCalls!);

            //send the results so the UI can already show them
            toolResultsMessages.Invoke(toolResults);

            //add results of the called tools to the conversation history
            toolCallReponse.Messages.AddRange(toolResults);

            //convert json
            var json = JsonConvert.SerializeObject(toolCallReponse, JsonSettings);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            //send content and wait for response
            HttpResponseMessage response = await HttpClient.PostAsync(ChatCompletionsUrl, content);
            response.EnsureSuccessStatusCode();

            //read response as json and convert to response object
            string responseContent = await response.Content.ReadAsStringAsync();
            LMResponse modelResponse = JsonConvert.DeserializeObject<LMResponse>(responseContent, JsonSettings) ?? throw new Exception("JSON could not deserialize response!");

            _clientInUse = false;
            actualResponse.Invoke(modelResponse);
        }

        //TODO Invoke smth before return
        public static async Task GetAvailableModels(Action<ModelsResponse> result) {
            if (_clientInUse) return;
            if (!HttpHelper.ValidateUrl(ModelsUrl)) return;
            _clientInUse = true;

            //get all available models
            HttpResponseMessage response = await HttpClient.GetAsync(ModelsUrl);
            response.EnsureSuccessStatusCode();

            //deserialize response
            string responseContent = await response.Content.ReadAsStringAsync();
            ModelsResponse mr = JsonConvert.DeserializeObject<ModelsResponse>(responseContent, JsonSettings) ?? throw new Exception("JSON could not deserialize 'models' response!");

            //set data to empty list to avoid nullreferenceexceptions
            if (mr.Error != null) mr.Data = new();

            _clientInUse = false;
            result.Invoke(mr);
        }
    }

    public class ModelsResponse {
        [JsonProperty("data")] public required List<Model> Data { get; set; }
        [JsonProperty("object")] private string Object => "list";
        [JsonProperty("error")] public string? Error { get; set; }
    }

    public class Model {

        public string GetName {
            get {
                int id = Id.IndexOf('/') + 1;
                string formatted = Id.Substring(id).Replace('-', ' ');
                return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(formatted);
            }
        }
        public string GetPublisher {
            get {
                int maxId = Id.IndexOf('/');
                if (maxId == -1) maxId = 0;
                string formatted = Id.Substring(0, maxId).Replace('-', ' ');
                return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(formatted);
            }
        }

        [JsonProperty("id")] public required string Id { get; set; }
        [JsonProperty("object")] private string Object => "model";
        [JsonProperty("owned_by")] public required string OwnedBy { get; set; }
    }
}