namespace AnnouncementFormatter.Core.Models;

/// <summary>
/// Represents a complete announcement document.
/// </summary>
public class AnnouncementDocument
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = "Untitled Document";

    public string Subtitle { get; set; } = "Civil Air Patrol - Heartland Composite Squadron";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime ModifiedAt { get; set; } = DateTime.Now;
    public List<DocumentWidget> Widgets { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
}
