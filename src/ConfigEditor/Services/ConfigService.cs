using System.Diagnostics;
using System.Text;
using System.Text.Json;
using ConfigEditor.Models;

namespace ConfigEditor.Services;

public class ConfigService
{
    public DisplayConfig LoadConfig(string path)
    {
        var json = File.ReadAllText(path, Encoding.UTF8);
        var options = new JsonSerializerOptions
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true
        };

        var config = JsonSerializer.Deserialize<DisplayConfig>(json, options) ?? new DisplayConfig();
        return config;
    }

    public void SaveConfig(string path, DisplayConfig config)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        var json = JsonSerializer.Serialize(config, options);
        File.WriteAllText(path, json, Encoding.UTF8);
    }
    
    public void CreateEmptyConfig(string path)
    {
        var emptyConfig = new DisplayConfig
        {
            ProgramDisplayConfigs = new List<ProgramDisplayConfig>(),
            UseAutoRefreshRate = true,
            UseAutoHdr = true,
            UseBrightnessLevel = true
        };
        SaveConfig(path, emptyConfig);
    }

    public void RestartMainApp(string configPath)
    {
        var autoRefreshDir = Path.GetDirectoryName(configPath) ?? AppContext.BaseDirectory;
        var batPath = Path.Combine(autoRefreshDir, "RestartAutoRefreshHDR.bat");

        if (!File.Exists(batPath))
        {
            throw new FileNotFoundException($"RestartAutoRefreshHDR.bat não encontrado em {autoRefreshDir}", batPath);
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c \"{batPath}\"",
            WorkingDirectory = autoRefreshDir,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        Process.Start(startInfo);
    }
}
