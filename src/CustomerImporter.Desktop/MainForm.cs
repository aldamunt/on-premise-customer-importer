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
    private readonly Button _btnImportCsv = new();
    private readonly Button _btnImportJson = new();
    private readonly Button _btnAdd = new();
    private readonly Button _btnDelete = new();

    public MainForm()
    {
        var dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
        _store = new CustomerStore(Path.Combine(dataDir, "clientes_store.db"));

        InitializeLayout();
        LoadStore();
    }

    private void InitializeLayout()
    {
        Text = "Customer Importer";
        Size = new Size(900, 550);
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(700, 400);

        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 40,
            Padding = new Padding(4),
            AutoSize = true
        };

        _btnImportCsv.Text = "Importar CSV";
        _btnImportCsv.Click += async (_, _) => await ImportFile("csv");

        _btnImportJson.Text = "Importar JSON";
        _btnImportJson.Click += async (_, _) => await ImportFile("json");

        _btnAdd.Text = "Añadir";
        _btnAdd.Click += OnAddClick;

        _btnDelete.Text = "Eliminar";
        _btnDelete.Click += OnDeleteClick;

        toolbar.Controls.AddRange([_btnImportCsv, _btnImportJson, _btnAdd, _btnDelete]);

        _progressBar.Dock = DockStyle.Bottom;
        _progressBar.Height = 20;
        _progressBar.Visible = false;

        _statusLabel.Dock = DockStyle.Bottom;
        _statusLabel.Height = 20;
        _statusLabel.Text = "Listo.";

        _bindingSource.DataSource = _bindingList;

        _grid.Dock = DockStyle.Fill;
        _grid.DataSource = _bindingSource;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.EditMode = DataGridViewEditMode.EditOnDoubleClick;

        Controls.Add(_grid);
        Controls.Add(toolbar);
        Controls.Add(_progressBar);
        Controls.Add(_statusLabel);

        FormClosing += OnFormClosing;
    }

    private void LoadStore()
    {
        _customers = _store.Load();
        RefreshGrid();
        _statusLabel.Text = _customers.Count > 0
            ? $"Cargados {_customers.Count} clientes desde almacén local."
            : "Listo.";
    }

    private async Task ImportFile(string format)
    {
        var filter = format == "csv"
            ? "CSV files (*.csv)|*.csv"
            : "JSON files (*.json)|*.json";

        using var dialog = new OpenFileDialog { Filter = filter };
        if (dialog.ShowDialog() != DialogResult.OK) return;

        SetButtonsEnabled(false);
        _progressBar.Visible = true;
        _progressBar.Value = 0;
        _statusLabel.Text = "Importando...";

        var content = await File.ReadAllTextAsync(dialog.FileName);

        var result = format == "csv"
            ? CsvCustomerImporter.Import(content)
            : JsonCustomerImporter.Import(content);

        _progressBar.Maximum = result.Customers.Count > 0 ? result.Customers.Count : 1;

        var merged = 0;
        foreach (var customer in result.Customers)
        {
            if (customer.Dni is not null)
                _customers[customer.Dni] = customer;

            merged++;
            _progressBar.Value = merged;
            await Task.Delay(1);
        }

        RefreshGrid();

        _progressBar.Visible = false;
        SetButtonsEnabled(true);

        if (result.Errors.Count > 0)
        {
            var errorSummary = string.Join(Environment.NewLine, result.Errors.Take(20));
            if (result.Errors.Count > 20)
                errorSummary += $"{Environment.NewLine}... y {result.Errors.Count - 20} errores más.";

            MessageBox.Show(errorSummary, "Errores de importación",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        _statusLabel.Text = $"Importados {merged} clientes. Total: {_customers.Count}.";
    }

    private void OnAddClick(object? sender, EventArgs e)
    {
        _bindingList.Add(new Customer());
        _grid.CurrentCell = _grid.Rows[^1].Cells[0];
        _grid.BeginEdit(true);
    }

    private void OnDeleteClick(object? sender, EventArgs e)
    {
        if (_grid.SelectedRows.Count == 0) return;

        var toRemove = _grid.SelectedRows.Cast<DataGridViewRow>()
            .Select(r => r.DataBoundItem as Customer)
            .Where(c => c?.Dni is not null)
            .ToList();

        foreach (var customer in toRemove)
        {
            _customers.Remove(customer!.Dni!);
            _bindingList.Remove(customer);
        }

        _statusLabel.Text = $"Eliminados {toRemove.Count} clientes. Total: {_customers.Count}.";
    }

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        SyncBindingListToDict();
        _store.Save(_customers);
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
    }

    private void SetButtonsEnabled(bool enabled)
    {
        _btnImportCsv.Enabled = enabled;
        _btnImportJson.Enabled = enabled;
        _btnAdd.Enabled = enabled;
        _btnDelete.Enabled = enabled;
    }
}
