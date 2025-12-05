using System.Text.Json.Serialization;

namespace AutoRefreshHDR.ConfigEditor.Models;

public class DisplayConfig
{
    public List<ProgramDisplayConfig> ProgramDisplayConfigs { get; set; } = new();

    public bool UseAutoRefreshRate { get; set; }

    // Nome igual ao do JSON: "UseAutoHDR"
    public bool UseAutoHDR { get; set; }

    public bool UseBrightnessLevel { get; set; }
}

public class ProgramDisplayConfig
{
    public string ProgramName { get; set; } = string.Empty;

    public uint? RefreshRate { get; set; }

    public bool Hdr { get; set; }

    public uint? BrightnessLevel { get; set; }

    public bool Active { get; set; }
}
