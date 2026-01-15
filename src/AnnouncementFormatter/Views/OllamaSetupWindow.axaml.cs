using Avalonia.Controls;

namespace AnnouncementFormatter.Views;

public partial class OllamaSetupWindow : Window
{
    public OllamaSetupWindow()
    {
        InitializeComponent();
    }

    private void OnCancelClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close(false);
    }
}
