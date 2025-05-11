using Newtonsoft.Json;

namespace LMInterface.Services
{
    public class SettingsService {

        //API stuff
        [JsonProperty("api_url")] public string? ApiUrl { get; set; } = "http://localhost:1234/v1";
    }
}
