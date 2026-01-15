using Avalonia.Controls;
using Avalonia.Media;
using Avalonia;
using AnnouncementFormatter.ViewModels;
using AnnouncementFormatter.Controls;
using System.Text.RegularExpressions;

namespace AnnouncementFormatter.Views;

public partial class TemplateEditorWindow : Window
{
    private HtmlPreviewControl? _htmlPreview;

    public TemplateEditorWindow()
    {
        InitializeComponent();
        Loaded += OnWindowLoaded;
    }

    private void OnWindowLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _htmlPreview = this.FindControl<HtmlPreviewControl>("HtmlPreview");
        
        if (DataContext is TemplateEditorViewModel viewModel)
        {
            // Load template on window open
            _ = viewModel.LoadTemplateAsync();

            // Subscribe to preview changes with async rendering
            viewModel.PropertyChanged += async (s, e) =>
            {
                if (e.PropertyName == nameof(TemplateEditorViewModel.PreviewHtml))
                {
                    await UpdatePreviewAsync(viewModel.PreviewHtml);
                }
            };

            // Check if dialog should close
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(TemplateEditorViewModel.DialogResult))
                {
                    Close(viewModel.DialogResult);
                }
            };
        }
    }

    /// <summary>
    /// Updates preview using HtmlPreviewControl with automatic debouncing
    /// </summary>
    private async Task UpdatePreviewAsync(string htmlContent)
    {
        if (_htmlPreview == null) return;

        try
        {
            await _htmlPreview.RenderHtmlAsync(htmlContent);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Preview error: {ex.Message}");
        }
    }

    private void OnCancelClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close(false);
    }
}
