namespace AutoRefreshHDR.Models
{
    public class DisplayConfig
    {
        public ProgramDisplayConfig[] ProgramDisplayConfigs { get; set; } = Array.Empty<ProgramDisplayConfig>();
        public bool UseAutoRefreshRate { get; set; }
        public bool UseAutoHDR { get; set; }
    }
}
