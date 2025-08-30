namespace AutoRefreshHDR.Models
{
    public class ProgramDisplayConfig
    {
        public string ProgramName { get; set; } = string.Empty;
        public uint RefreshRate { get; set; }
        public bool Hdr { get; set; }
    }
}
