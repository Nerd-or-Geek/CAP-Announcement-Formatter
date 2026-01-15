using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Media;
using AnnouncementFormatter.Core.Services;
using System.Diagnostics;

namespace AnnouncementFormatter.ViewModels;

public partial class OllamaSetupViewModel : ViewModelBase
{
    private readonly OllamaService _ollamaService;
    
    [ObservableProperty]
    private string _ollamaStatus = "Checking...";
    
    [ObservableProperty]
    private IBrush _ollamaStatusColor = Brushes.Orange;
    
    [ObservableProperty]
    private string _systemRam = "Detecting...";
    
    [ObservableProperty]
    private string _recommendedLevel = "Detecting...";
    
    [ObservableProperty]
    private bool _showModelSelection = false;
    
    [ObservableProperty]
    private bool _showDownloadSection = false;
    
    [ObservableProperty]
    private bool _showProgress = false;
    
    [ObservableProperty]
    private string _progressMessage = "";
    
    [ObservableProperty]
    private bool _isProgressIndeterminate = true;
    
    [ObservableProperty]
    private double _progressValue = 0;
    
    [ObservableProperty]
    private bool _canSetup = false;
    
    [ObservableProperty]
    private bool _isGoodSelected = true;
    
    [ObservableProperty]
    private bool _isBetterSelected = false;
    
    [ObservableProperty]
    private bool _isBestSelected = false;
    
    [ObservableProperty]
    private bool _canUseBestModel = false;
    
    [ObservableProperty]
    private bool? _dialogResult;

    private SystemCapability _detectedCapability;
    private bool _ollamaInstalled;

    public OllamaSetupViewModel()
    {
        _ollamaService = new OllamaService();
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        // Detect system capabilities
        _detectedCapability = SystemDetector.DetectCapability();
        var ramGB = SystemDetector.GetTotalRamGB();
        
        SystemRam = $"{ramGB:F1} GB";
        RecommendedLevel = SystemDetector.GetCapabilityDescription(_detectedCapability);
        CanUseBestModel = _detectedCapability == SystemCapability.Best;

        // Set default selection based on capability
        IsGoodSelected = _detectedCapability == SystemCapability.Good;
        IsBetterSelected = _detectedCapability == SystemCapability.Better;
        IsBestSelected = _detectedCapability == SystemCapability.Best;

        // Check if Ollama is installed
        _ollamaInstalled = await _ollamaService.IsOllamaInstalledAsync();
        
        if (_ollamaInstalled)
        {
            OllamaStatus = "✅ Installed";
            OllamaStatusColor = Brushes.Green;
            ShowModelSelection = true;
            CanSetup = true;
            
            // Check if model already downloaded
            var models = await _ollamaService.GetInstalledModelsAsync();
            if (models.Count > 0)
            {
                DialogResult = true; // Already setup, can close
            }
        }
        else
        {
            OllamaStatus = "❌ Not Installed";
            OllamaStatusColor = Brushes.Red;
            ShowDownloadSection = true;
            CanSetup = false;
        }
    }

    [RelayCommand]
    private void DownloadOllama()
    {
        try
        {
            var url = SystemDetector.GetOllamaDownloadUrl();
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });

            ProgressMessage = "Please install Ollama and restart this application.";
            ShowProgress = true;
        }
        catch (Exception ex)
        {
            ProgressMessage = $"Error: {ex.Message}";
            ShowProgress = true;
        }
    }

    [RelayCommand]
    private async Task SetupAsync()
    {
        try
        {
            CanSetup = false;
            ShowProgress = true;
            IsProgressIndeterminate = true;
            ProgressMessage = "Downloading AI model...";

            // Determine selected model
            var modelName = GetSelectedModelName();
            
            // Download model
            var progress = new Progress<string>(status =>
            {
                ProgressMessage = status;
            });

            var success = await _ollamaService.PullModelAsync(modelName, progress);
            
            if (success)
            {
                ProgressMessage = "✅ AI model ready!";
                _ollamaService.SetModel(modelName);
                
                await Task.Delay(1000);
                DialogResult = true;
            }
            else
            {
                ProgressMessage = "❌ Failed to download model. Check your internet connection.";
                CanSetup = true;
            }
        }
        catch (Exception ex)
        {
            ProgressMessage = $"Error: {ex.Message}";
            CanSetup = true;
        }
        finally
        {
            IsProgressIndeterminate = false;
        }
    }

    private string GetSelectedModelName()
    {
        if (IsBestSelected)
            return OllamaService.GetRecommendedModel(SystemCapability.Best);
        else if (IsBetterSelected)
            return OllamaService.GetRecommendedModel(SystemCapability.Better);
        else
            return OllamaService.GetRecommendedModel(SystemCapability.Good);
    }

    partial void OnDialogResultChanged(bool? value)
    {
        if (value.HasValue)
        {
            // Window will close with this result
        }
    }
}
