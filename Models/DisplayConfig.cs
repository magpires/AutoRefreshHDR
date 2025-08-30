namespace AutoRefreshHDR.Models
{
    public class DisplayConfig
    {
        public ProgramDisplayConfig[] ProgramDisplayConfigs { get; init; } = [];
        public bool UseAutoRefreshRate { get; init; }
        public bool UseAutoHdr { get; init; }
    }
}
