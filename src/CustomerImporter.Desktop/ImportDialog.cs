using CustomerImporter.Core.Models;

namespace CustomerImporter.Desktop;

public class ImportDialog : Form
{
    private readonly ProgressBar _progressBar = new();
    private readonly Label _statusLabel = new();
    private readonly ListView _resultList = new();
    private readonly Button _btnAccept = new();
    private readonly Button _btnCancel = new();

    public bool Accepted { get; private set; }

    public ImportDialog()
    {
        InitializeLayout();
    }

    private void InitializeLayout()
    {
        Text = "Previsualización de Importación";
        Size = new Size(960, 540);
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(700, 400);
        MaximizeBox = false;
        MinimizeBox = false;
        Font = new Font("Segoe UI", 9.5f);

        // --- Header ---
        _statusLabel.Dock = DockStyle.Top;
        _statusLabel.Height = 44;
        _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        _statusLabel.Padding = new Padding(14, 0, 14, 0);
        _statusLabel.Text = "Analizando fichero...";
        _statusLabel.Font = new Font("Segoe UI Semibold", 11f);
        _statusLabel.BackColor = Color.FromArgb(245, 247, 250);

        // --- ProgressBar ---
        _progressBar.Dock = DockStyle.Top;
        _progressBar.Height = 8;
        _progressBar.Style = ProgressBarStyle.Continuous;

        // --- ListView ---
        _resultList.Dock = DockStyle.Fill;
        _resultList.View = View.Details;
        _resultList.FullRowSelect = true;
        _resultList.GridLines = true;
        _resultList.Font = new Font("Segoe UI", 9.25f);
        _resultList.Columns.Add("Fila", 50, HorizontalAlignment.Center);
        _resultList.Columns.Add("Estado", 80, HorizontalAlignment.Center);
        _resultList.Columns.Add("Datos", 360);
        _resultList.Columns.Add("Detalle", 240);
        _resultList.Resize += (_, _) => AdjustColumns();

        // --- Bottom panel ---
        var bottomPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 70,
            BackColor = Color.FromArgb(245, 247, 250)
        };

        _btnAccept.Text = "Importar";
        _btnAccept.Size = new Size(130, 38);
        _btnAccept.Font = new Font("Segoe UI Semibold", 9.5f);
        _btnAccept.TextAlign = ContentAlignment.MiddleCenter;
        _btnAccept.UseVisualStyleBackColor = true;
        _btnAccept.Enabled = false;
        _btnAccept.Click += (_, _) => { Accepted = true; Close(); };

        _btnCancel.Text = "Cancelar";
        _btnCancel.Size = new Size(130, 38);
        _btnCancel.Font = new Font("Segoe UI", 9.5f);
        _btnCancel.TextAlign = ContentAlignment.MiddleCenter;
        _btnCancel.UseVisualStyleBackColor = true;
        _btnCancel.Click += (_, _) => { Accepted = false; Close(); };

        bottomPanel.Controls.Add(_btnAccept);
        bottomPanel.Controls.Add(_btnCancel);

        Controls.Add(_resultList);
        Controls.Add(_progressBar);
        Controls.Add(_statusLabel);
        Controls.Add(bottomPanel);

        AcceptButton = _btnAccept;
        CancelButton = _btnCancel;

        Load += (_, _) => PositionButtons();
        Resize += (_, _) => PositionButtons();
    }

    private void PositionButtons()
    {
        var panel = _btnAccept.Parent!;
        int y = (panel.ClientSize.Height - _btnAccept.Height) / 2;

        _btnCancel.Left = panel.ClientSize.Width - _btnCancel.Width - 14;
        _btnCancel.Top = y;

        _btnAccept.Left = _btnCancel.Left - _btnAccept.Width - 10;
        _btnAccept.Top = y;
    }

    private void AdjustColumns()
    {
        if (_resultList.Columns.Count < 4) return;
        var available = _resultList.ClientSize.Width - 50 - 80;
        _resultList.Columns[2].Width = (int)(available * 0.55);
        _resultList.Columns[3].Width = (int)(available * 0.45);
    }

    public void ShowResult(ImportResult result)
    {
        _progressBar.Maximum = result.TotalRows > 0 ? result.TotalRows : 1;
        _progressBar.Value = 0;
        _resultList.Items.Clear();

        int successIdx = 0;
        int errorIdx = 0;

        for (int row = 1; row <= result.TotalRows; row++)
        {
            _progressBar.Value = row;

            if (errorIdx < result.Errors.Count && result.Errors[errorIdx].Row == row)
            {
                var err = result.Errors[errorIdx];
                var item = new ListViewItem(row.ToString());
                item.SubItems.Add("NO ENTRA");
                item.SubItems.Add(Truncate(err.RawData, 80));
                item.SubItems.Add(string.Join(" | ", err.Messages));
                item.BackColor = Color.FromArgb(255, 235, 235);
                item.ForeColor = Color.FromArgb(160, 30, 30);
                _resultList.Items.Add(item);
                errorIdx++;
            }
            else if (successIdx < result.Customers.Count)
            {
                var c = result.Customers[successIdx];
                var item = new ListViewItem(row.ToString());
                item.SubItems.Add("ENTRA");
                item.SubItems.Add($"{c.Dni}  —  {c.Nombre} {c.Apellidos}");
                item.SubItems.Add("");
                item.BackColor = Color.FromArgb(235, 250, 240);
                item.ForeColor = Color.FromArgb(30, 100, 50);
                _resultList.Items.Add(item);
                successIdx++;
            }
        }

        while (errorIdx < result.Errors.Count)
        {
            var err = result.Errors[errorIdx];
            var item = new ListViewItem("—");
            item.SubItems.Add("NO ENTRA");
            item.SubItems.Add(Truncate(err.RawData, 80));
            item.SubItems.Add(string.Join(" | ", err.Messages));
            item.BackColor = Color.FromArgb(255, 235, 235);
            item.ForeColor = Color.FromArgb(160, 30, 30);
            _resultList.Items.Add(item);
            errorIdx++;
        }

        _progressBar.Value = _progressBar.Maximum;

        var ok = result.Customers.Count;
        var fail = result.Errors.Count;

        if (fail == 0)
        {
            _statusLabel.Text = $"Se importarán {ok} cliente(s). ¿Desea continuar?";
            _statusLabel.ForeColor = Color.FromArgb(30, 130, 50);
        }
        else if (ok == 0)
        {
            _statusLabel.Text = $"No se puede importar: {fail} registro(s) con errores.";
            _statusLabel.ForeColor = Color.FromArgb(200, 50, 50);
        }
        else
        {
            _statusLabel.Text = $"Se importarán {ok} de {ok + fail} registros ({fail} con errores). ¿Desea continuar?";
            _statusLabel.ForeColor = Color.FromArgb(180, 120, 0);
        }

        _btnAccept.Enabled = ok > 0;
        _btnAccept.Focus();
    }

    private static string Truncate(string text, int max) =>
        text.Length <= max ? text : text[..max] + "...";
}
