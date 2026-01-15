using CommunityToolkit.Mvvm.ComponentModel;
using AnnouncementFormatter.Core.Models;

namespace AnnouncementFormatter.ViewModels;

/// <summary>
/// ViewModel for a widget instance in the document.
/// </summary>
public partial class DocumentWidgetViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _id = string.Empty;

    [ObservableProperty]
    private string _widgetDefinitionId = string.Empty;

    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private int _order;

    [ObservableProperty]
    private bool _isSelected;

    public DocumentWidget Model { get; }
    public WidgetDefinition? Definition { get; set; }

    public DocumentWidgetViewModel(DocumentWidget model, WidgetDefinition? definition)
    {
        try
        {
            Model = model;
            Definition = definition;
            Id = model.Id;
            WidgetDefinitionId = model.WidgetDefinitionId;
            DisplayName = definition?.DisplayName ?? "Unknown Widget";
            Order = model.Order;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DocumentWidgetViewModel error: {ex}");
            throw;
        }
    }

    public string GetFieldValue(string fieldName)
    {
        return Model.Fields.FirstOrDefault(f => f.Name == fieldName)?.Value ?? string.Empty;
    }

    public void SetFieldValue(string fieldName, string value)
    {
        var field = Model.Fields.FirstOrDefault(f => f.Name == fieldName);
        if (field != null)
        {
            field.Value = value;
        }
        else
        {
            Model.Fields.Add(new DocumentFieldValue { Name = fieldName, Value = value });
        }
        Model.ModifiedAt = DateTime.Now;
    }
}
