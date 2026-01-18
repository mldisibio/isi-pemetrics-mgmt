using System.Collections.Immutable;
using DimensionManagement.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using PEMetrics.DataApi.Models;
using PEMetrics.DataApi.Ports;

namespace DimensionManagement.Controls;

/// <summary>User control for PC-to-Cell mapping maintenance.</summary>
public sealed class CellByPCStationMaintenanceControl : UserControl
{
    static readonly Font MonoFont = new("Lucida Console", 8f);

    readonly ForReadingPEMetricsDimensions _queryRepo;
    readonly ForMappingPCStationToCell _mappingRepo;

    // Top toolbar
    readonly CheckBox _activeOnlyCheckbox;
    readonly Button _newMappingButton;

    // Grid
    readonly DataGridView _grid;
    ImmutableList<CellByPCStation> _allMappings = [];

    // Reference data for pickers
    ImmutableList<PCStation> _allPCStations = [];
    ImmutableList<Cell> _allCells = [];

    // Detail panel
    readonly Panel _detailPanel;
    readonly Label _detailHeader;
    readonly TextBox _mapIdTextBox;
    readonly TextBox _pcStationTextBox;
    readonly ListBox _pcStationSuggestions;
    readonly ComboBox _cellComboBox;
    readonly TextBox _purposeTextBox;
    readonly TextBox _extendedNameTextBox;
    readonly Button _generateExtNameButton;
    readonly TextBox _activeFromTextBox;
    readonly TextBox _activeToTextBox;
    readonly Button _saveButton;
    readonly Button _cancelButton;

    CellByPCStation? _editingMapping;
    bool _isNewMapping;
    bool _suppressPcSuggestions;

    public CellByPCStationMaintenanceControl(IServiceProvider services)
    {
        _queryRepo = services.GetRequiredService<ForReadingPEMetricsDimensions>();
        _mappingRepo = services.GetRequiredService<ForMappingPCStationToCell>();

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

        _newMappingButton = new Button
        {
            Text = "+ New Mapping",
            AutoSize = true,
            Location = new Point(120, 6)
        };
        _newMappingButton.Click += NewMappingButton_Click;

        toolbarPanel.Controls.Add(_activeOnlyCheckbox);
        toolbarPanel.Controls.Add(_newMappingButton);

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
            Font = MonoFont,
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
        };
        var dateStyle = new DataGridViewCellStyle
        {
            Format = "yyyy-MM-dd",
            Alignment = DataGridViewContentAlignment.MiddleCenter
        };
        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "StationMapId", HeaderText = "ID", DataPropertyName = "StationMapId", Width = 40, FillWeight = 5 },
            new DataGridViewTextBoxColumn { Name = "PcName", HeaderText = "PC Name", DataPropertyName = "PcName", FillWeight = 12 },
            new DataGridViewTextBoxColumn { Name = "CellName", HeaderText = "Cell", DataPropertyName = "CellName", FillWeight = 10 },
            new DataGridViewTextBoxColumn { Name = "PcPurpose", HeaderText = "Purpose", DataPropertyName = "PcPurpose", FillWeight = 20 },
            new DataGridViewTextBoxColumn { Name = "ActiveFrom", HeaderText = "Active From", DataPropertyName = "ActiveFrom", Width = 80, FillWeight = 10, DefaultCellStyle = dateStyle },
            new DataGridViewTextBoxColumn { Name = "ActiveTo", HeaderText = "Active To", DataPropertyName = "ActiveTo", Width = 80, FillWeight = 10, DefaultCellStyle = dateStyle },
            new DataGridViewTextBoxColumn { Name = "ExtendedName", HeaderText = "Ext. Name", DataPropertyName = "ExtendedName", FillWeight = 33 }
        );
        _grid.CellDoubleClick += Grid_CellDoubleClick;

        // Detail panel (initially hidden)
        _detailPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 260,
            Visible = false,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(10)
        };

        _detailHeader = new Label
        {
            Text = "Edit Mapping",
            Font = new Font(Font.FontFamily, 10, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(10, 10)
        };

        // Row 1: MapId (readonly), PC Station (type-ahead)
        var mapIdLabel = new Label { Text = "Map ID:", Location = new Point(10, 45), AutoSize = true };
        _mapIdTextBox = new TextBox { Location = new Point(75, 42), Width = 80, ReadOnly = true, BackColor = SystemColors.Control, Font = MonoFont };

        var pcStationLabel = new Label { Text = "PC Station:", Location = new Point(180, 45), AutoSize = true };
        _pcStationTextBox = new TextBox { Location = new Point(260, 42), Width = 200, Font = MonoFont };
        _pcStationTextBox.TextChanged += PcStationTextBox_TextChanged;
        _pcStationTextBox.KeyDown += PcStationTextBox_KeyDown;
        _pcStationTextBox.Leave += PcStationTextBox_Leave;

        _pcStationSuggestions = new ListBox
        {
            Location = new Point(260, 67),
            Width = 200,
            Height = 90,
            Font = MonoFont,
            Visible = false
        };
        _pcStationSuggestions.Click += PcStationSuggestions_Click;
        _pcStationSuggestions.KeyDown += PcStationSuggestions_KeyDown;

        // Row 2: Cell (dropdown)
        var cellLabel = new Label { Text = "Cell:", Location = new Point(10, 75), AutoSize = true };
        _cellComboBox = new ComboBox
        {
            Location = new Point(75, 72),
            Width = 200,
            Font = MonoFont,
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        // Row 3: Purpose
        var purposeLabel = new Label { Text = "Purpose:", Location = new Point(10, 105), AutoSize = true };
        _purposeTextBox = new TextBox { Location = new Point(75, 102), Width = 385, Font = MonoFont };

        // Row 4: Extended Name with generate button
        var extendedNameLabel = new Label { Text = "Ext. Name:", Location = new Point(10, 135), AutoSize = true };
        _extendedNameTextBox = new TextBox { Location = new Point(75, 132), Width = 350, Font = MonoFont };
        _generateExtNameButton = new Button
        {
            Text = "↻",
            Location = new Point(430, 131),
            Width = 28,
            Height = 23,
            Font = new Font(Font.FontFamily, 9f)
        };
        _generateExtNameButton.Click += GenerateExtNameButton_Click;
        var extNameHint = new Label { Text = "(generate)", Location = new Point(460, 135), AutoSize = true, ForeColor = SystemColors.GrayText };

        // Row 5: ActiveFrom, ActiveTo
        var activeFromLabel = new Label { Text = "Active From:", Location = new Point(10, 165), AutoSize = true };
        _activeFromTextBox = new TextBox { Location = new Point(90, 162), Width = 100, Font = MonoFont };

        var activeToLabel = new Label { Text = "Active To:", Location = new Point(220, 165), AutoSize = true };
        _activeToTextBox = new TextBox { Location = new Point(290, 162), Width = 100, Font = MonoFont };
        var activeToHint = new Label { Text = "(blank if active)", Location = new Point(400, 165), AutoSize = true, ForeColor = SystemColors.GrayText };

        // Buttons
        _cancelButton = new Button { Text = "Cancel", Location = new Point(300, 195), Width = 80 };
        _cancelButton.Click += (s, e) => HideDetailPanel();

        _saveButton = new Button { Text = "Save", Location = new Point(390, 195), Width = 80 };
        _saveButton.Click += SaveButton_Click;

        _detailPanel.Controls.AddRange(
        [
            _detailHeader,
            mapIdLabel, _mapIdTextBox,
            pcStationLabel, _pcStationTextBox, _pcStationSuggestions,
            cellLabel, _cellComboBox,
            purposeLabel, _purposeTextBox,
            extendedNameLabel, _extendedNameTextBox, _generateExtNameButton, extNameHint,
            activeFromLabel, _activeFromTextBox,
            activeToLabel, _activeToTextBox, activeToHint,
            _cancelButton, _saveButton
        ]);

        // Add controls in correct order (bottom to top for docking)
        Controls.Add(_grid);
        Controls.Add(_detailPanel);
        Controls.Add(toolbarPanel);
    }

    public async Task LoadDataAsync(int? selectMapId = null)
    {
        // Load reference data for pickers
        var pcStationsTask = _queryRepo.GetPCStationsAsync();
        var cellsTask = _queryRepo.GetCellsAsync();
        var mappingsTask = _queryRepo.GetPcToCellMappingsAsync();

        await Task.WhenAll(pcStationsTask, cellsTask, mappingsTask).ConfigureAwait(true);

        _allPCStations = pcStationsTask.Result;
        _allCells = cellsTask.Result;
        _allMappings = mappingsTask.Result;

        // Populate Cell dropdown with active cells
        PopulateCellDropdown();

        ApplyFilter(selectMapId);
    }

    void PopulateCellDropdown()
    {
        _cellComboBox.Items.Clear();
        var activeCells = _allCells.Where(c => c.IsActive).OrderBy(c => c.CellName);
        foreach (var cell in activeCells)
        {
            _cellComboBox.Items.Add(new CellItem(cell));
        }
    }

    void ApplyFilter(int? selectMapId = null)
    {
        // Preserve current sort state
        var sortColumn = _grid.SortedColumn;
        var sortOrder = _grid.SortOrder;

        // Capture current selection if no override provided
        var idToSelect = selectMapId
            ?? (_grid.CurrentRow?.DataBoundItem as CellByPCStation)?.StationMapId;

        var filtered = _activeOnlyCheckbox.Checked
            ? _allMappings.Where(m => m.IsActive).ToList()
            : _allMappings.ToList();

        _grid.DataSource = new SortableBindingList<CellByPCStation>(filtered);

        // Re-apply sort if one was active
        if (sortColumn != null && sortOrder != SortOrder.None)
        {
            var direction = sortOrder == SortOrder.Ascending
                ? System.ComponentModel.ListSortDirection.Ascending
                : System.ComponentModel.ListSortDirection.Descending;
            _grid.Sort(sortColumn, direction);
        }

        // Re-select the target row
        SelectRowByMapId(idToSelect);
    }

    void SelectRowByMapId(int? mapId)
    {
        if (_grid.Rows.Count == 0) return;

        if (mapId.HasValue)
        {
            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (row.DataBoundItem is CellByPCStation mapping && mapping.StationMapId == mapId.Value)
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

        if (_grid.Rows[e.RowIndex].DataBoundItem is CellByPCStation mapping)
            ShowEditPanel(mapping);
    }

    void NewMappingButton_Click(object? sender, EventArgs e)
    {
        ShowNewPanel();
    }

    void ShowEditPanel(CellByPCStation mapping)
    {
        _isNewMapping = false;
        _editingMapping = mapping;

        _detailHeader.Text = $"Edit Mapping (ID: {mapping.StationMapId})";
        _mapIdTextBox.Text = mapping.StationMapId.ToString();

        _suppressPcSuggestions = true;
        _pcStationTextBox.Text = mapping.PcName;
        _suppressPcSuggestions = false;
        _pcStationSuggestions.Visible = false;

        // Select the cell in dropdown
        SelectCellInDropdown(mapping.CellId);

        _purposeTextBox.Text = mapping.PcPurpose ?? "";
        _extendedNameTextBox.Text = mapping.ExtendedName ?? "";
        _activeFromTextBox.Text = mapping.ActiveFrom.ToString("yyyy-MM-dd");
        _activeToTextBox.Text = mapping.ActiveTo?.ToString("yyyy-MM-dd") ?? "";

        _detailPanel.Visible = true;
    }

    void ShowNewPanel()
    {
        _isNewMapping = true;
        _editingMapping = null;

        _detailHeader.Text = "New Mapping";
        _mapIdTextBox.Text = "(auto)";

        _suppressPcSuggestions = true;
        _pcStationTextBox.Text = "";
        _suppressPcSuggestions = false;
        _pcStationSuggestions.Visible = false;

        _cellComboBox.SelectedIndex = _cellComboBox.Items.Count > 0 ? 0 : -1;
        _purposeTextBox.Text = "";
        _extendedNameTextBox.Text = "";
        _activeFromTextBox.Text = DateTime.Today.ToString("yyyy-MM-dd");
        _activeToTextBox.Text = "";

        _detailPanel.Visible = true;
        _pcStationTextBox.Focus();
    }

    void SelectCellInDropdown(int cellId)
    {
        for (int i = 0; i < _cellComboBox.Items.Count; i++)
        {
            if (_cellComboBox.Items[i] is CellItem item && item.CellId == cellId)
            {
                _cellComboBox.SelectedIndex = i;
                return;
            }
        }
        _cellComboBox.SelectedIndex = -1;
    }

    void HideDetailPanel()
    {
        _detailPanel.Visible = false;
        _pcStationSuggestions.Visible = false;
        _editingMapping = null;
    }

    void GenerateExtNameButton_Click(object? sender, EventArgs e)
    {
        var pcName = _pcStationTextBox.Text.Trim();
        var purpose = _purposeTextBox.Text.Trim();

        var extName = string.IsNullOrEmpty(purpose)
            ? pcName
            : $"{pcName} - [{purpose}]";

        _extendedNameTextBox.Text = extName;
        _extendedNameTextBox.Focus();
    }

    #region PC Station Type-Ahead

    void PcStationTextBox_TextChanged(object? sender, EventArgs e)
    {
        if (_suppressPcSuggestions) return;

        var text = _pcStationTextBox.Text.Trim();
        if (string.IsNullOrEmpty(text))
        {
            _pcStationSuggestions.Visible = false;
            return;
        }

        // Filter PC stations: contains match, sorted alphabetically, top 5
        var matches = _allPCStations
            .Where(s => s.PcName.StartsWith(text, StringComparison.OrdinalIgnoreCase))
            .OrderBy(s => s.PcName)
            .Take(5)
            .ToList();

        if (matches.Count == 0)
        {
            _pcStationSuggestions.Visible = false;
            return;
        }

        _pcStationSuggestions.BeginUpdate();
        _pcStationSuggestions.Items.Clear();
        foreach (var station in matches)
        {
            _pcStationSuggestions.Items.Add(station.PcName);
        }
        _pcStationSuggestions.EndUpdate();

        _pcStationSuggestions.Visible = true;
        _pcStationSuggestions.BringToFront();
    }

    void PcStationTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (!_pcStationSuggestions.Visible) return;

        if (e.KeyCode == Keys.Down)
        {
            _pcStationSuggestions.Focus();
            if (_pcStationSuggestions.Items.Count > 0)
                _pcStationSuggestions.SelectedIndex = 0;
            e.Handled = true;
        }
        else if (e.KeyCode == Keys.Enter)
        {
            // Accept first suggestion if available
            if (_pcStationSuggestions.Items.Count > 0)
            {
                AcceptPcSuggestion(_pcStationSuggestions.Items[0]?.ToString() ?? "");
            }
            e.Handled = true;
            e.SuppressKeyPress = true;
        }
        else if (e.KeyCode == Keys.Escape)
        {
            _pcStationSuggestions.Visible = false;
            e.Handled = true;
        }
    }

    void PcStationTextBox_Leave(object? sender, EventArgs e)
    {
        // Delay hiding to allow click on suggestion list
        BeginInvoke(() =>
        {
            if (!_pcStationSuggestions.Focused)
                _pcStationSuggestions.Visible = false;
        });
    }

    void PcStationSuggestions_Click(object? sender, EventArgs e)
    {
        if (_pcStationSuggestions.SelectedItem is string pcName)
        {
            AcceptPcSuggestion(pcName);
        }
    }

    void PcStationSuggestions_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter && _pcStationSuggestions.SelectedItem is string pcName)
        {
            AcceptPcSuggestion(pcName);
            e.Handled = true;
            e.SuppressKeyPress = true;
        }
        else if (e.KeyCode == Keys.Escape)
        {
            _pcStationSuggestions.Visible = false;
            _pcStationTextBox.Focus();
            e.Handled = true;
        }
    }

    void AcceptPcSuggestion(string pcName)
    {
        _suppressPcSuggestions = true;
        _pcStationTextBox.Text = pcName;
        _suppressPcSuggestions = false;
        _pcStationSuggestions.Visible = false;
        _cellComboBox.Focus();
    }

    #endregion

    async void SaveButton_Click(object? sender, EventArgs e)
    {
        // Validate PC Station
        var pcName = _pcStationTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(pcName))
        {
            MessageBox.Show("PC Station is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _pcStationTextBox.Focus();
            return;
        }

        // Verify PC Station exists
        var pcStation = _allPCStations.FirstOrDefault(s => s.PcName.Equals(pcName, StringComparison.OrdinalIgnoreCase));
        if (pcStation == null)
        {
            MessageBox.Show($"PC Station '{pcName}' not found. Please select from the suggestions.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _pcStationTextBox.Focus();
            return;
        }

        // Validate Cell selection
        if (_cellComboBox.SelectedItem is not CellItem selectedCell)
        {
            MessageBox.Show("Cell is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _cellComboBox.Focus();
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

        var mapping = new CellByPCStation
        {
            StationMapId = _isNewMapping ? 0 : _editingMapping!.StationMapId,
            CellId = selectedCell.CellId,
            PcName = pcStation.PcName,
            PcPurpose = string.IsNullOrWhiteSpace(_purposeTextBox.Text) ? null : _purposeTextBox.Text.Trim(),
            ExtendedName = string.IsNullOrWhiteSpace(_extendedNameTextBox.Text) ? null : _extendedNameTextBox.Text.Trim(),
            ActiveFrom = activeFrom,
            ActiveTo = activeTo
        };

        try
        {
            _saveButton.Enabled = false;
            _cancelButton.Enabled = false;

            if (_isNewMapping)
            {
                var newId = await _mappingRepo.InsertAsync(mapping);
                if (newId > 0)
                {
                    HideDetailPanel();
                    await LoadDataAsync(newId);
                }
                else
                {
                    MessageBox.Show("Failed to create mapping. Check the status bar for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                var success = await _mappingRepo.UpdateAsync(mapping);
                if (success)
                {
                    HideDetailPanel();
                    await LoadDataAsync(mapping.StationMapId);
                }
                else
                {
                    MessageBox.Show("Failed to update mapping. Check the status bar for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        finally
        {
            _saveButton.Enabled = true;
            _cancelButton.Enabled = true;
        }
    }

    /// <summary>Helper class for Cell dropdown items.</summary>
    sealed class CellItem(Cell cell)
    {
        public int CellId => cell.CellId;
        public override string ToString() => cell.CellName;
    }
}
