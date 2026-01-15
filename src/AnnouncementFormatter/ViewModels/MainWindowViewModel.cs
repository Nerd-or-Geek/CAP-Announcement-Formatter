using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AnnouncementFormatter.Core.Models;
using AnnouncementFormatter.Core.Services;

namespace AnnouncementFormatter.ViewModels;

/// <summary>
/// Main ViewModel for the application window.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly WidgetLibraryService _widgetLibrary;
    private readonly DocumentPersistenceService _documentService;
    private readonly HtmlRenderingService _renderingService;
    private OllamaService? _ollamaService;
    private bool _aiEnabled = false;

    [ObservableProperty]
    private string _title = "CAP Announcement Formatter";

    [ObservableProperty]
    private UserMode _currentMode = UserMode.Beginner;

    [ObservableProperty]
    private AnnouncementDocument? _currentDocument;

    [ObservableProperty]
    private string _currentFilePath = string.Empty;

    [ObservableProperty]
    private bool _hasUnsavedChanges;

    [ObservableProperty]
    private string _previewHtml = string.Empty;

    public ObservableCollection<WidgetDefinition> AvailableWidgets { get; } = new();
    public ObservableCollection<DocumentWidgetViewModel> DocumentWidgets { get; } = new();

    [ObservableProperty]
    private DocumentWidgetViewModel? _selectedWidget;

    // Mode-based visibility properties
    public bool IsBeginnerMode => CurrentMode == UserMode.Beginner;
    public bool IsIntermediateOrExpert => CurrentMode == UserMode.Intermediate || CurrentMode == UserMode.Expert;
    public bool IsExpertMode => CurrentMode == UserMode.Expert;
    public bool CanReorderWidgets => CurrentMode != UserMode.Beginner;
    public bool CanExportHtml => CurrentMode == UserMode.Intermediate || CurrentMode == UserMode.Expert;
    public bool CanCreateWidgets => CurrentMode == UserMode.Expert;

    public MainWindowViewModel()
    {
        // Get application directory
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var widgetDir = Path.Combine(baseDir, "Assets", "widgets");
        var templateDir = Path.Combine(baseDir, "Assets", "templates");

        // Ensure directories exist
        try { Directory.CreateDirectory(widgetDir); } catch { }
        try { Directory.CreateDirectory(templateDir); } catch { }

        // Initialize services
        _widgetLibrary = new WidgetLibraryService(widgetDir, templateDir);
        _documentService = new DocumentPersistenceService();
        _renderingService = new HtmlRenderingService(_widgetLibrary, templateDir);

        // Load widgets from JSON files
        LoadWidgetsFromFiles();
        
        // Create new document
        CreateNewDocument();
    }

    private void LoadWidgetsFromFiles()
    {
        AvailableWidgets.Clear();
        
        // Try loading from JSON files first
        try
        {
            var task = _widgetLibrary.LoadWidgetsAsync();
            task.Wait(3000);
            
            var loadedWidgets = _widgetLibrary.GetAllWidgets();
            foreach (var widget in loadedWidgets)
            {
                AvailableWidgets.Add(widget);
            }
            
            System.Diagnostics.Debug.WriteLine($"Loaded {AvailableWidgets.Count} widgets from JSON files");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load JSON widgets: {ex.Message}");
        }
        
        // If no widgets loaded, create defaults
        if (AvailableWidgets.Count == 0)
        {
            LoadHardcodedWidgets();
        }
    }

    private void LoadHardcodedWidgets()
    {
        AvailableWidgets.Clear();
        
        // Add hardcoded widgets
        AvailableWidgets.Add(new WidgetDefinition
        {
            Id = "meeting",
            DisplayName = "Meeting",
            Category = "Events",
            Description = "Meeting announcement",
            Template = "meeting.html",
            Fields = new List<WidgetField>
            {
                new() { Id = "title", Label = "Title", Type = FieldType.String, DefaultValue = "Staff Meeting", Required = true },
                new() { Id = "date", Label = "Date", Type = FieldType.Date, DefaultValue = "2026-01-15", Required = true },
                new() { Id = "time", Label = "Time", Type = FieldType.String, DefaultValue = "10:00 AM", Required = true },
                new() { Id = "location", Label = "Location", Type = FieldType.String, DefaultValue = "Conference Room", Required = true },
                new() { Id = "details", Label = "Details", Type = FieldType.Multiline, DefaultValue = "Meeting details", Required = false }
            }
        });

        AvailableWidgets.Add(new WidgetDefinition
        {
            Id = "alert",
            DisplayName = "Alert",
            Category = "Notices",
            Description = "Important alert",
            Template = "alert.html",
            Fields = new List<WidgetField>
            {
                new() { Id = "severity", Label = "Severity", Type = FieldType.Dropdown, DefaultValue = "Important", Required = true, Options = new List<string> { "Low", "Medium", "Important", "Critical" } },
                new() { Id = "title", Label = "Title", Type = FieldType.String, DefaultValue = "Important Notice", Required = true },
                new() { Id = "message", Label = "Message", Type = FieldType.Multiline, DefaultValue = "Alert message", Required = true },
                new() { Id = "action", Label = "Action", Type = FieldType.Multiline, DefaultValue = "Please respond", Required = false }
            }
        });

        AvailableWidgets.Add(new WidgetDefinition
        {
            Id = "announcement",
            DisplayName = "Announcement",
            Category = "General",
            Description = "General announcement",
            Template = "info.html",
            Fields = new List<WidgetField>
            {
                new() { Id = "title", Label = "Title", Type = FieldType.String, DefaultValue = "Announcement", Required = true },
                new() { Id = "content", Label = "Content", Type = FieldType.Multiline, DefaultValue = "Announcement text", Required = true },
                new() { Id = "date", Label = "Date", Type = FieldType.Date, DefaultValue = "2026-01-15", Required = false }
            }
        });

        AvailableWidgets.Add(new WidgetDefinition
        {
            Id = "regulation",
            DisplayName = "Regulation",
            Category = "Policy",
            Description = "Regulation or policy update",
            Template = "regulation.html",
            Fields = new List<WidgetField>
            {
                new() { Id = "number", Label = "Regulation #", Type = FieldType.String, DefaultValue = "CAP 60-1", Required = true },
                new() { Id = "title", Label = "Title", Type = FieldType.String, DefaultValue = "Policy Update", Required = true },
                new() { Id = "effectiveDate", Label = "Effective Date", Type = FieldType.Date, DefaultValue = "2026-01-15", Required = true },
                new() { Id = "summary", Label = "Summary", Type = FieldType.Multiline, DefaultValue = "Summary of changes", Required = true },
                new() { Id = "details", Label = "Details", Type = FieldType.Multiline, DefaultValue = "Detailed information", Required = false }
            }
        });

        AvailableWidgets.Add(new WidgetDefinition
        {
            Id = "opportunity",
            DisplayName = "Opportunity",
            Category = "Programs",
            Description = "Program or opportunity",
            Template = "info.html",
            Fields = new List<WidgetField>
            {
                new() { Id = "title", Label = "Title", Type = FieldType.String, DefaultValue = "Training Program", Required = true },
                new() { Id = "content", Label = "Description", Type = FieldType.Multiline, DefaultValue = "Program details", Required = true },
                new() { Id = "contact", Label = "Contact", Type = FieldType.String, DefaultValue = "Wing Training Officer", Required = false }
            }
        });
    }

    [RelayCommand]
    private async Task LoadWidgetsAsync()
    {
        await _widgetLibrary.LoadWidgetsAsync();
        RefreshAvailableWidgets();
    }

    private void RefreshAvailableWidgets()
    {
        AvailableWidgets.Clear();
        var widgets = _widgetLibrary.GetWidgetsForMode(CurrentMode);
        foreach (var widget in widgets)
        {
            AvailableWidgets.Add(widget);
        }
    }

    [RelayCommand]
    private void CreateNewDocument()
    {
        CurrentDocument = _documentService.CreateNewDocument();
        CurrentFilePath = string.Empty;
        DocumentWidgets.Clear();
        HasUnsavedChanges = false;
        UpdatePreview();
    }

    [RelayCommand]
    private void SaveDocument()
    {
        if (CurrentDocument == null) return;

        if (string.IsNullOrEmpty(CurrentFilePath))
        {
            // Use Documents folder with timestamp
            var fileName = $"{CurrentDocument.Title}_{DateTime.Now:yyyyMMdd_HHmmss}.xml";
            CurrentFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                fileName);
        }

        try
        {
            var task = _documentService.SaveDocumentAsync(CurrentDocument, CurrentFilePath);
            task.Wait(); // Block until complete
            HasUnsavedChanges = false;
            System.Diagnostics.Debug.WriteLine($"Document saved to: {CurrentFilePath}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Save error: {ex.Message}");
        }
    }

    [RelayCommand]
    private void OpenDocument(string? filePath = null)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return;
        }

        try
        {
            var task = _documentService.LoadDocumentAsync(filePath);
            CurrentDocument = task.Result;
            CurrentFilePath = filePath;
            LoadDocumentWidgets();
            HasUnsavedChanges = false;
            UpdatePreview();
            System.Diagnostics.Debug.WriteLine($"Document loaded from: {filePath}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Open error: {ex.Message}");
        }
    }

    [RelayCommand]
    private void AddWidget(WidgetDefinition? definition)
    {
        try
        {
            if (definition == null || CurrentDocument == null) return;

            var widget = new DocumentWidget
            {
                Id = Guid.NewGuid().ToString(),
                WidgetDefinitionId = definition.Id,
                Order = CurrentDocument.Widgets.Count,
                CreatedAt = DateTime.Now,
                ModifiedAt = DateTime.Now
            };

            // Initialize fields with default values
            if (definition.Fields != null)
            {
                foreach (var field in definition.Fields)
                {
                    widget.Fields.Add(new DocumentFieldValue
                    {
                        Name = field.Id,
                        Value = field.DefaultValue ?? string.Empty
                    });
                }
            }

            CurrentDocument.Widgets.Add(widget);
            var viewModel = new DocumentWidgetViewModel(widget, definition);
            DocumentWidgets.Add(viewModel);

            HasUnsavedChanges = true;
            UpdatePreview();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AddWidget error: {ex}");
        }
    }

    [RelayCommand]
    private void RemoveWidget(DocumentWidgetViewModel? widgetViewModel)
    {
        if (widgetViewModel == null || CurrentDocument == null) return;

        CurrentDocument.Widgets.Remove(widgetViewModel.Model);
        DocumentWidgets.Remove(widgetViewModel);

        // Reorder remaining widgets
        for (int i = 0; i < DocumentWidgets.Count; i++)
        {
            DocumentWidgets[i].Order = i;
            DocumentWidgets[i].Model.Order = i;
        }

        HasUnsavedChanges = true;
        UpdatePreview();
    }

    [RelayCommand]
    private void MoveWidgetUp(DocumentWidgetViewModel? widgetViewModel)
    {
        if (widgetViewModel == null || widgetViewModel.Order == 0) return;

        var index = DocumentWidgets.IndexOf(widgetViewModel);
        if (index > 0)
        {
            DocumentWidgets.Move(index, index - 1);
            ReorderWidgets();
        }
    }

    [RelayCommand]
    private void MoveWidgetDown(DocumentWidgetViewModel? widgetViewModel)
    {
        if (widgetViewModel == null) return;

        var index = DocumentWidgets.IndexOf(widgetViewModel);
        if (index < DocumentWidgets.Count - 1)
        {
            DocumentWidgets.Move(index, index + 1);
            ReorderWidgets();
        }
    }

    private void ReorderWidgets()
    {
        for (int i = 0; i < DocumentWidgets.Count; i++)
        {
            DocumentWidgets[i].Order = i;
            DocumentWidgets[i].Model.Order = i;
        }
        HasUnsavedChanges = true;
        UpdatePreview();
    }

    [RelayCommand]
    private void SwitchMode(UserMode mode)
    {
        CurrentMode = mode;
        
        // Notify UI of mode-based property changes
        OnPropertyChanged(nameof(IsBeginnerMode));
        OnPropertyChanged(nameof(IsIntermediateOrExpert));
        OnPropertyChanged(nameof(IsExpertMode));
        OnPropertyChanged(nameof(CanReorderWidgets));
        OnPropertyChanged(nameof(CanExportHtml));
        OnPropertyChanged(nameof(CanCreateWidgets));
        
        RefreshAvailableWidgets();
    }

    [RelayCommand]
    private void UpdatePreview()
    {
        if (CurrentDocument == null) 
        {
            PreviewHtml = "<html><body><p>No document</p></body></html>";
            return;
        }

        try
        {
            _ = Task.Run(async () =>
            {
                var html = await _renderingService.RenderDocumentAsync(CurrentDocument);
                PreviewHtml = html;
            });
        }
        catch
        {
            PreviewHtml = "<html><body><p>Preview rendering...</p></body></html>";
        }
    }
    
    [RelayCommand]
    private async Task CreateWidget()
    {
        var editorVm = new WidgetEditorViewModel();
        var window = new Views.WidgetEditorWindow
        {
            DataContext = editorVm
        };
        
        // ShowDialog requires a parent window - we'll use a workaround
        var app = Avalonia.Application.Current;
        if (app?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
            && desktop.MainWindow is not null)
        {
            await window.ShowDialog(desktop.MainWindow);
            
            if (editorVm.DialogResult && editorVm.CreatedWidget != null)
            {
                // Reload widgets to include the new one
                await _widgetLibrary.ReloadAsync();
                RefreshAvailableWidgets();
            }
        }
    }

    [RelayCommand]
    private async Task EditWidget(WidgetDefinition widget)
    {
        if (widget == null) return;

        var editorVm = WidgetEditorViewModel.FromDefinition(widget);
        var window = new Views.WidgetEditorWindow { DataContext = editorVm };

        var app = Avalonia.Application.Current;
        if (app?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
            && desktop.MainWindow is not null)
        {
            await window.ShowDialog(desktop.MainWindow);

            if (editorVm.DialogResult && editorVm.CreatedWidget != null)
            {
                await _widgetLibrary.ReloadAsync();
                RefreshAvailableWidgets();
            }
        }
    }

    [RelayCommand]
    private async Task DeleteWidget(WidgetDefinition widget)
    {
        if (widget == null) return;

        await _widgetLibrary.DeleteWidgetAsync(widget.Id);
        RefreshAvailableWidgets();
    }

    [RelayCommand]
    private async Task EditTemplate(WidgetDefinition widget)
    {
        if (widget == null) return;

        var editorVm = TemplateEditorViewModel.Create(widget, _widgetLibrary, 
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "templates"));
        var window = new Views.TemplateEditorWindow { DataContext = editorVm };

        var app = Avalonia.Application.Current;
        if (app?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
            && desktop.MainWindow is not null)
        {
            await window.ShowDialog(desktop.MainWindow);

            if (editorVm.DialogResult)
            {
                await _widgetLibrary.ReloadAsync();
                RefreshAvailableWidgets();
            }
        }
    }

    [RelayCommand]
    private async Task DeleteTemplate(WidgetDefinition widget)
    {
        if (widget == null) return;

        await _widgetLibrary.DeleteTemplateAsync(widget.Id);
        RefreshAvailableWidgets();
    }
    
    [RelayCommand]
    private async Task PreviewInBrowser()
    {
        if (CurrentDocument == null) return;
        
        try
        {
            // Make sure we have the latest preview
            await UpdatePreviewAsync();

            var tempPath = Path.Combine(Path.GetTempPath(), $"preview_{DateTime.Now:yyyyMMddHHmmss}.html");
            var html = await _renderingService.RenderDocumentAsync(CurrentDocument);
            await File.WriteAllTextAsync(tempPath, html);
            
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = tempPath,
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error opening preview: {ex.Message}");
        }
    }

    private async Task UpdatePreviewAsync()
    {
        if (CurrentDocument == null)
        {
            PreviewHtml = "<html><body><p>No document loaded</p></body></html>";
            return;
        }

        try
        {
            var html = await _renderingService.RenderDocumentAsync(CurrentDocument);
            PreviewHtml = html;
        }
        catch (Exception ex)
        {
            PreviewHtml = $"<html><body><p>Error rendering preview: {ex.Message}</p></body></html>";
        }
    }

    private static string EscapeHtml(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;");
    }

    [RelayCommand]
    private void ExportToHtml()
    {
        if (CurrentDocument == null) return;

        try
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"{CurrentDocument.Title.Replace(" ", "_")}_{timestamp}.html";
            var outputPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                fileName);

            var task = _renderingService.ExportToFileAsync(CurrentDocument, outputPath);
            task.Wait();
            
            System.Diagnostics.Debug.WriteLine($"Exported to: {outputPath}");
            
            // Open the file in default browser
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = outputPath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Export error: {ex.Message}");
        }
    }

    private void LoadDocumentWidgets()
    {
        DocumentWidgets.Clear();
        if (CurrentDocument == null) return;

        foreach (var widget in CurrentDocument.Widgets.OrderBy(w => w.Order))
        {
            var definition = _widgetLibrary.GetWidgetById(widget.WidgetDefinitionId);
            var viewModel = new DocumentWidgetViewModel(widget, definition);
            DocumentWidgets.Add(viewModel);
        }
    }

    partial void OnSelectedWidgetChanged(DocumentWidgetViewModel? value)
    {
        // Update property panel when selection changes
    }

    // ===== AI Features =====

    [ObservableProperty]
    private bool _isAiEnabled = false;

    [ObservableProperty]
    private string _aiStatus = "AI Disabled";

    [RelayCommand]
    private async Task SetupAIAsync()
    {
        try
        {
            // Check if Ollama is already setup
            if (_ollamaService == null)
            {
                _ollamaService = new OllamaService();
            }

            var isInstalled = await _ollamaService.IsOllamaInstalledAsync();
            var models = await _ollamaService.GetInstalledModelsAsync();

            if (!isInstalled || models.Count == 0)
            {
                // Show setup dialog - need to get parent window from somewhere
                // For now, create without parent
                var setupWindow = new Views.OllamaSetupWindow
                {
                    DataContext = new OllamaSetupViewModel()
                };

                // Note: Passing null for owner window - this is acceptable
                var result = await setupWindow.ShowDialog<bool?>(App.MainWindow);
                
                if (result == true)
                {
                    IsAiEnabled = true;
                    _aiEnabled = true;
                    AiStatus = "AI Ready ✅";
                }
            }
            else
            {
                // Already setup
                IsAiEnabled = true;
                _aiEnabled = true;
                AiStatus = "AI Ready ✅";
                _ollamaService.SetModel(models[0].Name);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AI Setup error: {ex.Message}");
            AiStatus = "AI Setup Failed ❌";
        }
    }

    [RelayCommand]
    private async Task ImproveWithAIAsync()
    {
        if (!_aiEnabled || _ollamaService == null || SelectedWidget == null)
            return;

        try
        {
            AiStatus = "Improving...";

            // Get the main content field
            var contentField = SelectedWidget.Definition?.Fields
                .FirstOrDefault(f => f.Type == FieldType.Multiline);

            if (contentField != null)
            {
                var original = SelectedWidget.GetFieldValue(contentField.Id);
                var improved = await _ollamaService.ImproveFormattingAsync(original, "widget content");
                SelectedWidget.SetFieldValue(contentField.Id, improved);
                
                UpdatePreview();
                HasUnsavedChanges = true;
            }

            AiStatus = "AI Ready ✅";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AI Improve error: {ex.Message}");
            AiStatus = "AI Error ❌";
        }
    }

    [RelayCommand]
    private async Task GetAISuggestionsAsync()
    {
        if (!_aiEnabled || _ollamaService == null || SelectedWidget == null)
            return;

        try
        {
            AiStatus = "Getting suggestions...";

            var contentField = SelectedWidget.Definition?.Fields
                .FirstOrDefault(f => f.Type == FieldType.Multiline);

            if (contentField != null)
            {
                var content = SelectedWidget.GetFieldValue(contentField.Id);
                var suggestions = await _ollamaService.GetSuggestionsAsync(content);
                
                // TODO: Show suggestions in a dialog
                System.Diagnostics.Debug.WriteLine($"AI Suggestions:\n{suggestions}");
            }

            AiStatus = "AI Ready ✅";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AI Suggestions error: {ex.Message}");
            AiStatus = "AI Error ❌";
        }
    }
}
