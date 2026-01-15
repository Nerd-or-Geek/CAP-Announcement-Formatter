namespace AnnouncementFormatter.Core.Models;

/// <summary>
/// Represents an instance of a widget in a document.
/// </summary>
public class DocumentWidget
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string WidgetDefinitionId { get; set; } = string.Empty;
    public int Order { get; set; }
    public List<DocumentFieldValue> Fields { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime ModifiedAt { get; set; } = DateTime.Now;
}
