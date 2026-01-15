using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Media;
using Avalonia;
using AnnouncementFormatter.Core.Models;
using AnnouncementFormatter.ViewModels;
using AnnouncementFormatter.Controls;
using System.Text.RegularExpressions;

namespace AnnouncementFormatter.Views;

public partial class MainWindow : Window
{
    private HtmlPreviewControl? _htmlPreview;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnWindowLoaded;
    }
    
    private void OnWindowLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Set up drop handler for document canvas
        var canvas = this.FindControl<ScrollViewer>("DocumentCanvas");
        if (canvas != null)
        {
            DragDrop.SetAllowDrop(canvas, true);
            canvas.AddHandler(DragDrop.DropEvent, OnCanvasDrop);
            canvas.AddHandler(DragDrop.DragOverEvent, OnCanvasDragOver);
        }

        // Set up HTML preview control and bind to preview updates
        _htmlPreview = this.FindControl<HtmlPreviewControl>("HtmlPreview");
        if (_htmlPreview != null && DataContext is MainWindowViewModel viewModel)
        {
            viewModel.PropertyChanged += async (s, e) =>
            {
                if (e.PropertyName == nameof(MainWindowViewModel.PreviewHtml))
                {
                    await UpdateHtmlPreviewAsync(viewModel.PreviewHtml);
                }
            };
            
            // Initial content
            _ = UpdateHtmlPreviewAsync(viewModel.PreviewHtml);
        }
    }

    /// <summary>
    /// Updates the HTML preview with debouncing for performance
    /// </summary>
    private async Task UpdateHtmlPreviewAsync(string htmlContent)
    {
        if (_htmlPreview == null) return;

        try
        {
            await _htmlPreview.RenderHtmlAsync(htmlContent);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"HTML preview error: {ex.Message}");
        }
    }

    private void OnWidgetClicked(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border border && 
            border.DataContext is WidgetDefinition widget &&
            DataContext is MainWindowViewModel viewModel)
        {
            // Start drag operation
            var data = new DataObject();
            data.Set("WidgetDefinition", widget);
            _ = DragDrop.DoDragDrop(e, data, DragDropEffects.Copy);
        }
    }
    
    private void OnWidgetDoubleClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Double-click adds widget directly (legacy behavior)
        if (sender is Border border && 
            border.DataContext is WidgetDefinition widget &&
            DataContext is MainWindowViewModel viewModel)
        {
            viewModel.AddWidgetCommand.Execute(widget);
        }
    }

    private void OnOpenDocumentClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            if (DataContext is not MainWindowViewModel viewModel)
                return;

            // For now, open the sample document from the examples folder
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var samplePath = Path.Combine(baseDir, "Assets", "examples", "sample_document.xml");
            
            if (File.Exists(samplePath))
            {
                viewModel.OpenDocumentCommand.Execute(samplePath);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Sample document not found");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error opening file: {ex.Message}");
        }
    }

    private void OnBeginnerModeClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.SwitchModeCommand.Execute(UserMode.Beginner);
        }
    }

    private void OnIntermediateModeClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.SwitchModeCommand.Execute(UserMode.Intermediate);
        }
    }

    private void OnExpertModeClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.SwitchModeCommand.Execute(UserMode.Expert);
        }
    }
    
    private void OnCanvasDragOver(object? sender, DragEventArgs e)
    {
        // Accept widget drops
        if (e.Data.Contains("WidgetDefinition"))
        {
            e.DragEffects = DragDropEffects.Copy;
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }
    }
    
    private void OnCanvasDrop(object? sender, DragEventArgs e)
    {
        if (e.Data.Get("WidgetDefinition") is WidgetDefinition widget &&
            DataContext is MainWindowViewModel viewModel)
        {
            viewModel.AddWidgetCommand.Execute(widget);
        }
    }
    
    private void OnFieldValueChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.UpdatePreviewCommand.Execute(null);
        }
    }
}
