using System.Text.Json;
using AnnouncementFormatter.Core.Models;

namespace AnnouncementFormatter.Core.Services;

/// <summary>
/// Loads and manages widget definitions from JSON files.
/// </summary>
public class WidgetLibraryService
{
    private readonly Dictionary<string, WidgetDefinition> _widgets = new();
    private readonly string _widgetDirectory;
    private readonly string _templateDirectory;

    public WidgetLibraryService(string widgetDirectory, string templateDirectory)
    {
        _widgetDirectory = widgetDirectory;
        _templateDirectory = templateDirectory;
    }

    /// <summary>
    /// Loads all widget definitions from the configured directory.
    /// </summary>
    public async Task LoadWidgetsAsync()
    {
        _widgets.Clear();

        if (!Directory.Exists(_widgetDirectory))
        {
            Directory.CreateDirectory(_widgetDirectory);
            return;
        }

        var jsonFiles = Directory.GetFiles(_widgetDirectory, "*.json", SearchOption.AllDirectories);

        foreach (var file in jsonFiles)
        {
            try
            {
                var json = await File.ReadAllTextAsync(file);
                var widget = JsonSerializer.Deserialize<WidgetDefinition>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (widget != null && !string.IsNullOrEmpty(widget.Id))
                {
                    _widgets[widget.Id] = widget;
                }
            }
            catch (Exception ex)
            {
                // Log error (in production, use proper logging)
                Console.WriteLine($"Failed to load widget from {file}: {ex.Message}");
            }
        }
    }

    public async Task ReloadAsync()
    {
        _widgets.Clear();
        await LoadWidgetsAsync();
    }

    public async Task<bool> DeleteWidgetAsync(string widgetId)
    {
        if (string.IsNullOrWhiteSpace(widgetId)) return false;

        var removed = _widgets.Remove(widgetId);

        var jsonPath = Path.Combine(_widgetDirectory, $"{widgetId}.json");
        if (File.Exists(jsonPath))
        {
            File.Delete(jsonPath);
        }

        var templatePath = Path.Combine(_templateDirectory, $"{widgetId}.html");
        if (File.Exists(templatePath))
        {
            File.Delete(templatePath);
        }

        await ReloadAsync();
        return removed;
    }

    /// <summary>
    /// Gets all loaded widget definitions.
    /// </summary>
    public IEnumerable<WidgetDefinition> GetAllWidgets()
    {
        return _widgets.Values;
    }

    /// <summary>
    /// Gets widgets filtered by user mode.
    /// </summary>
    public IEnumerable<WidgetDefinition> GetWidgetsForMode(UserMode mode)
    {
        return _widgets.Values.Where(w => 
            w.AllowedModes == null || 
            w.AllowedModes.Count == 0 || 
            w.AllowedModes.Contains(mode));
    }

    /// <summary>
    /// Gets widgets grouped by category.
    /// </summary>
    public Dictionary<string, List<WidgetDefinition>> GetWidgetsByCategory(UserMode mode)
    {
        return GetWidgetsForMode(mode)
            .GroupBy(w => w.Category)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    /// <summary>
    /// Gets a specific widget definition by ID.
    /// </summary>
    public WidgetDefinition? GetWidgetById(string id)
    {
        _widgets.TryGetValue(id, out var widget);
        return widget;
    }

    /// <summary>
    /// Saves a widget definition to a JSON file (Expert mode only).
    /// </summary>
    public async Task SaveWidgetDefinitionAsync(WidgetDefinition widget)
    {
        var fileName = $"{widget.Id}.json";
        var filePath = Path.Combine(_widgetDirectory, fileName);

        var json = JsonSerializer.Serialize(widget, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(filePath, json);
        _widgets[widget.Id] = widget;
    }

    /// <summary>
    /// Gets the template HTML content for a widget.
    /// </summary>
    public async Task<string> GetTemplateContentAsync(string widgetId)
    {
        var templatePath = Path.Combine(_templateDirectory, $"{widgetId}.html");
        if (File.Exists(templatePath))
        {
            return await File.ReadAllTextAsync(templatePath);
        }
        return string.Empty;
    }

    /// <summary>
    /// Saves template HTML content for a widget (Expert mode only).
    /// </summary>
    public async Task SaveTemplateAsync(string widgetId, string htmlContent)
    {
        var templatePath = Path.Combine(_templateDirectory, $"{widgetId}.html");
        Directory.CreateDirectory(_templateDirectory);
        await File.WriteAllTextAsync(templatePath, htmlContent);
    }

    /// <summary>
    /// Deletes a template file for a widget (Expert mode only).
    /// </summary>
    public async Task DeleteTemplateAsync(string widgetId)
    {
        var templatePath = Path.Combine(_templateDirectory, $"{widgetId}.html");
        if (File.Exists(templatePath))
        {
            File.Delete(templatePath);
        }
        await Task.CompletedTask;
    }
}
