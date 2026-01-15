using System.Text.Json.Serialization;

namespace AnnouncementFormatter.Core.Models;

/// <summary>
/// Defines a field within a widget (loaded from JSON).
/// </summary>
public class WidgetField
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public FieldType Type { get; set; }

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("required")]
    public bool Required { get; set; } = true;

    [JsonPropertyName("defaultValue")]
    public string? DefaultValue { get; set; }

    [JsonPropertyName("placeholder")]
    public string? Placeholder { get; set; }

    [JsonPropertyName("options")]
    public List<string>? Options { get; set; }

    [JsonPropertyName("validation")]
    public string? ValidationPattern { get; set; }

    [JsonPropertyName("helpText")]
    public string? HelpText { get; set; }
}
