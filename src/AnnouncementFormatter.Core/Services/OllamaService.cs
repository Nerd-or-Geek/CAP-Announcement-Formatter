using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AnnouncementFormatter.Core.Services;

/// <summary>
/// Service for interacting with Ollama local LLM to improve formatting
/// </summary>
public class OllamaService
{
    private readonly HttpClient _httpClient;
    private const string DefaultOllamaUrl = "http://localhost:11434";
    private string? _currentModel;

    public OllamaService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(DefaultOllamaUrl),
            Timeout = TimeSpan.FromMinutes(2)
        };
    }

    /// <summary>
    /// Checks if Ollama is installed and running
    /// </summary>
    public async Task<bool> IsOllamaInstalledAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/version");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets list of installed models
    /// </summary>
    public async Task<List<OllamaModel>> GetInstalledModelsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/tags");
            if (!response.IsSuccessStatusCode) return new List<OllamaModel>();

            var result = await response.Content.ReadFromJsonAsync<OllamaTagsResponse>();
            return result?.Models ?? new List<OllamaModel>();
        }
        catch
        {
            return new List<OllamaModel>();
        }
    }

    /// <summary>
    /// Pulls (downloads) a model from Ollama
    /// </summary>
    public async Task<bool> PullModelAsync(string modelName, IProgress<string>? progress = null)
    {
        try
        {
            var request = new { name = modelName, stream = true };
            var response = await _httpClient.PostAsJsonAsync("/api/pull", request);
            
            if (!response.IsSuccessStatusCode) return false;

            // Stream progress updates
            await using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);
            
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(line)) continue;
                
                try
                {
                    var update = JsonSerializer.Deserialize<OllamaPullProgress>(line);
                    progress?.Report(update?.Status ?? "Downloading...");
                }
                catch { }
            }

            _currentModel = modelName;
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Improves HTML/text formatting using AI
    /// </summary>
    public async Task<string> ImproveFormattingAsync(string content, string context = "announcement")
    {
        if (string.IsNullOrEmpty(_currentModel))
        {
            // Try to use any available model
            var models = await GetInstalledModelsAsync();
            if (models.Count == 0)
                throw new InvalidOperationException("No Ollama models available");
            
            _currentModel = models[0].Name;
        }

        var prompt = $@"You are an expert in formatting professional announcements and HTML content.
Please improve the following {context} by:
- Fixing grammar and spelling
- Improving clarity and readability
- Ensuring professional tone
- Maintaining the original HTML structure if present
- Keeping all template variables like {{{{variable}}}} intact

Content to improve:
{content}

Provide ONLY the improved content, no explanations.";

        try
        {
            var request = new
            {
                model = _currentModel,
                prompt = prompt,
                stream = false,
                options = new
                {
                    temperature = 0.3,  // Lower temperature for more consistent output
                    top_p = 0.9
                }
            };

            var response = await _httpClient.PostAsJsonAsync("/api/generate", request);
            if (!response.IsSuccessStatusCode)
                return content; // Return original on error

            var result = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>();
            return result?.Response?.Trim() ?? content;
        }
        catch
        {
            return content; // Return original on error
        }
    }

    /// <summary>
    /// Suggests improvements without modifying content
    /// </summary>
    public async Task<string> GetSuggestionsAsync(string content)
    {
        if (string.IsNullOrEmpty(_currentModel))
        {
            var models = await GetInstalledModelsAsync();
            if (models.Count == 0)
                return "No AI model available";
            
            _currentModel = models[0].Name;
        }

        var prompt = $@"Analyze this announcement content and provide 3-5 specific suggestions to improve it:
{content}

Provide suggestions as a numbered list.";

        try
        {
            var request = new
            {
                model = _currentModel,
                prompt = prompt,
                stream = false
            };

            var response = await _httpClient.PostAsJsonAsync("/api/generate", request);
            if (!response.IsSuccessStatusCode)
                return "Unable to generate suggestions";

            var result = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>();
            return result?.Response?.Trim() ?? "No suggestions available";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Sets the current model to use
    /// </summary>
    public void SetModel(string modelName)
    {
        _currentModel = modelName;
    }

    /// <summary>
    /// Gets recommended model based on system capabilities
    /// </summary>
    public static string GetRecommendedModel(SystemCapability capability)
    {
        return capability switch
        {
            SystemCapability.Good => "llama3.2:3b",      // Fast, 3B parameters
            SystemCapability.Better => "llama3.2:8b",    // Medium, 8B parameters
            SystemCapability.Best => "llama3.2:70b",     // Slow, 70B parameters
            _ => "llama3.2:3b"
        };
    }
}

/// <summary>
/// System capability levels
/// </summary>
public enum SystemCapability
{
    Good,    // Good/Fast - 3B model (4GB RAM)
    Better,  // Better/Medium - 8B model (8GB RAM)
    Best     // Best/Slow - 70B model (64GB RAM)
}

// DTOs for Ollama API
public class OllamaModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
    
    [JsonPropertyName("size")]
    public long Size { get; set; }
    
    [JsonPropertyName("modified_at")]
    public DateTime ModifiedAt { get; set; }
}

public class OllamaTagsResponse
{
    [JsonPropertyName("models")]
    public List<OllamaModel> Models { get; set; } = new();
}

public class OllamaPullProgress
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = "";
    
    [JsonPropertyName("completed")]
    public long Completed { get; set; }
    
    [JsonPropertyName("total")]
    public long Total { get; set; }
}

public class OllamaGenerateResponse
{
    [JsonPropertyName("response")]
    public string Response { get; set; } = "";
    
    [JsonPropertyName("done")]
    public bool Done { get; set; }
}
