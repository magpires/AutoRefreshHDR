using System.Diagnostics;
using System.Text;
using System.Text.Json;
using AutoRefreshHDR.ConfigEditor.Models;

namespace AutoRefreshHDR.ConfigEditor.Services;

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
        config.ProgramDisplayConfigs ??= new List<ProgramDisplayConfig>();
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
