using System.Diagnostics;
using System.Text;

namespace ConfigEditor;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new ConfigEditorForm());
    }

    /// <summary>
    /// Caminho atual do arquivo appsettings sendo editado.
    /// Mantido em memória para que o formulário saiba onde salvar e de onde reiniciar.
    /// </summary>
    internal static string? CurrentConfigPath { get; set; }
}
