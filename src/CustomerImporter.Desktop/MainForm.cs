using System.ComponentModel;
using CustomerImporter.Core.Models;
using CustomerImporter.Core.Services;

namespace CustomerImporter.Desktop;

public class MainForm : Form
{
    private readonly CustomerStore _store;
    private Dictionary<string, Customer> _customers = new();
    private readonly BindingList<Customer> _bindingList = new();
    private readonly BindingSource _bindingSource = new();

    private readonly DataGridView _grid = new();
    private readonly ProgressBar _progressBar = new();
    private readonly Label _statusLabel = new();
    private readonly ToolStrip _toolbar = new();
    private readonly ToolStripButton _btnImportCsv = new();
    private readonly ToolStripButton _btnImportJson = new();
    private readonly ToolStripButton _btnAdd = new();
    private readonly ToolStripButton _btnDelete = new();
    private readonly ToolStripButton _btnExportCsv = new();
    private readonly ToolStripButton _btnExportJson = new();
    private ToolStripStatusLabel _countLabel = new();
    private ToolStripStatusLabel _saveIndicator = new();

    private bool _isPersisted = true;

    public MainForm()
    {
        var dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
        _store = new CustomerStore(Path.Combine(dataDir, "clientes_store.db"));

        InitializeLayout();
        LoadStore();
    }

    private void InitializeLayout()
    {
        Text = "Gestión de Clientes — Customer Importer";
        Size = new Size(1280, 720);
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1024, 600);
        Font = new Font("Segoe UI", 9.5f);

        // --- ToolStrip ---
        _toolbar.GripStyle = ToolStripGripStyle.Hidden;
        _toolbar.Padding = new Padding(4, 2, 4, 2);
        _toolbar.ImageScalingSize = new Size(20, 20);
        _toolbar.Renderer = new ToolStripProfessionalRenderer(
            new ProfessionalColorTable { UseSystemColors = true });

        _btnImportCsv.Text = "  Importar CSV  ";
        _btnImportCsv.DisplayStyle = ToolStripItemDisplayStyle.Text;
        _btnImportCsv.Click += async (_, _) => await ImportFile("csv");

        _btnImportJson.Text = "  Importar JSON  ";
        _btnImportJson.DisplayStyle = ToolStripItemDisplayStyle.Text;
        _btnImportJson.Click += async (_, _) => await ImportFile("json");

        _btnAdd.Text = "  Añadir cliente  ";
        _btnAdd.DisplayStyle = ToolStripItemDisplayStyle.Text;
        _btnAdd.Click += OnAddClick;

        _btnDelete.Text = "  Eliminar seleccionado  ";
        _btnDelete.DisplayStyle = ToolStripItemDisplayStyle.Text;
        _btnDelete.Click += OnDeleteClick;

        _btnExportCsv.Text = "  Exportar CSV  ";
        _btnExportCsv.DisplayStyle = ToolStripItemDisplayStyle.Text;
        _btnExportCsv.Click += (_, _) => ExportFile("csv");

        _btnExportJson.Text = "  Exportar JSON  ";
        _btnExportJson.DisplayStyle = ToolStripItemDisplayStyle.Text;
        _btnExportJson.Click += (_, _) => ExportFile("json");

        _toolbar.Items.Add(_btnImportCsv);
        _toolbar.Items.Add(_btnImportJson);
        _toolbar.Items.Add(new ToolStripSeparator());
        _toolbar.Items.Add(_btnExportCsv);
        _toolbar.Items.Add(_btnExportJson);
        _toolbar.Items.Add(new ToolStripSeparator());
        _toolbar.Items.Add(_btnAdd);
        _toolbar.Items.Add(_btnDelete);

        // --- StatusStrip ---
        var statusStrip = new StatusStrip();
        var statusItem = new ToolStripStatusLabel { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
        _statusLabel.Tag = statusItem;
        statusItem.Text = "Listo.";

        _countLabel = new ToolStripStatusLabel
        {
            Alignment = ToolStripItemAlignment.Right,
            Font = new Font("Segoe UI", 9f),
            ForeColor = Color.FromArgb(100, 100, 100)
        };

        _saveIndicator = new ToolStripStatusLabel
        {
            Alignment = ToolStripItemAlignment.Right,
            Font = new Font("Segoe UI Semibold", 9f),
            BorderSides = ToolStripStatusLabelBorderSides.Left,
            Padding = new Padding(6, 0, 6, 0)
        };

        statusStrip.Items.Add(statusItem);
        statusStrip.Items.Add(_countLabel);
        statusStrip.Items.Add(_saveIndicator);

        // --- ProgressBar ---
        _progressBar.Dock = DockStyle.Bottom;
        _progressBar.Height = 6;
        _progressBar.Visible = false;

        // --- DataGridView ---
        _bindingSource.DataSource = _bindingList;

        _grid.Dock = DockStyle.Fill;
        _grid.DataSource = _bindingSource;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
        _grid.BackgroundColor = SystemColors.Window;
        _grid.BorderStyle = BorderStyle.Fixed3D;
        _grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        _grid.GridColor = Color.FromArgb(220, 225, 230);
        _grid.RowHeadersVisible = false;
        _grid.ColumnHeadersHeight = 42;
        _grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        _grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            Font = new Font("Segoe UI Semibold", 10f),
            BackColor = Color.FromArgb(55, 71, 95),
            ForeColor = Color.White,
            Alignment = DataGridViewContentAlignment.MiddleLeft,
            Padding = new Padding(8, 0, 4, 0)
        };
        _grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            Font = new Font("Segoe UI", 9.75f),
            Padding = new Padding(8, 4, 4, 4),
            SelectionBackColor = Color.FromArgb(210, 228, 250),
            SelectionForeColor = Color.Black
        };
        _grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.FromArgb(247, 249, 252),
            SelectionBackColor = Color.FromArgb(210, 228, 250),
            SelectionForeColor = Color.Black
        };
        _grid.EnableHeadersVisualStyles = false;
        _grid.RowTemplate.Height = 38;
        _grid.DataBindingComplete += OnGridDataBindingComplete;
        _grid.CellEndEdit += OnCellEndEdit;

        // --- Layout order matters: last added = bottom ---
        Controls.Add(_grid);
        Controls.Add(_toolbar);
        Controls.Add(_progressBar);
        Controls.Add(statusStrip);

        FormClosing += OnFormClosing;
    }

    private void OnGridDataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
    {
        if (_grid.Columns.Count == 0) return;

        var config = new Dictionary<string, (string Header, int FillWeight, DataGridViewContentAlignment Align)>
        {
            ["Dni"]             = ("DNI",              10, DataGridViewContentAlignment.MiddleLeft),
            ["Nombre"]          = ("Nombre",           15, DataGridViewContentAlignment.MiddleLeft),
            ["Apellidos"]       = ("Apellidos",        20, DataGridViewContentAlignment.MiddleLeft),
            ["FechaNacimiento"] = ("Fecha Nac.",       12, DataGridViewContentAlignment.MiddleCenter),
            ["Telefono"]        = ("Teléfono",         13, DataGridViewContentAlignment.MiddleLeft),
            ["Email"]           = ("Email",            30, DataGridViewContentAlignment.MiddleLeft),
        };

        foreach (DataGridViewColumn col in _grid.Columns)
        {
            if (!config.TryGetValue(col.Name, out var cfg)) continue;
            col.HeaderText = cfg.Header;
            col.FillWeight = cfg.FillWeight;
            col.DefaultCellStyle.Alignment = cfg.Align;
            col.MinimumWidth = 80;
        }
    }

    private void OnCellEndEdit(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.RowIndex >= _bindingList.Count) return;

        var customer = _bindingList[e.RowIndex];
        var errors = CustomerValidator.Validate(customer);

        ClearRowErrors(e.RowIndex);

        if (errors.Count > 0)
        {
            foreach (var error in errors)
                MarkCellError(e.RowIndex, error.Field, error.Message);

            MarkUnsaved();
            SetStatus($"Fila {e.RowIndex + 1}: {errors.Count} campo(s) inválido(s). Corrija para guardar.");
            return;
        }

        if (ValidateAllRows())
            PersistAndMarkSaved();
    }

    private bool ValidateAllRows()
    {
        var allValid = true;
        for (int i = 0; i < _bindingList.Count; i++)
        {
            ClearRowErrors(i);
            var errors = CustomerValidator.Validate(_bindingList[i]);
            if (errors.Count > 0)
            {
                foreach (var error in errors)
                    MarkCellError(i, error.Field, error.Message);
                allValid = false;
            }
        }
        return allValid;
    }

    private void ClearRowErrors(int rowIndex)
    {
        if (rowIndex >= _grid.Rows.Count) return;
        var row = _grid.Rows[rowIndex];
        row.ErrorText = "";
        foreach (DataGridViewCell cell in row.Cells)
        {
            cell.ErrorText = "";
            cell.Style.BackColor = Color.Empty;
        }
    }

    private void MarkCellError(int rowIndex, string fieldName, string message)
    {
        if (rowIndex >= _grid.Rows.Count) return;
        if (_grid.Columns[fieldName] is not { } col) return;

        var cell = _grid.Rows[rowIndex].Cells[col.Index];
        cell.ErrorText = message;
        cell.Style.BackColor = Color.FromArgb(255, 230, 230);
    }

    private void PersistAndMarkSaved()
    {
        SyncBindingListToDict();
        _store.Save(_customers);
        _isPersisted = true;
        UpdateSaveIndicator();
        SetStatus($"Guardado. {_customers.Count} cliente(s) persistidos.");
    }

    private void MarkUnsaved()
    {
        _isPersisted = false;
        UpdateSaveIndicator();
    }

    private void UpdateSaveIndicator()
    {
        if (_isPersisted)
        {
            _saveIndicator.Text = "Guardado";
            _saveIndicator.ForeColor = Color.FromArgb(30, 130, 50);
        }
        else
        {
            _saveIndicator.Text = "Sin guardar";
            _saveIndicator.ForeColor = Color.FromArgb(200, 50, 50);
        }
    }

    private void LoadStore()
    {
        _customers = _store.Load();
        RefreshGrid();
        _isPersisted = true;
        UpdateSaveIndicator();
        SetStatus(_customers.Count > 0
            ? $"Cargados {_customers.Count} clientes desde almacén local."
            : "Listo.");
    }

    private async Task ImportFile(string format)
    {
        var filter = format == "csv"
            ? "Archivos CSV (*.csv)|*.csv"
            : "Archivos JSON (*.json)|*.json";

        using var dialog = new OpenFileDialog
        {
            Filter = filter,
            Title = $"Seleccionar fichero {format.ToUpper()}"
        };
        if (dialog.ShowDialog() != DialogResult.OK) return;

        SetButtonsEnabled(false);
        _progressBar.Visible = true;
        _progressBar.Value = 0;
        SetStatus("Importando...");

        var content = await File.ReadAllTextAsync(dialog.FileName);

        var result = format == "csv"
            ? CsvCustomerImporter.Import(content)
            : JsonCustomerImporter.Import(content);

        using var importDialog = new ImportDialog();
        importDialog.ShowResult(result);
        importDialog.ShowDialog(this);

        _progressBar.Visible = false;
        SetButtonsEnabled(true);

        if (!importDialog.Accepted)
        {
            SetStatus("Importación cancelada por el usuario.");
            return;
        }

        foreach (var customer in result.Customers)
        {
            if (customer.Dni is not null)
                _customers[customer.Dni] = customer;
        }

        RefreshGrid();
        PersistAndMarkSaved();
        SetStatus($"Importados {result.Customers.Count} clientes. Total: {_customers.Count}.");
    }

    private void ExportFile(string format)
    {
        if (_bindingList.Count == 0)
        {
            SetStatus("No hay clientes para exportar.");
            return;
        }

        var filter = format == "csv"
            ? "Archivos CSV (*.csv)|*.csv"
            : "Archivos JSON (*.json)|*.json";

        using var dialog = new SaveFileDialog
        {
            Filter = filter,
            Title = $"Exportar clientes a {format.ToUpper()}",
            FileName = $"clientes.{format}"
        };

        if (dialog.ShowDialog() != DialogResult.OK) return;

        var customers = _bindingList.ToList();
        var content = format == "csv"
            ? CsvCustomerExporter.Export(customers)
            : JsonCustomerExporter.Export(customers);

        File.WriteAllText(dialog.FileName, content);
        SetStatus($"Exportados {customers.Count} clientes a {dialog.FileName}.");
    }

    private void OnAddClick(object? sender, EventArgs e)
    {
        _bindingList.Add(new Customer());
        _grid.CurrentCell = _grid.Rows[^1].Cells[0];
        _grid.BeginEdit(true);
        MarkUnsaved();
        SetStatus("Nuevo cliente. Rellene todos los campos correctamente para guardar.");
    }

    private void OnDeleteClick(object? sender, EventArgs e)
    {
        if (_grid.SelectedRows.Count == 0)
        {
            SetStatus("Seleccione al menos una fila para eliminar.");
            return;
        }

        var toRemove = _grid.SelectedRows.Cast<DataGridViewRow>()
            .Select(r => r.DataBoundItem as Customer)
            .Where(c => c is not null)
            .ToList();

        if (toRemove.Count == 0) return;

        var confirm = MessageBox.Show(
            $"¿Eliminar {toRemove.Count} cliente(s)?",
            "Confirmar eliminación",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes) return;

        foreach (var customer in toRemove)
        {
            if (customer!.Dni is not null)
                _customers.Remove(customer.Dni);
            _bindingList.Remove(customer);
        }

        _countLabel.Text = $"{_bindingList.Count} registro(s)";

        if (ValidateAllRows())
            PersistAndMarkSaved();
        else
            MarkUnsaved();

        SetStatus($"Eliminados {toRemove.Count} clientes. Total: {_customers.Count}.");
    }

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        if (_isPersisted) return;

        if (!ValidateAllRows())
        {
            var result = MessageBox.Show(
                "Hay datos con errores de validación que no se pueden guardar.\n\n¿Desea salir sin guardar los cambios?",
                "Datos no guardados",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }
        }
        else
        {
            PersistAndMarkSaved();
        }
    }

    private void SyncBindingListToDict()
    {
        _customers.Clear();
        foreach (var c in _bindingList)
        {
            if (c.Dni is not null)
                _customers[c.Dni] = c;
        }
    }

    private void RefreshGrid()
    {
        _bindingList.Clear();
        foreach (var c in _customers.Values)
            _bindingList.Add(c);
        _countLabel.Text = $"{_bindingList.Count} registro(s)";
    }

    private void SetButtonsEnabled(bool enabled)
    {
        _btnImportCsv.Enabled = enabled;
        _btnImportJson.Enabled = enabled;
        _btnExportCsv.Enabled = enabled;
        _btnExportJson.Enabled = enabled;
        _btnAdd.Enabled = enabled;
        _btnDelete.Enabled = enabled;
    }

    private void SetStatus(string text)
    {
        if (_statusLabel.Tag is ToolStripStatusLabel item)
            item.Text = text;
    }
}
