using System.Text;
using AnnouncementFormatter.Core.Models;

namespace AnnouncementFormatter.Core.Services;

/// <summary>
/// Renders documents to HTML format with inline CSS.
/// </summary>
public class HtmlRenderingService
{
    private readonly WidgetLibraryService _widgetLibrary;
    private readonly string _templateDirectory;

    public HtmlRenderingService(WidgetLibraryService widgetLibrary, string templateDirectory)
    {
        _widgetLibrary = widgetLibrary;
        _templateDirectory = templateDirectory;
    }

    /// <summary>
    /// Renders a complete document to HTML with inline CSS.
    /// </summary>
    public async Task<string> RenderDocumentAsync(AnnouncementDocument document)
    {
        var sb = new StringBuilder();

        // HTML header with inline styles
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("    <meta charset=\"UTF-8\">");
        sb.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        sb.AppendLine($"    <title>{EscapeHtml(document.Title)}</title>");
        sb.AppendLine("    <link rel=\"preconnect\" href=\"https://fonts.googleapis.com\">");
        sb.AppendLine("    <link rel=\"preconnect\" href=\"https://fonts.gstatic.com\" crossorigin>");
        sb.AppendLine("    <link href=\"https://fonts.googleapis.com/css2?family=Ubuntu:wght@400;500;700&display=swap\" rel=\"stylesheet\">");
        sb.AppendLine("    <style>");
        sb.AppendLine(GetBaseStyles());
        sb.AppendLine("    </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body style=\"font-family: 'Ubuntu', Arial, sans-serif; background: linear-gradient(135deg, #001489 0%, #0056d2 100%); color: #333; line-height: 1.6; padding: 20px; min-height: 100vh; margin: 0;\">");
        sb.AppendLine("    <div style=\"max-width: 1000px; margin: 0 auto; background: #fff; box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3); border-radius: 12px; overflow: hidden;\">");
        
        // Header with document title
        sb.AppendLine("        <div style=\"background: linear-gradient(135deg, #001489 0%, #003ab8 100%); color: #fff; padding: 50px 40px; text-align: center; position: relative; overflow: hidden; border-top: 6px solid; border-image: linear-gradient(90deg, #FFCD00 0%, #BA0C2F 50%, #FFCD00 100%) 1;\">");
        sb.AppendLine($"            <h1 style=\"font-size: clamp(1.5em, 5vw, 2.8em); margin: 0 0 10px 0; font-weight: 800; text-transform: uppercase; letter-spacing: 2px; text-shadow: 0 2px 8px rgba(0,0,0,0.25), 0 0 2px #fff; line-height: 1.1; color: #fff; background: rgba(0,0,0,0.08); border-radius: 8px; padding: 0.2em 0.5em; display: inline-block; word-break: break-word;\">{EscapeHtml(document.Title)}</h1>");
        sb.AppendLine($"            <p style=\"font-size: clamp(1em, 2.5vw, 1.3em); font-weight: 500; opacity: 0.98; margin: 0; color: #FFCD00; text-shadow: 0 1px 4px rgba(0,0,0,0.18); background: rgba(0,0,0,0.05); border-radius: 6px; padding: 0.15em 0.4em; display: inline-block; word-break: break-word;\">{EscapeHtml(document.Subtitle)}</p>");
        sb.AppendLine("        </div>");
        
        // Content area
        sb.AppendLine("        <div style=\"padding: 40px;\">");

        // Render each widget
        foreach (var widget in document.Widgets.OrderBy(w => w.Order))
        {
            var definition = _widgetLibrary.GetWidgetById(widget.WidgetDefinitionId);
            if (definition != null)
            {
                var widgetHtml = await RenderWidgetAsync(widget, definition);
                sb.AppendLine(widgetHtml);
            }
        }

        sb.AppendLine("        </div>");
        
        // Footer
        sb.AppendLine("        <div style=\"background: linear-gradient(135deg, #f5f5f5 0%, #e8e8e8 100%); padding: 30px; text-align: center; font-size: 1em; color: #666; border-top: 6px solid; border-image: linear-gradient(90deg, #FFCD00 0%, #BA0C2F 50%, #001489 100%) 1;\">");
        sb.AppendLine($"            <p style=\"margin: 0;\">Generated on {DateTime.Now:MMMM dd, yyyy}</p>");
        sb.AppendLine("        </div>");
        
        sb.AppendLine("    </div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    /// <summary>
    /// Renders a single widget instance.
    /// </summary>
    private async Task<string> RenderWidgetAsync(DocumentWidget widget, WidgetDefinition definition)
    {
        var templatePath = Path.Combine(_templateDirectory, definition.Template);
        string template;

        if (File.Exists(templatePath))
        {
            template = await File.ReadAllTextAsync(templatePath);
        }
        else
        {
            // Fallback to default template
            template = GetDefaultWidgetTemplate();
        }

        // Replace variables in template
        foreach (var field in widget.Fields)
        {
            var placeholder = $"{{{{{field.Name}}}}}";
            template = template.Replace(placeholder, EscapeHtml(field.Value));
        }
        
        // Apply widget colors if defined
        var colors = definition.Colors ?? new WidgetColors();
        template = template.Replace("{{color_primary}}", colors.Primary);
        template = template.Replace("{{color_background}}", colors.Background);
        template = template.Replace("{{color_text}}", colors.Text);
        template = template.Replace("{{color_accent}}", colors.Accent);

        // Replace any remaining placeholders with empty string
        template = System.Text.RegularExpressions.Regex.Replace(template, @"\{\{[^}]+\}\}", "");

        return template;
    }

    /// <summary>
    /// Gets base CSS styles for the document.
    /// </summary>
    private string GetBaseStyles()
    {
        return @"
            body {
                font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                line-height: 1.6;
                color: #333;
                max-width: 800px;
                margin: 0 auto;
                padding: 20px;
                background-color: #f5f5f5;
            }
            .document-container {
                background: white;
                padding: 40px;
                box-shadow: 0 2px 10px rgba(0,0,0,0.1);
            }
            .document-title {
                font-size: 2em;
                color: #2c3e50;
                border-bottom: 3px solid #3498db;
                padding-bottom: 10px;
                margin-bottom: 30px;
            }
            .widget {
                margin-bottom: 30px;
                padding: 20px;
                border-left: 4px solid #3498db;
                background-color: #f9f9f9;
            }
            .widget-title {
                font-size: 1.5em;
                color: #2c3e50;
                margin-bottom: 15px;
            }
            .widget-field {
                margin-bottom: 10px;
            }
            .field-label {
                font-weight: bold;
                color: #555;
                margin-right: 8px;
            }
            .field-value {
                color: #333;
            }
            .alert-widget {
                border-left-color: #e74c3c;
                background-color: #fef5f5;
            }
            .info-widget {
                border-left-color: #3498db;
                background-color: #f0f8ff;
            }
            .warning-widget {
                border-left-color: #f39c12;
                background-color: #fffbf0;
            }
        ";
    }

    /// <summary>
    /// Gets a default widget template if no custom template exists.
    /// </summary>
    private string GetDefaultWidgetTemplate()
    {
        return @"
        <div class=""widget"">
            <div class=""widget-content"">
                {{content}}
            </div>
        </div>";
    }

    /// <summary>
    /// Escapes HTML special characters.
    /// </summary>
    private string EscapeHtml(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;");
    }

    /// <summary>
    /// Exports document to HTML file.
    /// </summary>
    public async Task ExportToFileAsync(AnnouncementDocument document, string outputPath)
    {
        var html = await RenderDocumentAsync(document);
        await File.WriteAllTextAsync(outputPath, html);
    }
}
