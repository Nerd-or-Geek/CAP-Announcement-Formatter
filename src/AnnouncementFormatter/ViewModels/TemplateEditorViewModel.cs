using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AnnouncementFormatter.Core.Models;
using AnnouncementFormatter.Core.Services;

namespace AnnouncementFormatter.ViewModels;

/// <summary>
/// ViewModel for editing widget templates (HTML).
/// </summary>
public partial class TemplateEditorViewModel : ViewModelBase
{
    private readonly WidgetLibraryService _widgetLibrary;
    private readonly string _templateDirectory;

    [ObservableProperty]
    private WidgetDefinition? _widget;

    [ObservableProperty]
    private string _templateContent = string.Empty;

    [ObservableProperty]
    private string _previewHtml = string.Empty;

    [ObservableProperty]
    private bool _dialogResult;

    public TemplateEditorViewModel()
    {
        // Default constructor for XAML
        _widgetLibrary = null!;
        _templateDirectory = string.Empty;
    }

    public TemplateEditorViewModel(WidgetLibraryService widgetLibrary, string templateDirectory)
    {
        _widgetLibrary = widgetLibrary;
        _templateDirectory = templateDirectory;
    }

    partial void OnTemplateContentChanged(string value)
    {
        // Auto-update preview when content changes
        UpdatePreview();
    }

    public static TemplateEditorViewModel Create(WidgetDefinition widget, WidgetLibraryService widgetLibrary, string templateDirectory)
    {
        var vm = new TemplateEditorViewModel(widgetLibrary, templateDirectory)
        {
            Widget = widget
        };
        return vm;
    }

    public async Task LoadTemplateAsync()
    {
        if (Widget == null) return;

        try
        {
            TemplateContent = await _widgetLibrary.GetTemplateContentAsync(Widget.Id);
            UpdatePreview();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading template: {ex.Message}");
        }
    }

    [RelayCommand]
    private void UpdatePreview()
    {
        PreviewHtml = TemplateContent;
    }

    [RelayCommand]
    private async Task SaveTemplate()
    {
        if (Widget == null) return;

        try
        {
            await _widgetLibrary.SaveTemplateAsync(Widget.Id, TemplateContent);
            DialogResult = true;
            System.Diagnostics.Debug.WriteLine($"Template saved for widget: {Widget.Id}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving template: {ex.Message}");
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogResult = false;
    }
}
