using Newtonsoft.Json;

namespace LMInterface
{
    public class Settings {

        //API stuff
        [JsonProperty("api_url")] public string ApiUrl { get; set; }
        [JsonProperty("selected_model")] public string SelectedModel { get; set; }
    }
}
