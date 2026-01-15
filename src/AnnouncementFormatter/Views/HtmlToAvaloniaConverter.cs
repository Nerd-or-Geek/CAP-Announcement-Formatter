using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AnnouncementFormatter.Views;

/// <summary>
/// Converts basic HTML to Avalonia visual controls with style parsing.
/// </summary>
public class HtmlToAvaloniaConverter
{
    public static Control ConvertHtmlToVisual(string htmlContent)
    {
        if (string.IsNullOrEmpty(htmlContent))
        {
            return new TextBlock { 
                Text = "No content", 
                Foreground = new SolidColorBrush(Colors.Gray),
                Margin = new Thickness(10)
            };
        }

        var mainPanel = new StackPanel { Spacing = 0 };

        try
        {
            // Replace template placeholders
            var html = htmlContent;
            html = Regex.Replace(html, @"\{\{[^}]*\}\}", "Sample Text", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"\{\{#if[^}]*\}\}.*?\{\{\/if\}\}", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // Parse main containers (divs, sections)
            var divMatches = Regex.Matches(html, @"<div[^>]*>(.*?)<\/div>", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (divMatches.Count > 0)
            {
                foreach (Match divMatch in divMatches)
                {
                    var divContent = divMatch.Groups[1].Value;
                    var divTag = divMatch.Value.Substring(0, divMatch.Value.IndexOf('>'));
                    
                    var border = ParseDivWithStyles(divContent, divTag);
                    mainPanel.Children.Add(border);
                }
            }
            else
            {
                // Fallback: parse paragraphs and text
                var elements = ParseSimpleHtml(html);
                foreach (var element in elements)
                {
                    mainPanel.Children.Add(element);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"HTML conversion error: {ex.Message}");
            mainPanel.Children.Add(new TextBlock { 
                Text = "Render error: " + ex.Message, 
                Foreground = new SolidColorBrush(Colors.Red),
                Margin = new Thickness(10),
                TextWrapping = TextWrapping.Wrap
            });
        }

        return mainPanel;
    }

    private static Border ParseDivWithStyles(string divContent, string divTag)
    {
        var border = new Border
        {
            Padding = new Thickness(16),
            Margin = new Thickness(8)
        };

        // Extract style attribute
        var styleMatch = Regex.Match(divTag, @"style=""([^""]*)""", RegexOptions.IgnoreCase);
        if (styleMatch.Success)
        {
            var style = styleMatch.Groups[1].Value;
            ApplyStyles(border, style);
        }

        // Parse inner content (headers, text, sub-divs)
        var innerStack = new StackPanel { Spacing = 8 };

        // Parse headers
        var headerMatches = Regex.Matches(divContent, @"<h[1-6][^>]*>(.*?)<\/h[1-6]>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        foreach (Match headerMatch in headerMatches)
        {
            var headerText = StripHtmlTags(headerMatch.Groups[1].Value);
            var headerBlock = new TextBlock
            {
                Text = System.Net.WebUtility.HtmlDecode(headerText),
                FontSize = 14,
                FontWeight = FontWeight.Bold,
                Foreground = new SolidColorBrush(Color.Parse("#BA0C2F")),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 8)
            };
            innerStack.Children.Add(headerBlock);
        }

        // Parse strong/bold
        var strongMatches = Regex.Matches(divContent, @"<strong[^>]*>(.*?)<\/strong>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        foreach (Match strongMatch in strongMatches)
        {
            var strongText = StripHtmlTags(strongMatch.Groups[1].Value);
            var strongBlock = new TextBlock
            {
                Text = System.Net.WebUtility.HtmlDecode(strongText),
                FontWeight = FontWeight.Bold,
                Foreground = new SolidColorBrush(Colors.Black),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 4, 0, 4)
            };
            innerStack.Children.Add(strongBlock);
        }

        // Parse regular text (remove all tags)
        var plainText = StripHtmlTags(divContent);
        plainText = System.Net.WebUtility.HtmlDecode(plainText).Trim();
        
        // Clean up placeholder text
        plainText = Regex.Replace(plainText, @"Sample Text\s*", "");

        if (!string.IsNullOrWhiteSpace(plainText) && plainText.Length > 2)
        {
            var textBlock = new TextBlock
            {
                Text = plainText,
                Foreground = new SolidColorBrush(Colors.Black),
                TextWrapping = TextWrapping.Wrap,
                FontSize = 11,
                LineHeight = 1.4
            };
            innerStack.Children.Add(textBlock);
        }

        border.Child = innerStack;
        return border;
    }

    private static void ApplyStyles(Border border, string styleString)
    {
        try
        {
            // Parse background color
            var bgMatch = Regex.Match(styleString, @"background(?:-color)?:\s*([^;]+)", RegexOptions.IgnoreCase);
            if (bgMatch.Success)
            {
                var bgValue = bgMatch.Groups[1].Value.Trim();
                try
                {
                    if (bgValue.Contains("rgba"))
                    {
                        // Handle rgba - use simplified color
                        if (bgValue.Contains("186") || bgValue.Contains("BA0C2F"))
                            border.Background = new SolidColorBrush(Color.Parse("#FFF3CD"));
                        else if (bgValue.Contains("255") && bgValue.Contains("249"))
                            border.Background = new SolidColorBrush(Color.Parse("#FFF9E6"));
                        else
                            border.Background = new SolidColorBrush(Colors.White);
                    }
                    else
                    {
                        border.Background = new SolidColorBrush(Color.Parse(bgValue));
                    }
                }
                catch
                {
                    border.Background = new SolidColorBrush(Colors.White);
                }
            }
            else
            {
                border.Background = new SolidColorBrush(Colors.White);
            }

            // Parse border color
            var borderMatch = Regex.Match(styleString, @"border(?:-left)?:\s*([^;]+)", RegexOptions.IgnoreCase);
            if (borderMatch.Success)
            {
                var borderValue = borderMatch.Groups[1].Value;
                try
                {
                    var colorMatch = Regex.Match(borderValue, @"([#\w]+)");
                    if (colorMatch.Success)
                    {
                        border.BorderBrush = new SolidColorBrush(Color.Parse(colorMatch.Groups[1].Value));
                    }

                    if (borderValue.Contains("left"))
                    {
                        border.BorderThickness = new Thickness(3, 0, 0, 0);
                    }
                    else
                    {
                        var thicknessMatch = Regex.Match(borderValue, @"(\d+)px");
                        if (thicknessMatch.Success && int.TryParse(thicknessMatch.Groups[1].Value, out var thickness))
                        {
                            border.BorderThickness = new Thickness(thickness);
                        }
                    }
                }
                catch
                {
                    border.BorderThickness = new Thickness(1);
                }
            }

            // Parse padding
            var paddingMatch = Regex.Match(styleString, @"padding:\s*(\d+)px", RegexOptions.IgnoreCase);
            if (paddingMatch.Success && int.TryParse(paddingMatch.Groups[1].Value, out var padding))
            {
                border.Padding = new Thickness(padding);
            }

            // Parse margin
            var marginMatch = Regex.Match(styleString, @"margin:\s*(\d+)px\s+(\d+)px", RegexOptions.IgnoreCase);
            if (marginMatch.Success)
            {
                if (int.TryParse(marginMatch.Groups[1].Value, out var mTop) && int.TryParse(marginMatch.Groups[2].Value, out var mRight))
                {
                    border.Margin = new Thickness(mRight, mTop, mRight, mTop);
                }
            }
            else
            {
                border.Margin = new Thickness(8);
            }

            // Parse border-radius
            var radiusMatch = Regex.Match(styleString, @"border-radius:\s*(\d+)px", RegexOptions.IgnoreCase);
            if (radiusMatch.Success && int.TryParse(radiusMatch.Groups[1].Value, out var radius))
            {
                border.CornerRadius = new CornerRadius(radius);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Style parsing error: {ex.Message}");
        }
    }

    private static List<Control> ParseSimpleHtml(string html)
    {
        var controls = new List<Control>();

        var lines = Regex.Split(html, @"<br\s*/?>");
        foreach (var line in lines)
        {
            var cleanLine = StripHtmlTags(line).Trim();
            cleanLine = System.Net.WebUtility.HtmlDecode(cleanLine);

            if (!string.IsNullOrWhiteSpace(cleanLine) && cleanLine.Length > 1)
            {
                controls.Add(new TextBlock
                {
                    Text = cleanLine,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = new SolidColorBrush(Colors.Black),
                    Margin = new Thickness(10, 4, 10, 4),
                    FontSize = 11
                });
            }
        }

        return controls;
    }

    private static string StripHtmlTags(string html)
    {
        return Regex.Replace(html, @"<[^>]+>", "");
    }
}
