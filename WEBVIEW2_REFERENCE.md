# WebView2 HTML Preview Component (WinForms/WPF Reference)

> **Note**: This is a reference guide for implementing HTML preview with WebView2 in WinForms or WPF applications. The current project uses Avalonia with a custom HTML-to-Avalonia converter instead.

---

## When to Use WebView2

WebView2 (Microsoft Edge Chromium) is ideal for:
- **WinForms** applications
- **WPF** applications  
- Applications requiring full **JavaScript** support
- Complex HTML with **CSS** (including external stylesheets)
- Applications needing **JavaScript interop** (C# ↔ JS)

---

## Installation

### NuGet Package

```bash
dotnet add package Microsoft.Web.WebView2
```

### Verify Package

```xml
<!-- In your .csproj file -->
<ItemGroup>
    <PackageReference Include="Microsoft.Web.WebView2" Version="1.0.2210.55" />
</ItemGroup>
```

---

## WinForms Implementation

### XAML-less Setup (C# Only)

```csharp
using Microsoft.Web.WebView2.WinForms;
using System.Windows.Forms;

public class HtmlPreviewControl : UserControl
{
    private WebView2 _webView;
    private string _currentHtml = string.Empty;
    private const int DebounceDelayMs = 150;
    private System.Threading.Timer? _debounceTimer;

    public HtmlPreviewControl()
    {
        InitializeWebView();
    }

    /// <summary>
    /// Initialize WebView2 asynchronously
    /// CRITICAL: WebView2 requires async initialization before use
    /// </summary>
    private async void InitializeWebView()
    {
        _webView = new WebView2
        {
            Dock = DockStyle.Fill,
            CreationProperties = new CoreWebView2CreationProperties
            {
                BrowserExecutableFolder = null,  // Use default Edge installation
                UserDataFolder = null,           // Use default temp folder
            }
        };

        Controls.Add(_webView);

        try
        {
            // REQUIRED: Initialize WebView2 core before using NavigateToString
            await _webView.EnsureCoreWebView2Async(null);
            
            // Disable unnecessary features
            _webView.CoreWebView2.Settings.AreDevToolsEnabled = false;
            _webView.CoreWebView2.Settings.IsScriptEnabled = false;  // Optional
            _webView.CoreWebView2.Settings.IsWebMessageSecurityEnabled = true;
            
            // Disable context menu
            _webView.CoreWebView2.ContextMenuRequested += (s, e) =>
            {
                e.Handled = true;
            };
        }
        catch (Exception ex)
        {
            MessageBox.Show($"WebView2 initialization failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Render HTML string with debouncing to prevent excessive renders
    /// </summary>
    public void RenderHtmlAsync(string html)
    {
        _currentHtml = html;
        
        // Cancel previous debounce timer
        _debounceTimer?.Dispose();
        
        // Schedule render after delay
        _debounceTimer = new System.Threading.Timer(
            _ => RenderHtmlInternal(html),
            null,
            DebounceDelayMs,
            System.Threading.Timeout.Infinite
        );
    }

    /// <summary>
    /// Synchronous render without debounce
    /// </summary>
    public void RenderHtml(string html)
    {
        _debounceTimer?.Dispose();
        RenderHtmlInternal(html);
    }

    /// <summary>
    /// Internal render method - uses NavigateToString()
    /// </summary>
    private void RenderHtmlInternal(string html)
    {
        try
        {
            if (_webView?.CoreWebView2 == null)
            {
                System.Diagnostics.Debug.WriteLine("WebView2 not initialized yet");
                return;
            }

            _currentHtml = html;

            // CRITICAL: NavigateToString() loads HTML from string
            // Supports: HTML, CSS (inline + <style>), JavaScript
            _webView.CoreWebView2.NavigateToString(html);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Render error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get currently rendered HTML
    /// </summary>
    public string CurrentHtml => _currentHtml;

    /// <summary>
    /// Clean up resources
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _debounceTimer?.Dispose();
            _webView?.Dispose();
        }
        base.Dispose(disposing);
    }
}
```

### Usage in WinForms

```csharp
public partial class MainForm : Form
{
    private HtmlPreviewControl _htmlPreview;

    public MainForm()
    {
        InitializeComponent();
        
        _htmlPreview = new HtmlPreviewControl
        {
            Dock = DockStyle.Fill,
            Parent = this
        };
    }

    private void UpdatePreview(string html)
    {
        // Async with debouncing (recommended for user input)
        _htmlPreview.RenderHtmlAsync(html);
        
        // Or synchronous (for direct updates)
        // _htmlPreview.RenderHtml(html);
    }
}
```

---

## WPF Implementation

### XAML Definition

```xml
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:wv2="clr-namespace:Microsoft.Web.WebView2.Wpf;assembly=Microsoft.Web.WebView2.Wpf"
        Title="HTML Preview" Height="400" Width="600">
    <Grid>
        <!-- WebView2 control -->
        <wv2:WebView2 x:Name="WebViewControl" />
    </Grid>
</Window>
```

### Code-Behind

```csharp
using Microsoft.Web.WebView2.Wpf;
using System.Windows;

public partial class PreviewWindow : Window
{
    private string _currentHtml = string.Empty;
    private const int DebounceDelayMs = 150;
    private System.Timers.Timer? _debounceTimer;

    public PreviewWindow()
    {
        InitializeComponent();
        InitializeWebView();
    }

    /// <summary>
    /// Initialize WebView2 asynchronously
    /// </summary>
    private async void InitializeWebView()
    {
        try
        {
            // REQUIRED: Initialize core before use
            await WebViewControl.EnsureCoreWebView2Async(null);
            
            // Disable dev tools and context menu
            WebViewControl.CoreWebView2.Settings.AreDevToolsEnabled = false;
            WebViewControl.CoreWebView2.ContextMenuRequested += (s, e) =>
            {
                e.Handled = true;
            };
        }
        catch (Exception ex)
        {
            MessageBox.Show($"WebView2 initialization failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Render HTML with debouncing
    /// </summary>
    public void RenderHtmlAsync(string html)
    {
        _currentHtml = html;
        
        _debounceTimer?.Stop();
        _debounceTimer?.Dispose();
        
        _debounceTimer = new System.Timers.Timer(DebounceDelayMs)
        {
            AutoReset = false
        };
        _debounceTimer.Elapsed += (s, e) =>
        {
            Dispatcher.Invoke(() => RenderHtmlInternal(html));
        };
        _debounceTimer.Start();
    }

    /// <summary>
    /// Synchronous render
    /// </summary>
    public void RenderHtml(string html)
    {
        _debounceTimer?.Stop();
        RenderHtmlInternal(html);
    }

    /// <summary>
    /// Internal render - NavigateToString()
    /// </summary>
    private void RenderHtmlInternal(string html)
    {
        try
        {
            if (WebViewControl?.CoreWebView2 == null)
            {
                System.Diagnostics.Debug.WriteLine("WebView2 not ready");
                return;
            }

            // CRITICAL: NavigateToString loads HTML from string
            WebViewControl.CoreWebView2.NavigateToString(html);
            _currentHtml = html;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Render error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get current HTML
    /// </summary>
    public string CurrentHtml => _currentHtml;

    protected override void OnClosed(EventArgs e)
    {
        _debounceTimer?.Dispose();
        WebViewControl?.Dispose();
        base.OnClosed(e);
    }
}
```

---

## Complete HTML Example

Here's example HTML that works with WebView2:

```csharp
string html = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {
            font-family: Arial, sans-serif;
            margin: 0;
            background-color: #f5f5f5;
        }
        .container {
            background: white;
            padding: 20px;
            margin: 10px;
            border-left: 4px solid #ba0c2f;
            border-radius: 4px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }
        .header {
            color: #ba0c2f;
            font-size: 18px;
            font-weight: bold;
            margin-bottom: 10px;
        }
        .content {
            color: #333;
            font-size: 14px;
            line-height: 1.5;
        }
        strong {
            color: #333;
            font-weight: bold;
        }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">Announcement Title</div>
        <div class=""content"">
            <p>This is a <strong>sample announcement</strong> rendered in WebView2.</p>
            <p>HTML + CSS + JavaScript all supported.</p>
        </div>
    </div>
</body>
</html>
";

previewControl.RenderHtml(html);
```

---

## Key Concepts

### EnsureCoreWebView2Async()

```csharp
// CRITICAL: Must call before NavigateToString()
await WebViewControl.EnsureCoreWebView2Async(null);

// Returns CoreWebView2 handle when ready
// Parameters:
// - userDataFolder: null = use default temp folder
// - browserExecutableFolder: null = use default Edge installation
```

**Why it's needed**:
- WebView2 initializes asynchronously
- Core must load before any navigation
- Calling NavigateToString() without this will fail silently

### NavigateToString()

```csharp
// Load HTML from string (NO external URLs)
WebViewControl.CoreWebView2.NavigateToString(html);

// Supports:
// ✅ HTML (any valid HTML5)
// ✅ Inline CSS <style> tags
// ✅ Inline style="" attributes
// ✅ JavaScript <script> tags
// ✅ Data URIs

// Does NOT load:
// ❌ External URLs (http://, https://)
// ❌ Local files via file:// (blocked by default)
```

### Settings

```csharp
// Disable developer tools
WebViewControl.CoreWebView2.Settings.AreDevToolsEnabled = false;

// Disable JavaScript (if only HTML/CSS needed)
WebViewControl.CoreWebView2.Settings.IsScriptEnabled = false;

// Disable context menu
WebViewControl.CoreWebView2.ContextMenuRequested += (s, e) =>
{
    e.Handled = true;
};

// Other useful settings
Settings.AreHostObjectsAllowed = false;  // Block WinRT objects
Settings.IsWebMessageSecurityEnabled = true;
Settings.IsBuiltInErrorPageEnabled = true;
```

---

## Performance & Optimization

### Memory Management

```csharp
// Each WebView2 instance uses ~50-100MB RAM
// Initialize once, reuse for multiple renders

// ✅ GOOD: Reuse single control
private WebView2 _webView;  // Initialize once

public void RenderHtml(string html)
{
    _webView.CoreWebView2.NavigateToString(html);
}

// ❌ BAD: Create new control each time
public void RenderHtml(string html)
{
    var webView = new WebView2();  // Memory leak!
    webView.EnsureCoreWebView2Async();
    webView.CoreWebView2.NavigateToString(html);
}
```

### Debouncing Example

```csharp
private System.Timers.Timer? _debounceTimer;

public void RenderHtmlAsync(string html)
{
    // Cancel previous render
    _debounceTimer?.Stop();
    
    // Schedule new render in 150ms
    _debounceTimer = new System.Timers.Timer(150)
    {
        AutoReset = false
    };
    _debounceTimer.Elapsed += (s, e) =>
    {
        // Render on UI thread
        Dispatcher.Invoke(() =>
            WebViewControl.CoreWebView2.NavigateToString(html)
        );
    };
    _debounceTimer.Start();
}
```

**Result**: 10 updates/sec → 1 render/sec (90% reduction)

---

## Error Handling

```csharp
try
{
    // Attempt to render
    if (WebViewControl?.CoreWebView2 == null)
    {
        throw new InvalidOperationException("WebView2 not initialized");
    }
    
    WebViewControl.CoreWebView2.NavigateToString(html);
}
catch (ArgumentException ex) when (ex.Message.Contains("invalid"))
{
    // HTML parsing failed
    System.Diagnostics.Debug.WriteLine($"Invalid HTML: {ex.Message}");
}
catch (Exception ex)
{
    // Other errors
    System.Diagnostics.Debug.WriteLine($"Render failed: {ex.Message}");
    
    // Show error state
    string errorHtml = $"<html><body><p style='color:red;'>Error: {ex.Message}</p></body></html>";
    WebViewControl.CoreWebView2.NavigateToString(errorHtml);
}
```

---

## Comparison: Avalonia vs WebView2

| Feature | Avalonia Control | WebView2 |
|---------|-----------------|---------|
| **Integration** | Native Avalonia | Embedded browser |
| **Performance** | Fast (native render) | Slower (Chromium) |
| **Memory** | ~5-10MB | ~50-100MB |
| **JavaScript** | ❌ Not supported | ✅ Full support |
| **External CSS** | ⚠️ Limited | ✅ Full support |
| **Initialization** | Immediate | Async required |
| **Use Case** | Simple HTML+CSS | Complex HTML/JS |
| **Offline** | ✅ Yes | ✅ Yes |
| **Platform** | Avalonia apps | WinForms/WPF |

---

## Checklist for WebView2 Implementation

- [ ] Added `Microsoft.Web.WebView2` NuGet package
- [ ] Called `EnsureCoreWebView2Async()` before any NavigateToString()
- [ ] Added error handling for initialization
- [ ] Disabled dev tools and context menu
- [ ] Implemented debouncing for performance
- [ ] Tested with sample HTML
- [ ] Verified offline operation (no external URLs)
- [ ] Added proper disposal in Dispose() method
- [ ] Tested memory usage after multiple renders

---

## References

- [Microsoft.Web.WebView2 Documentation](https://learn.microsoft.com/en-us/microsoft-edge/webview2/)
- [WebView2 API Reference](https://learn.microsoft.com/en-us/dotnet/api/microsoft.web.webview2.core)
- [WebView2 Best Practices](https://learn.microsoft.com/en-us/microsoft-edge/webview2/concepts/best-practices)

---

## Summary

WebView2 provides a full browser engine for rendering complex HTML/CSS/JavaScript in WinForms and WPF applications:

✅ **Initialize**: `EnsureCoreWebView2Async()`  
✅ **Render**: `NavigateToString(html)`  
✅ **Optimize**: Debounce rapid updates  
✅ **Secure**: Disable dev tools and scripts  
✅ **Error**: Catch and display gracefully  

For **Avalonia applications**, use the custom HtmlToAvaloniaConverter component included in this project instead.
