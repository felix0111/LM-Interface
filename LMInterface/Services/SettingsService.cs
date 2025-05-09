using Newtonsoft.Json;

namespace LMInterface.Services
{
    public class SettingsService {

        //API stuff
        [JsonProperty("api_url")] public string? ApiUrl { get; set; } = "";
        [JsonProperty("selected_model")] public string? SelectedModel { get; set; } = "";
    }
}
