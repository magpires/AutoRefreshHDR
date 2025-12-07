using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.ComponentModel;
using ConfigEditor.Models;
using ConfigEditor.Services;

namespace ConfigEditor;

public class ConfigEditorForm : Form
{
    private readonly CheckBox _chkUseAutoRefreshRate;
    private readonly CheckBox _chkUseAutoHDR;
    private readonly CheckBox _chkUseBrightness;

    private readonly DataGridView _grid;
    private readonly Button _saveButton;
    private readonly Button _addRowButton;
    private readonly Button _removeRowButton;
    
    private readonly ConfigService _configService;
    private string? _configPath;
    private DisplayConfig _currentConfig = new();
    private BindingList<ProgramDisplayConfig> _bindingList = new();

    public ConfigEditorForm()
    {
        _configService = new ConfigService();
        
        Text = "AutoRefreshHDR - Config Editor";
        Width = 900;
        Height = 650;
        StartPosition = FormStartPosition.CenterScreen;
        
        // Use um TableLayoutPanel para a estrutura principal
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            ColumnCount = 1,
            RowCount = 3, // 1 para config global, 1 para grid, 1 para botões de baixo
            RowStyles =
            {
                new RowStyle(SizeType.Absolute, 70), 
                new RowStyle(SizeType.Percent, 100),
                new RowStyle(SizeType.Absolute, 50)
            }
        };

        // 1. GroupBox para configurações globais
        var globalSettingsGroup = new GroupBox
        {
            Text = "Global Settings",
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };
        
        var globalSettingsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };

        _chkUseAutoRefreshRate = new CheckBox { Text = "Use Auto Refresh Rate", AutoSize = true, Padding = new Padding(0,0,20,0)};
        _chkUseAutoHDR = new CheckBox { Text = "Use Auto HDR", AutoSize = true, Padding = new Padding(0,0,20,0) };
        _chkUseBrightness = new CheckBox { Text = "Use Brightness Level", AutoSize = true };

        globalSettingsPanel.Controls.Add(_chkUseAutoRefreshRate);
        globalSettingsPanel.Controls.Add(_chkUseAutoHDR);
        globalSettingsPanel.Controls.Add(_chkUseBrightness);
        globalSettingsGroup.Controls.Add(globalSettingsPanel);

        // 2. GroupBox para configurações por programa
        var programSettingsGroup = new GroupBox
        {
            Text = "Per-Program Settings",
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };
        
        // Layout interno do grupo de programas
        var programSettingsLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            RowStyles = { new RowStyle(SizeType.Absolute, 40), new RowStyle(SizeType.Percent, 100) }
        };
        
        _addRowButton = new Button { Text = "+ Add", Width = 70 };
        _addRowButton.Click += (_, _) => AddRow();

        _removeRowButton = new Button { Text = "- Remove", Width = 80 };
        _removeRowButton.Click += (_, _) => RemoveSelectedRow();
        
        var gridButtonsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
        };
        gridButtonsPanel.Controls.Add(_addRowButton);
        gridButtonsPanel.Controls.Add(_removeRowButton);
        
        // Grid para ProgramDisplayConfigs
        _grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, // Adicionado para preencher o espaço
            BackgroundColor = Color.White, // Adicionado para cor de fundo branca
            RowHeadersVisible = false // Remove a coluna em branco com a seta de seleção
        };
        const string programNameColumnName = "ProgramNameDGV";

        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name= programNameColumnName, DataPropertyName = nameof(ProgramDisplayConfig.ProgramName), HeaderText = "Program Name", Width = 300 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ProgramDisplayConfig.RefreshRate), HeaderText = "Refresh (Hz)", Width = 90 });
        _grid.Columns.Add(new DataGridViewCheckBoxColumn { DataPropertyName = nameof(ProgramDisplayConfig.Hdr), HeaderText = "HDR", Width = 60 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ProgramDisplayConfig.BrightnessLevel), HeaderText = "Brightness (0-100)", Width = 110 });
        _grid.Columns.Add(new DataGridViewCheckBoxColumn { DataPropertyName = nameof(ProgramDisplayConfig.Active), HeaderText = "Active", Width = 60 });
        _grid.EditingControlShowing += GridOnEditingControlShowing;
        _grid.CellEndEdit += GridOnCellEndEdit;
        
        programSettingsLayout.Controls.Add(gridButtonsPanel, 0, 0);
        programSettingsLayout.Controls.Add(_grid, 0, 1);
        programSettingsGroup.Controls.Add(programSettingsLayout);
        
        // 3. Botões de baixo
        _saveButton = new Button { Text = "Save && Restart", Width = 130, Enabled = false };
        _saveButton.Click += SaveButtonOnClick;

        var bottomPanel = new Panel
        {
            Dock = DockStyle.Fill
        };

        bottomPanel.Controls.Add(_saveButton);
        _saveButton.Dock = DockStyle.Right;
        
        // Adiciona os grupos ao layout principal
        mainLayout.Controls.Add(globalSettingsGroup, 0, 0);
        mainLayout.Controls.Add(programSettingsGroup, 0, 1);
        mainLayout.Controls.Add(bottomPanel, 0, 2);

        Controls.Add(mainLayout);

        Load += (_, _) => TryLoadDefaultConfig();
    }
    
    private void TryLoadDefaultConfig()
    {
        var baseDir = AppContext.BaseDirectory;
        var localPath = Path.Combine(baseDir, "appsettings.jsonc");

        if (!File.Exists(localPath))
        {
            // Cria um arquivo de configuração vazio se não existir
            _configService.CreateEmptyConfig(localPath);
        }
        
        // Tenta carregar o arquivo (existente ou recém-criado)
        LoadConfigFile(localPath);
    }

    private void LoadConfigFile(string path)
    {
        try
        {
            _configPath = path;
            Program.CurrentConfigPath = path;

            Text = $"AutoRefreshHDR - Config Editor ({path})";
            
            _currentConfig = _configService.LoadConfig(path);

            _chkUseAutoRefreshRate.Checked = _currentConfig.UseAutoRefreshRate;
            _chkUseAutoHDR.Checked = _currentConfig.UseAutoHdr;
            _chkUseBrightness.Checked = _currentConfig.UseBrightnessLevel;

            _bindingList = new BindingList<ProgramDisplayConfig>(_currentConfig.ProgramDisplayConfigs);
            _grid.DataSource = _bindingList;

            _saveButton.Enabled = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao ler appsettings.jsonc: {ex.Message}", "Erro",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void GridOnEditingControlShowing(object? sender, DataGridViewEditingControlShowingEventArgs e)
    {
        // Remove manipuladores de eventos anteriores para evitar anexações múltiplas
        e.Control.KeyPress -= NumericOnlyKeyPress;
    
        var columnName = _grid.Columns[_grid.CurrentCell.ColumnIndex].DataPropertyName;

        if (columnName is nameof(ProgramDisplayConfig.RefreshRate) or nameof(ProgramDisplayConfig.BrightnessLevel))
        {
            // Adiciona o manipulador de eventos KeyPress para validação numérica
            e.Control.KeyPress += NumericOnlyKeyPress;
        }
    }
    
    private void NumericOnlyKeyPress(object? sender, KeyPressEventArgs e)
    {
        // Permite apenas dígitos e teclas de controle (como backspace)
        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
        {
            e.Handled = true;
        }
    }

    private void GridOnCellEndEdit(object? sender, DataGridViewCellEventArgs e)
    {
        var column = _grid.Columns[e.ColumnIndex];
        var columnName = column.DataPropertyName;

        if (columnName == nameof(ProgramDisplayConfig.BrightnessLevel))
        {
            var cell = _grid.Rows[e.RowIndex].Cells[e.ColumnIndex];
            var cellValue = cell.Value?.ToString();

            if (uint.TryParse(cellValue, out var value) && value > 100)
            {
                // Mostra um aviso e corrige o valor para 100
                MessageBox.Show("O valor de Brightness não pode ser maior que 100. Ele será ajustado para 100.", "Valor Inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cell.Value = 100;
            }
        }
    }

    private void AddRow()
    {
        using var ofd = new OpenFileDialog
        {
            Title = "Selecione o executável",
            Filter = "Executables (*.exe)|*.exe|Todos os arquivos (*.*)|*.*",
            CheckFileExists = true
        };

        if (ofd.ShowDialog(this) == DialogResult.OK)
        {
            var fileName = Path.GetFileName(ofd.FileName);
            if (_bindingList.Any(p => p.ProgramName.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show($"O programa '{fileName}' já existe na lista.", "Programa Duplicado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            _bindingList.Add(new ProgramDisplayConfig
            {
                ProgramName = fileName,
                RefreshRate = null,
                Hdr = false,
                BrightnessLevel = 100,
                Active = true
            });
        }
    }

    private void RemoveSelectedRow()
    {
        if (_grid.CurrentRow == null || _grid.CurrentRow.Index < 0) return;
        var item = _grid.CurrentRow.DataBoundItem as ProgramDisplayConfig;
        if (item != null)
        {
            _bindingList.Remove(item);
        }
    }

    private void SaveButtonOnClick(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_configPath) || !File.Exists(_configPath))
        {
            MessageBox.Show("Nenhum appsettings.jsonc válido selecionado.", "Erro",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Garante que edições pendentes na grid sejam aplicadas ao binding list
        _grid.EndEdit();
        if (BindingContext[_bindingList] is CurrencyManager cm)
        {
            cm.EndCurrentEdit();
        }

        _currentConfig.UseAutoRefreshRate = _chkUseAutoRefreshRate.Checked;
        _currentConfig.UseAutoHdr = _chkUseAutoHDR.Checked;
        _currentConfig.UseBrightnessLevel = _chkUseBrightness.Checked;
        _currentConfig.ProgramDisplayConfigs = _bindingList.ToList();

        try
        {
            _configService.SaveConfig(_configPath, _currentConfig);
            _configService.RestartMainApp(_configPath);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao salvar ou reiniciar: {ex.Message}", "Erro",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        MessageBox.Show("Configurações salvas e AutoRefreshHDR reiniciado.", "Sucesso",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
