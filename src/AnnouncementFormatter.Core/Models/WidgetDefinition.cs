using System.Text.Json.Serialization;

namespace AnnouncementFormatter.Core.Models;

/// <summary>
/// Defines a reusable widget template (loaded from JSON).
/// </summary>
public class WidgetDefinition
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = "General";

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    [JsonPropertyName("template")]
    public string Template { get; set; } = string.Empty;

    [JsonPropertyName("fields")]
    public List<WidgetField> Fields { get; set; } = new();

    [JsonPropertyName("allowedModes")]
    public List<UserMode>? AllowedModes { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0.0";

    [JsonPropertyName("colors")]
    public WidgetColors? Colors { get; set; }
}

public class WidgetColors
{
    [JsonPropertyName("primary")]
    public string Primary { get; set; } = "#001489";

    [JsonPropertyName("background")]
    public string Background { get; set; } = "#FFFFFF";

    [JsonPropertyName("text")]
    public string Text { get; set; } = "#333333";

    [JsonPropertyName("accent")]
    public string Accent { get; set; } = "#FFCD00";
}
