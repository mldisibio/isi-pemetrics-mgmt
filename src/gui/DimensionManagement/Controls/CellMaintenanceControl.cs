using System.Collections.Immutable;
using DimensionManagement.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using PEMetrics.DataApi.Models;
using PEMetrics.DataApi.Ports;

namespace DimensionManagement.Controls;

/// <summary>User control for Cell dimension maintenance.</summary>
public sealed class CellMaintenanceControl : UserControl
{
    static readonly Font MonoFont = new("Lucida Console", 8f);

    readonly IServiceProvider _services;
    readonly ForReadingPEMetricsDimensions _queryRepo;
    readonly ForManagingCells _cellRepo;

    // Top toolbar
    readonly CheckBox _activeOnlyCheckbox;
    readonly Button _newCellButton;

    // Grid
    readonly DataGridView _grid;
    ImmutableList<Cell> _allCells = [];

    // Detail panel
    readonly Panel _detailPanel;
    readonly Label _detailHeader;
    readonly TextBox _cellIdTextBox;
    readonly TextBox _cellNameTextBox;
    readonly TextBox _displayNameTextBox;
    readonly TextBox _activeFromTextBox;
    readonly TextBox _activeToTextBox;
    readonly TextBox _descriptionTextBox;
    readonly TextBox _altNamesTextBox;
    readonly Button _saveButton;
    readonly Button _cancelButton;

    Cell? _editingCell;
    bool _isNewCell;

    public CellMaintenanceControl(IServiceProvider services)
    {
        _services = services;
        _queryRepo = services.GetRequiredService<ForReadingPEMetricsDimensions>();
        _cellRepo = services.GetRequiredService<ForManagingCells>();

        // Toolbar panel
        var toolbarPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 40,
            Padding = new Padding(5)
        };

        _activeOnlyCheckbox = new CheckBox
        {
            Text = "Active Only",
            Checked = true,
            AutoSize = true,
            Location = new Point(10, 10)
        };
        _activeOnlyCheckbox.CheckedChanged += (s, e) => ApplyFilter();

        _newCellButton = new Button
        {
            Text = "+ New Cell",
            AutoSize = true,
            Location = new Point(120, 6)
        };
        _newCellButton.Click += NewCellButton_Click;

        toolbarPanel.Controls.Add(_activeOnlyCheckbox);
        toolbarPanel.Controls.Add(_newCellButton);

        // Data grid
        _grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoGenerateColumns = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            Font = MonoFont
        };
        var dateStyle = new DataGridViewCellStyle
        {
            Format = "yyyy-MM-dd",
            Alignment = DataGridViewContentAlignment.MiddleCenter
        };
        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "CellId", HeaderText = "ID", DataPropertyName = "CellId", Width = 50, FillWeight = 10 },
            new DataGridViewTextBoxColumn { Name = "CellName", HeaderText = "Cell Name", DataPropertyName = "CellName", FillWeight = 20 },
            new DataGridViewTextBoxColumn { Name = "DisplayName", HeaderText = "Display Name", DataPropertyName = "DisplayName", FillWeight = 20 },
            new DataGridViewTextBoxColumn { Name = "ActiveFrom", HeaderText = "Active From", DataPropertyName = "ActiveFrom", Width = 100, FillWeight = 15, DefaultCellStyle = dateStyle },
            new DataGridViewTextBoxColumn { Name = "ActiveTo", HeaderText = "Active To", DataPropertyName = "ActiveTo", Width = 100, FillWeight = 15, DefaultCellStyle = dateStyle },
            new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "Description", DataPropertyName = "Description", FillWeight = 30 }
        );
        _grid.CellDoubleClick += Grid_CellDoubleClick;

        // Detail panel (initially hidden)
        _detailPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 240,
            Visible = false,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(10)
        };

        _detailHeader = new Label
        {
            Text = "Edit Cell",
            Font = new Font(Font.FontFamily, 10, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(10, 10)
        };

        // Row 1: CellId (readonly), CellName
        var cellIdLabel = new Label { Text = "Cell ID:", Location = new Point(10, 40), AutoSize = true };
        _cellIdTextBox = new TextBox { Location = new Point(90, 37), Width = 80, ReadOnly = true, BackColor = SystemColors.Control, Font = MonoFont };

        var cellNameLabel = new Label { Text = "Cell Name:", Location = new Point(200, 40), AutoSize = true };
        _cellNameTextBox = new TextBox { Location = new Point(290, 37), Width = 200, Font = MonoFont };

        // Row 2: DisplayName
        var displayNameLabel = new Label { Text = "Display Name:", Location = new Point(10, 70), AutoSize = true };
        _displayNameTextBox = new TextBox { Location = new Point(90, 67), Width = 200, Font = MonoFont };

        // Row 3: ActiveFrom, ActiveTo (text boxes with YYYY-MM-DD format)
        var activeFromLabel = new Label { Text = "Active From:", Location = new Point(10, 100), AutoSize = true };
        _activeFromTextBox = new TextBox { Location = new Point(90, 97), Width = 100, Font = MonoFont };

        var activeToLabel = new Label { Text = "Active To:", Location = new Point(220, 100), AutoSize = true };
        _activeToTextBox = new TextBox { Location = new Point(290, 97), Width = 100, Font = MonoFont };
        var activeToHint = new Label { Text = "(blank if active)", Location = new Point(400, 100), AutoSize = true, ForeColor = SystemColors.GrayText };

        // Row 4: Description
        var descriptionLabel = new Label { Text = "Description:", Location = new Point(10, 130), AutoSize = true };
        _descriptionTextBox = new TextBox { Location = new Point(90, 127), Width = 500, Font = MonoFont };

        // Row 5: Alternative Names
        var altNamesLabel = new Label { Text = "Alt Names:", Location = new Point(10, 160), AutoSize = true };
        _altNamesTextBox = new TextBox { Location = new Point(90, 157), Width = 500, Font = MonoFont };

        // Buttons
        _cancelButton = new Button { Text = "Cancel", Location = new Point(420, 187), Width = 80 };
        _cancelButton.Click += (s, e) => HideDetailPanel();

        _saveButton = new Button { Text = "Save", Location = new Point(510, 187), Width = 80 };
        _saveButton.Click += SaveButton_Click;

        _detailPanel.Controls.AddRange(new Control[]
        {
            _detailHeader,
            cellIdLabel, _cellIdTextBox,
            cellNameLabel, _cellNameTextBox,
            displayNameLabel, _displayNameTextBox,
            activeFromLabel, _activeFromTextBox,
            activeToLabel, _activeToTextBox, activeToHint,
            descriptionLabel, _descriptionTextBox,
            altNamesLabel, _altNamesTextBox,
            _cancelButton, _saveButton
        });

        // Add controls in correct order (bottom to top for docking)
        Controls.Add(_grid);
        Controls.Add(_detailPanel);
        Controls.Add(toolbarPanel);
    }

    public async Task LoadDataAsync(int? selectCellId = null)
    {
        _allCells = await _queryRepo.GetCellsAsync();
        ApplyFilter(selectCellId);
    }

    void ApplyFilter(int? selectCellId = null)
    {
        // Preserve current sort state
        var sortColumn = _grid.SortedColumn;
        var sortOrder = _grid.SortOrder;

        // Capture current selection if no override provided
        var cellIdToSelect = selectCellId
            ?? (_grid.CurrentRow?.DataBoundItem as Cell)?.CellId;

        var filtered = _activeOnlyCheckbox.Checked
            ? _allCells.Where(c => c.IsActive).ToList()
            : _allCells.ToList();

        _grid.DataSource = new SortableBindingList<Cell>(filtered);

        // Re-apply sort if one was active
        if (sortColumn != null && sortOrder != SortOrder.None)
        {
            var direction = sortOrder == SortOrder.Ascending
                ? System.ComponentModel.ListSortDirection.Ascending
                : System.ComponentModel.ListSortDirection.Descending;
            _grid.Sort(sortColumn, direction);
        }

        // Re-select the target row
        SelectRowByCellId(cellIdToSelect);
    }

    void SelectRowByCellId(int? cellId)
    {
        if (_grid.Rows.Count == 0) return;

        // Find and select the row with matching CellId
        if (cellId.HasValue)
        {
            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (row.DataBoundItem is Cell cell && cell.CellId == cellId.Value)
                {
                    _grid.ClearSelection();
                    row.Selected = true;
                    _grid.CurrentCell = row.Cells[0];
                    return;
                }
            }
        }

        // Fallback: select first row
        _grid.ClearSelection();
        _grid.Rows[0].Selected = true;
        _grid.CurrentCell = _grid.Rows[0].Cells[0];
    }

    void Grid_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;

        if (_grid.Rows[e.RowIndex].DataBoundItem is Cell cell)
            ShowEditPanel(cell);
    }

    void NewCellButton_Click(object? sender, EventArgs e)
    {
        ShowNewPanel();
    }

    void ShowEditPanel(Cell cell)
    {
        _isNewCell = false;
        _editingCell = cell;

        _detailHeader.Text = $"Edit Cell (ID: {cell.CellId})";
        _cellIdTextBox.Text = cell.CellId.ToString();
        _cellNameTextBox.Text = cell.CellName;
        _displayNameTextBox.Text = cell.DisplayName;
        _activeFromTextBox.Text = cell.ActiveFrom.ToString("yyyy-MM-dd");
        _activeToTextBox.Text = cell.ActiveTo?.ToString("yyyy-MM-dd") ?? "";
        _descriptionTextBox.Text = cell.Description ?? "";
        _altNamesTextBox.Text = cell.AlternativeNames ?? "";

        _detailPanel.Visible = true;
    }

    void ShowNewPanel()
    {
        _isNewCell = true;
        _editingCell = null;

        _detailHeader.Text = "New Cell";
        _cellIdTextBox.Text = "(auto)";
        _cellNameTextBox.Text = "";
        _displayNameTextBox.Text = "";
        _activeFromTextBox.Text = DateTime.Today.ToString("yyyy-MM-dd");
        _activeToTextBox.Text = "";
        _descriptionTextBox.Text = "";
        _altNamesTextBox.Text = "";

        _detailPanel.Visible = true;
        _cellNameTextBox.Focus();
    }

    void HideDetailPanel()
    {
        _detailPanel.Visible = false;
        _editingCell = null;
    }

    async void SaveButton_Click(object? sender, EventArgs e)
    {
        // Basic validation
        if (string.IsNullOrWhiteSpace(_cellNameTextBox.Text))
        {
            MessageBox.Show("Cell Name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _cellNameTextBox.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(_displayNameTextBox.Text))
        {
            MessageBox.Show("Display Name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _displayNameTextBox.Focus();
            return;
        }

        // Parse ActiveFrom date
        if (!DateOnly.TryParse(_activeFromTextBox.Text.Trim(), out var activeFrom))
        {
            MessageBox.Show("Active From must be a valid date (YYYY-MM-DD).", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _activeFromTextBox.Focus();
            return;
        }

        // Parse ActiveTo date (optional)
        DateOnly? activeTo = null;
        if (!string.IsNullOrWhiteSpace(_activeToTextBox.Text))
        {
            if (!DateOnly.TryParse(_activeToTextBox.Text.Trim(), out var parsedActiveTo))
            {
                MessageBox.Show("Active To must be a valid date (YYYY-MM-DD) or blank.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _activeToTextBox.Focus();
                return;
            }
            activeTo = parsedActiveTo;
        }

        var cell = new Cell
        {
            CellId = _isNewCell ? 0 : _editingCell!.CellId,
            CellName = _cellNameTextBox.Text.Trim(),
            DisplayName = _displayNameTextBox.Text.Trim(),
            ActiveFrom = activeFrom,
            ActiveTo = activeTo,
            Description = string.IsNullOrWhiteSpace(_descriptionTextBox.Text) ? null : _descriptionTextBox.Text.Trim(),
            AlternativeNames = string.IsNullOrWhiteSpace(_altNamesTextBox.Text) ? null : _altNamesTextBox.Text.Trim()
        };

        try
        {
            _saveButton.Enabled = false;
            _cancelButton.Enabled = false;

            if (_isNewCell)
            {
                var newId = await _cellRepo.InsertAsync(cell);
                if (newId > 0)
                {
                    HideDetailPanel();
                    await Task.Delay(500);
                    await LoadDataAsync(newId);
                }
                else
                {
                    MessageBox.Show("Failed to create cell. Check the status bar for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                var success = await _cellRepo.UpdateAsync(cell);
                if (success)
                {
                    HideDetailPanel();
                    await Task.Delay(500);
                    await LoadDataAsync(cell.CellId);
                }
                else
                {
                    MessageBox.Show("Failed to update cell. Check the status bar for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        finally
        {
            _saveButton.Enabled = true;
            _cancelButton.Enabled = true;
        }
    }
}
