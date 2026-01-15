using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AnnouncementFormatter.Core.Models;

namespace AnnouncementFormatter.ViewModels;

public partial class WidgetEditorViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _widgetId = string.Empty;

    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private string _category = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _primaryColor = "#001489";

    [ObservableProperty]
    private string _backgroundColor = "#FFFFFF";

    [ObservableProperty]
    private string _textColor = "#333333";

    [ObservableProperty]
    private string _accentColor = "#FFCD00";

    [ObservableProperty]
    private string _htmlTemplate = @"<div style=""border: 2px solid {{color_primary}}; padding: 15px; border-radius: 4px; background: {{color_background}}; color: {{color_text}};"">
  <h3 style=""color: {{color_primary}}; margin: 0 0 10px 0;"">{{title}}</h3>
  <p style=""margin: 0; line-height: 1.6; color: {{color_text}};"">{{content}}</p>
  <div style=""margin-top: 10px; padding: 8px; background: {{color_accent}}; color: #000; border-radius: 3px; font-weight: 500;"">Accent Example</div>
</div>";

    [ObservableProperty]
    private string _previewHtml = string.Empty;

    public ObservableCollection<FieldEditorModel> Fields { get; } = new();

    public bool DialogResult { get; private set; }
    public WidgetDefinition? CreatedWidget { get; private set; }

    public WidgetEditorViewModel()
    {
        // Add default field examples
        Fields.Add(new FieldEditorModel { Id = "title", Label = "Title", Type = "String", DefaultValue = "Example Title" });
        Fields.Add(new FieldEditorModel { Id = "content", Label = "Content", Type = "Multiline", DefaultValue = "Example content goes here" });
        
        RefreshPreview();
    }

    public static WidgetEditorViewModel FromDefinition(WidgetDefinition definition)
    {
        var vm = new WidgetEditorViewModel
        {
            WidgetId = definition.Id,
            DisplayName = definition.DisplayName,
            Category = definition.Category,
            Description = definition.Description ?? string.Empty,
            HtmlTemplate = File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "templates", definition.Template))
                ? File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "templates", definition.Template))
                : string.Empty,
            PrimaryColor = definition.Colors?.Primary ?? "#001489",
            BackgroundColor = definition.Colors?.Background ?? "#FFFFFF",
            TextColor = definition.Colors?.Text ?? "#333333",
            AccentColor = definition.Colors?.Accent ?? "#FFCD00"
        };

        vm.Fields.Clear();
        foreach (var f in definition.Fields)
        {
            vm.Fields.Add(new FieldEditorModel
            {
                Id = f.Id,
                Label = f.Label,
                Type = f.Type.ToString(),
                DefaultValue = f.DefaultValue ?? string.Empty
            });
        }

        vm.RefreshPreview();
        return vm;
    }

    [RelayCommand]
    private void AddField()
    {
        Fields.Add(new FieldEditorModel 
        { 
            Id = $"field{Fields.Count + 1}", 
            Label = $"Field {Fields.Count + 1}",
            Type = "String",
            DefaultValue = ""
        });
    }

    [RelayCommand]
    private void RemoveField(FieldEditorModel field)
    {
        Fields.Remove(field);
    }

    [RelayCommand]
    private void RefreshPreview()
    {
        var preview = HtmlTemplate;
        
        // Replace color placeholders
        preview = preview.Replace("{{color_primary}}", PrimaryColor);
        preview = preview.Replace("{{color_background}}", BackgroundColor);
        preview = preview.Replace("{{color_text}}", TextColor);
        preview = preview.Replace("{{color_accent}}", AccentColor);
        
        // Replace field placeholders with sample values
        foreach (var field in Fields)
        {
            if (!string.IsNullOrEmpty(field.Id))
            {
                var value = string.IsNullOrEmpty(field.DefaultValue) ? $"[{field.Label}]" : field.DefaultValue;
                preview = preview.Replace($"{{{{{field.Id}}}}}", value);
            }
        }
        
        PreviewHtml = preview;
    }

    [RelayCommand]
    private async Task Save()
    {
        // Validate
        if (string.IsNullOrWhiteSpace(WidgetId) || 
            string.IsNullOrWhiteSpace(DisplayName) || 
            string.IsNullOrWhiteSpace(Category) ||
            string.IsNullOrWhiteSpace(HtmlTemplate) ||
            Fields.Count == 0)
        {
            System.Diagnostics.Debug.WriteLine("Validation failed - missing required fields");
            return;
        }

        try
        {
            // Create widget definition
            var widgetDef = new WidgetDefinition
            {
                Id = WidgetId.ToLowerInvariant().Replace(" ", "_"),
                DisplayName = DisplayName,
                Category = Category,
                Description = Description,
                Template = $"{WidgetId.ToLowerInvariant().Replace(" ", "_")}.html",
                Version = "1.0.0",
                Fields = new List<WidgetField>(),
                Colors = new WidgetColors
                {
                    Primary = PrimaryColor,
                    Background = BackgroundColor,
                    Text = TextColor,
                    Accent = AccentColor
                }
            };

            foreach (var field in Fields)
            {
                if (string.IsNullOrWhiteSpace(field.Id)) continue;
                
                widgetDef.Fields.Add(new WidgetField
                {
                    Id = field.Id,
                    Label = field.Label,
                    Type = Enum.TryParse<FieldType>(field.Type, out var fieldType) ? fieldType : FieldType.String,
                    DefaultValue = field.DefaultValue,
                    Required = true
                });
            }

            // Save JSON file
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var widgetsDir = Path.Combine(baseDir, "Assets", "widgets");
            Directory.CreateDirectory(widgetsDir);
            
            var jsonPath = Path.Combine(widgetsDir, $"{widgetDef.Id}.json");
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(widgetDef, options);
            await File.WriteAllTextAsync(jsonPath, json);
            
            System.Diagnostics.Debug.WriteLine($"Saved widget JSON to: {jsonPath}");

            // Save HTML template
            var templatesDir = Path.Combine(baseDir, "Assets", "templates");
            Directory.CreateDirectory(templatesDir);
            
            var htmlPath = Path.Combine(templatesDir, $"{widgetDef.Id}.html");
            await File.WriteAllTextAsync(htmlPath, HtmlTemplate);
            
            System.Diagnostics.Debug.WriteLine($"Saved widget HTML to: {htmlPath}");

            CreatedWidget = widgetDef;
            DialogResult = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving widget: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogResult = false;
    }
}

public partial class FieldEditorModel : ObservableObject
{
    [ObservableProperty]
    private string _id = string.Empty;

    [ObservableProperty]
    private string _label = string.Empty;

    [ObservableProperty]
    private string _type = "String";

    [ObservableProperty]
    private string _defaultValue = string.Empty;
}
