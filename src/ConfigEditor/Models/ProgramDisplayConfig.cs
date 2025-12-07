namespace ConfigEditor.Models
{
    public class ProgramDisplayConfig
    {
        public string ProgramName { get; init; } = string.Empty;
        public uint? RefreshRate { get; init; }
        public bool Hdr { get; init; }
        public uint? BrightnessLevel { get; init; }
        public bool Active { get; init; }
    }
}
