namespace ConfigEditor.Models;

public class DisplayConfig
{
    public List<ProgramDisplayConfig> ProgramDisplayConfigs { get; set; } = [];

    public bool UseAutoRefreshRate { get; set; }

    public bool UseAutoHdr { get; set; }

    public bool UseBrightnessLevel { get; set; }
}
