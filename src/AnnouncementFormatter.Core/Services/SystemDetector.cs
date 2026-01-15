using System.Diagnostics;

namespace AnnouncementFormatter.Core.Services;

/// <summary>
/// Detects system capabilities to recommend appropriate AI models
/// </summary>
public static class SystemDetector
{
    /// <summary>
    /// Detects system capability level based on available RAM
    /// </summary>
    public static SystemCapability DetectCapability()
    {
        try
        {
            var totalRamGB = GetTotalRamGB();
            
            if (totalRamGB >= 48)
                return SystemCapability.Best;   // 64GB+ for 70B model
            else if (totalRamGB >= 12)
                return SystemCapability.Better; // 16GB+ for 8B model
            else
                return SystemCapability.Good;   // 8GB+ for 3B model
        }
        catch
        {
            return SystemCapability.Good; // Default to fast model if detection fails
        }
    }

    /// <summary>
    /// Gets total system RAM in GB
    /// </summary>
    public static double GetTotalRamGB()
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "wmic",
                        Arguments = "ComputerSystem get TotalPhysicalMemory",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length > 1 && long.TryParse(lines[1].Trim(), out var bytes))
                {
                    return bytes / (1024.0 * 1024.0 * 1024.0); // Convert to GB
                }
            }
            else if (OperatingSystem.IsLinux())
            {
                var memInfo = File.ReadAllText("/proc/meminfo");
                var match = System.Text.RegularExpressions.Regex.Match(memInfo, @"MemTotal:\s+(\d+)\s+kB");
                if (match.Success && long.TryParse(match.Groups[1].Value, out var kb))
                {
                    return kb / (1024.0 * 1024.0); // Convert KB to GB
                }
            }
            else if (OperatingSystem.IsMacOS())
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "sysctl",
                        Arguments = "-n hw.memsize",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                var output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();

                if (long.TryParse(output, out var bytes))
                {
                    return bytes / (1024.0 * 1024.0 * 1024.0);
                }
            }
        }
        catch { }

        return 8.0; // Default estimate
    }

    /// <summary>
    /// Gets system information summary
    /// </summary>
    public static string GetSystemSummary()
    {
        var ramGB = GetTotalRamGB();
        var capability = DetectCapability();
        
        return $"System RAM: {ramGB:F1} GB\nRecommended: {GetCapabilityDescription(capability)}";
    }

    /// <summary>
    /// Gets user-friendly description of capability level
    /// </summary>
    public static string GetCapabilityDescription(SystemCapability capability)
    {
        return capability switch
        {
            SystemCapability.Good => "Good/Fast (3B model, ~2-3 seconds per response)",
            SystemCapability.Better => "Better/Medium (8B model, ~5-8 seconds per response)",
            SystemCapability.Best => "Best/Slow (70B model, ~20-30 seconds per response)",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Checks if Ollama executable exists in system PATH
    /// </summary>
    public static bool IsOllamaExecutableInstalled()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ollama",
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit(5000);
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the download URL for Ollama based on OS
    /// </summary>
    public static string GetOllamaDownloadUrl()
    {
        if (OperatingSystem.IsWindows())
            return "https://ollama.com/download/OllamaSetup.exe";
        else if (OperatingSystem.IsMacOS())
            return "https://ollama.com/download/Ollama-darwin.zip";
        else if (OperatingSystem.IsLinux())
            return "https://ollama.com/download/ollama-linux-amd64";
        
        return "https://ollama.com/download";
    }
}
