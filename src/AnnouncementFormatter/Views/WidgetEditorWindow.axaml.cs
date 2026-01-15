using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using AnnouncementFormatter.ViewModels;
using System.ComponentModel;

namespace AnnouncementFormatter.Views;

public partial class WidgetEditorWindow : Window
{
    public WidgetEditorWindow()
    {
        InitializeComponent();
        
        Loaded += (s, e) =>
        {
            if (DataContext is WidgetEditorViewModel vm)
            {
                vm.PropertyChanged += OnViewModelPropertyChanged;
                UpdateHtmlPreview();
            }
        };
    }
    
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(WidgetEditorViewModel.PreviewHtml))
        {
            UpdateHtmlPreview();
        }
    }
    
    private void UpdateHtmlPreview()
    {
        if (DataContext is not WidgetEditorViewModel vm) return;
        
        var container = this.FindControl<Border>("HtmlPreviewContainer");
        if (container == null) return;
        
        // Parse and render basic HTML as styled controls
        var html = vm.PreviewHtml;
        
        // For now, show the HTML with basic rendering
        // A full HTML renderer would require WebView2 or similar
        var textBlock = new TextBlock
        {
            Text = html,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 11,
            Foreground = Brushes.Black
        };
        
        container.Child = textBlock;
    }
    
    private async void OnSaveClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is WidgetEditorViewModel vm)
        {
            await vm.SaveCommand.ExecuteAsync(null);
            if (vm.DialogResult)
            {
                Close();
            }
        }
    }
    
    private void OnCancelClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is WidgetEditorViewModel vm)
        {
            vm.CancelCommand.Execute(null);
        }
        Close();
    }
}
