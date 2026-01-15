using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using AnnouncementFormatter.Views;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AnnouncementFormatter.Controls;

/// <summary>
/// Reusable HTML preview control that renders HTML with inline CSS
/// Uses HtmlToAvaloniaConverter to render HTML as native Avalonia controls
/// </summary>
public partial class HtmlPreviewControl : UserControl
{
    private readonly Border? _contentContainer;
    private CancellationTokenSource? _renderCancellation;
    private string _currentHtml = string.Empty;
    
    // Debounce delay to prevent excessive rendering during rapid updates
    private const int DebounceDelayMs = 150;

    public HtmlPreviewControl()
    {
        InitializeComponent();
        _contentContainer = this.FindControl<Border>("ContentContainer");
    }

    /// <summary>
    /// Renders HTML string with inline CSS as Avalonia controls
    /// Debounced to prevent excessive rendering during rapid typing
    /// </summary>
    /// <param name="html">HTML string with inline styles and CSS</param>
    public async Task RenderHtmlAsync(string html)
    {
        if (_contentContainer == null) return;
        
        // Cancel any pending render operation
        _renderCancellation?.Cancel();
        _renderCancellation = new CancellationTokenSource();
        var token = _renderCancellation.Token;
        
        _currentHtml = html;
        
        try
        {
            // Debounce: wait before rendering to batch rapid updates
            await Task.Delay(DebounceDelayMs, token);
            
            if (token.IsCancellationRequested) return;
            
            // Render on UI thread
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (token.IsCancellationRequested) return;
                
                try
                {
                    // Convert HTML to Avalonia visual tree
                    var content = HtmlToAvaloniaConverter.ConvertHtmlToVisual(html);
                    _contentContainer.Child = content;
                }
                catch (Exception ex)
                {
                    // Show error message in preview
                    _contentContainer.Child = new TextBlock
                    {
                        Text = $"Preview Error: {ex.Message}",
                        Foreground = Avalonia.Media.Brushes.Red,
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap
                    };
                }
            }, DispatcherPriority.Background);
        }
        catch (TaskCanceledException)
        {
            // Ignore cancellation - expected during rapid updates
        }
    }
    
    /// <summary>
    /// Synchronous render for immediate updates (no debounce)
    /// Use sparingly - prefer RenderHtmlAsync for user input scenarios
    /// </summary>
    public void RenderHtml(string html)
    {
        if (_contentContainer == null) return;
        
        _currentHtml = html;
        _renderCancellation?.Cancel();
        
        try
        {
            var content = HtmlToAvaloniaConverter.ConvertHtmlToVisual(html);
            _contentContainer.Child = content;
        }
        catch (Exception ex)
        {
            _contentContainer.Child = new TextBlock
            {
                Text = $"Preview Error: {ex.Message}",
                Foreground = Avalonia.Media.Brushes.Red,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            };
        }
    }
    
    /// <summary>
    /// Clears the preview content
    /// </summary>
    public void Clear()
    {
        _renderCancellation?.Cancel();
        _currentHtml = string.Empty;
        
        if (_contentContainer != null)
        {
            _contentContainer.Child = null;
        }
    }
    
    /// <summary>
    /// Gets the currently rendered HTML
    /// </summary>
    public string CurrentHtml => _currentHtml;

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
