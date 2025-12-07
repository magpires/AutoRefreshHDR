namespace ConfigEditor;

public static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new ConfigEditorForm());
    }

    internal static string? CurrentConfigPath { get; set; }
}
