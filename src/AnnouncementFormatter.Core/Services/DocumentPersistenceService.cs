using System.Xml.Linq;
using AnnouncementFormatter.Core.Models;

namespace AnnouncementFormatter.Core.Services;

/// <summary>
/// Handles loading and saving documents in XML format.
/// </summary>
public class DocumentPersistenceService
{
    /// <summary>
    /// Saves a document to XML format.
    /// </summary>
    public async Task SaveDocumentAsync(AnnouncementDocument document, string filePath)
    {
        var xDoc = new XDocument(
            new XDeclaration("1.0", "utf-8", "yes"),
            new XElement("Document",
                new XAttribute("Id", document.Id),
                new XAttribute("Title", document.Title),
                new XAttribute("CreatedAt", document.CreatedAt.ToString("O")),
                new XAttribute("ModifiedAt", document.ModifiedAt.ToString("O")),
                new XElement("Metadata",
                    document.Metadata.Select(kvp =>
                        new XElement("Property",
                            new XAttribute("Key", kvp.Key),
                            new XAttribute("Value", kvp.Value)))),
                new XElement("Widgets",
                    document.Widgets.OrderBy(w => w.Order).Select(widget =>
                        new XElement("Widget",
                            new XAttribute("Id", widget.Id),
                            new XAttribute("DefinitionId", widget.WidgetDefinitionId),
                            new XAttribute("Order", widget.Order),
                            new XAttribute("CreatedAt", widget.CreatedAt.ToString("O")),
                            new XAttribute("ModifiedAt", widget.ModifiedAt.ToString("O")),
                            widget.Fields.Select(field =>
                                new XElement("Field",
                                    new XAttribute("Name", field.Name),
                                    new XCData(field.Value))))))
            )
        );

        await Task.Run(() => xDoc.Save(filePath));
    }

    /// <summary>
    /// Loads a document from XML format.
    /// </summary>
    public async Task<AnnouncementDocument> LoadDocumentAsync(string filePath)
    {
        var xDoc = await Task.Run(() => XDocument.Load(filePath));
        var root = xDoc.Element("Document");

        if (root == null)
            throw new InvalidOperationException("Invalid document format: missing Document root element.");

        var document = new AnnouncementDocument
        {
            Id = root.Attribute("Id")?.Value ?? Guid.NewGuid().ToString(),
            Title = root.Attribute("Title")?.Value ?? "Untitled Document",
            CreatedAt = DateTime.Parse(root.Attribute("CreatedAt")?.Value ?? DateTime.Now.ToString("O")),
            ModifiedAt = DateTime.Parse(root.Attribute("ModifiedAt")?.Value ?? DateTime.Now.ToString("O"))
        };

        // Load metadata
        var metadata = root.Element("Metadata");
        if (metadata != null)
        {
            foreach (var prop in metadata.Elements("Property"))
            {
                var key = prop.Attribute("Key")?.Value;
                var value = prop.Attribute("Value")?.Value;
                if (key != null && value != null)
                {
                    document.Metadata[key] = value;
                }
            }
        }

        // Load widgets
        var widgets = root.Element("Widgets");
        if (widgets != null)
        {
            foreach (var widgetElement in widgets.Elements("Widget"))
            {
                var widget = new DocumentWidget
                {
                    Id = widgetElement.Attribute("Id")?.Value ?? Guid.NewGuid().ToString(),
                    WidgetDefinitionId = widgetElement.Attribute("DefinitionId")?.Value ?? string.Empty,
                    Order = int.Parse(widgetElement.Attribute("Order")?.Value ?? "0"),
                    CreatedAt = DateTime.Parse(widgetElement.Attribute("CreatedAt")?.Value ?? DateTime.Now.ToString("O")),
                    ModifiedAt = DateTime.Parse(widgetElement.Attribute("ModifiedAt")?.Value ?? DateTime.Now.ToString("O"))
                };

                foreach (var fieldElement in widgetElement.Elements("Field"))
                {
                    widget.Fields.Add(new DocumentFieldValue
                    {
                        Name = fieldElement.Attribute("Name")?.Value ?? string.Empty,
                        Value = fieldElement.Value
                    });
                }

                document.Widgets.Add(widget);
            }
        }

        return document;
    }

    /// <summary>
    /// Creates a new empty document.
    /// </summary>
    public AnnouncementDocument CreateNewDocument()
    {
        return new AnnouncementDocument
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Untitled Document",
            CreatedAt = DateTime.Now,
            ModifiedAt = DateTime.Now
        };
    }
}
