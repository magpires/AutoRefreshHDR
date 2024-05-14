namespace AutoRefreshHDR.Models
{
    public class ProgramDisplayConfig
    {
        public string ProgramName { get; set; } = string.Empty;
        public int refreshRate { get; set; }
        public bool Hdr { get; set; }
    }
}
