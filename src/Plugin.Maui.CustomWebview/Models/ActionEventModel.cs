namespace Plugin.Maui.CustomWebview.Models;

[JsonObject]
public class ActionEventModel
{
    [JsonProperty("action", Required = Required.Always)]
    public string Action { get; set; }

    [JsonProperty("data")]
    public string Data { get; set; }
}