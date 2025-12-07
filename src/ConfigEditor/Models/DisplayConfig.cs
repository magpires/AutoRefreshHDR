namespace ConfigEditor.Models;

public class DisplayConfig
{
    public List<ProgramDisplayConfig> ProgramDisplayConfigs { get; set; } = [];

    public bool UseAutoRefreshRate { get; set; }

    public bool UseAutoHdr { get; set; }

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
