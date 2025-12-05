using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.ComponentModel;
using AutoRefreshHDR.ConfigEditor.Models;

namespace AutoRefreshHDR.ConfigEditor;

public class ConfigEditorForm : Form
{
    private readonly CheckBox _chkUseAutoRefreshRate;
    private readonly CheckBox _chkUseAutoHDR;
    private readonly CheckBox _chkUseBrightness;

    private readonly DataGridView _grid;
    private readonly Button _saveButton;
    private readonly Button _openButton;
    private readonly Button _addRowButton;
    private readonly Button _removeRowButton;

    private string? _configPath;
    private DisplayConfig _currentConfig = new();
    private BindingList<ProgramDisplayConfig> _bindingList = new();

    public ConfigEditorForm()
    {
        Text = "AutoRefreshHDR - Config Editor";
        Width = 900;
        Height = 650;
        StartPosition = FormStartPosition.CenterScreen;

        // Painel de opções gerais (checkboxes)
        var topPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 40,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(8, 8, 8, 0)
        };

        _chkUseAutoRefreshRate = new CheckBox { Text = "Use Auto Refresh Rate" };
        _chkUseAutoHDR = new CheckBox { Text = "Use Auto HDR" };
        _chkUseBrightness = new CheckBox { Text = "Use Brightness Level" };

        topPanel.Controls.Add(_chkUseAutoRefreshRate);
        topPanel.Controls.Add(_chkUseAutoHDR);
        topPanel.Controls.Add(_chkUseBrightness);

        // Grid para ProgramDisplayConfigs
        _grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false
        };

        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(ProgramDisplayConfig.ProgramName),
            HeaderText = "Program Name",
            Width = 250
        });

        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(ProgramDisplayConfig.RefreshRate),
            HeaderText = "Refresh (Hz)",
            Width = 90
        });

        _grid.Columns.Add(new DataGridViewCheckBoxColumn
        {
            DataPropertyName = nameof(ProgramDisplayConfig.Hdr),
            HeaderText = "HDR",
            Width = 60
        });

        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(ProgramDisplayConfig.BrightnessLevel),
            HeaderText = "Brightness (0-100)",
            Width = 110
        });

        _grid.Columns.Add(new DataGridViewCheckBoxColumn
        {
            DataPropertyName = nameof(ProgramDisplayConfig.Active),
            HeaderText = "Active",
            Width = 60
        });

        _grid.CellValidating += GridOnCellValidating;

        // Botões de baixo
        _saveButton = new Button
        {
            Text = "Save && Restart",
            Dock = DockStyle.Right,
            Width = 130,
            Enabled = false
        };
        _saveButton.Click += SaveButtonOnClick;

        _openButton = new Button
        {
            Text = "Open appsettings.jsonc",
            Dock = DockStyle.Left,
            Width = 180
        };
        _openButton.Click += OpenButtonOnClick;

        _addRowButton = new Button
        {
            Text = "+ Add",
            Dock = DockStyle.Left,
            Width = 70
        };
        _addRowButton.Click += (_, _) => AddRow();

        _removeRowButton = new Button
        {
            Text = "- Remove",
            Dock = DockStyle.Left,
            Width = 80
        };
        _removeRowButton.Click += (_, _) => RemoveSelectedRow();

        var bottomPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 40
        };

        bottomPanel.Controls.Add(_saveButton);
        bottomPanel.Controls.Add(_removeRowButton);
        bottomPanel.Controls.Add(_addRowButton);
        bottomPanel.Controls.Add(_openButton);

        Controls.Add(_grid);
        Controls.Add(topPanel);
        Controls.Add(bottomPanel);

        Load += (_, _) => TryLoadDefaultConfig();
    }

    private void TryLoadDefaultConfig()
    {
        // 1) Tenta abrir um appsettings.jsonc no mesmo diretório do executável do editor
        var baseDir = AppContext.BaseDirectory;
        var localPath = Path.Combine(baseDir, "appsettings.jsonc");

        if (File.Exists(localPath))
        {
            LoadConfigFile(localPath);
            return;
        }

        // 2) Caso não exista, força o usuário a escolher o arquivo do projeto AutoRefreshHDR
        OpenConfigFileWithDialog();
    }

    private void OpenButtonOnClick(object? sender, EventArgs e)
    {
        OpenConfigFileWithDialog();
    }

    private void OpenConfigFileWithDialog()
    {
        using var ofd = new OpenFileDialog
        {
            Title = "Selecione o appsettings.jsonc do AutoRefreshHDR",
            Filter = "JSON config (appsettings.jsonc)|appsettings.jsonc|Todos os arquivos (*.*)|*.*",
            CheckFileExists = true,
            CheckPathExists = true,
            FileName = "appsettings.jsonc"
        };

        if (ofd.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        LoadConfigFile(ofd.FileName);
    }

    private void LoadConfigFile(string path)
    {
        try
        {
            _configPath = path;
            Program.CurrentConfigPath = path;

            Text = $"AutoRefreshHDR - Config Editor ({path})";

            var json = File.ReadAllText(path, Encoding.UTF8);
            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true
            };

            _currentConfig = JsonSerializer.Deserialize<DisplayConfig>(json, options) ?? new DisplayConfig();
            _currentConfig.ProgramDisplayConfigs ??= new List<ProgramDisplayConfig>();

            _chkUseAutoRefreshRate.Checked = _currentConfig.UseAutoRefreshRate;
            _chkUseAutoHDR.Checked = _currentConfig.UseAutoHDR;
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

    private void GridOnCellValidating(object? sender, DataGridViewCellValidatingEventArgs e)
    {
        if (e.RowIndex < 0) return;

        var columnName = _grid.Columns[e.ColumnIndex].DataPropertyName;
        if (columnName is nameof(ProgramDisplayConfig.RefreshRate) or nameof(ProgramDisplayConfig.BrightnessLevel))
        {
            var text = e.FormattedValue?.ToString();
            if (string.IsNullOrWhiteSpace(text))
            {
                _grid.Rows[e.RowIndex].ErrorText = string.Empty;
                return; // permite nulo
            }

            if (!uint.TryParse(text, out var value))
            {
                e.Cancel = true;
                _grid.Rows[e.RowIndex].ErrorText = "Digite apenas números inteiros.";
                return;
            }

            if (columnName == nameof(ProgramDisplayConfig.BrightnessLevel) && (value > 100))
            {
                e.Cancel = true;
                _grid.Rows[e.RowIndex].ErrorText = "Brightness deve ser entre 0 e 100.";
                return;
            }

            _grid.Rows[e.RowIndex].ErrorText = string.Empty;
        }
    }

    private void AddRow()
    {
        _bindingList.Add(new ProgramDisplayConfig
        {
            ProgramName = string.Empty,
            RefreshRate = null,
            Hdr = false,
            BrightnessLevel = 100,
            Active = true
        });
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
        _currentConfig.UseAutoHDR = _chkUseAutoHDR.Checked;
        _currentConfig.UseBrightnessLevel = _chkUseBrightness.Checked;
        _currentConfig.ProgramDisplayConfigs = _bindingList.ToList();

        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var json = JsonSerializer.Serialize(_currentConfig, options);
            File.WriteAllText(_configPath, json, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao salvar appsettings.jsonc: {ex.Message}", "Erro",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try
        {
            // Pasta onde está o AutoRefreshHDR.exe (mesma pasta do appsettings selecionado)
            var autoRefreshDir = Path.GetDirectoryName(_configPath) ?? AppContext.BaseDirectory;
            var batPath = Path.Combine(autoRefreshDir, "RestartAutoRefreshHDR.bat");

            if (!File.Exists(batPath))
            {
                MessageBox.Show($"RestartAutoRefreshHDR.bat não encontrado em {autoRefreshDir}", "Erro",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c \"RestartAutoRefreshHDR.bat\"",
                WorkingDirectory = autoRefreshDir,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao reiniciar o AutoRefreshHDR: {ex.Message}", "Erro",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        MessageBox.Show("Configurações salvas e AutoRefreshHDR reiniciado.", "Sucesso",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
