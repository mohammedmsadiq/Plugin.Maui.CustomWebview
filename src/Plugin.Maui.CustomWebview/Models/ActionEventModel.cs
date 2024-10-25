using Newtonsoft.Json;

namespace Plugin.Maui.CustomWebView.Models;

[JsonObject]
public class ActionEventModel
{
    [JsonProperty("action", Required = Required.Always)]
    public string Action { get; set; }

    [JsonProperty("data")]
    public string Data { get; set; }
}