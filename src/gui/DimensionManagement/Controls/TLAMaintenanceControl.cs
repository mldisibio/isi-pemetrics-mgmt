using System.Collections.Immutable;
using DimensionManagement.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using PEMetrics.DataApi.Models;
using PEMetrics.DataApi.Ports;

namespace DimensionManagement.Controls;

/// <summary>User control for Part Number (TLA) maintenance.</summary>
public sealed class TLAMaintenanceControl : UserControl
{
    static readonly Font MonoFont = new("Lucida Console", 8f);

    readonly ForReadingPEMetricsDimensions _queryRepo;
    readonly ForManagingPartNumbers _tlaRepo;
    readonly ForMappingPartNumberToCells _mappingRepo;

    // Top toolbar
    readonly CheckBox _inUseCheckbox;
    readonly CheckBox _unusedCheckbox;
    readonly TextBox _filterTextBox;
    readonly Button _newPartButton;

    // Grid
    readonly DataGridView _grid;
    ImmutableList<TLA> _allTLAs = [];
    ImmutableList<CellByPartNo> _allCellMappings = [];

    // Reference data
    ImmutableList<Cell> _allCells = [];

    // Detail panel
    readonly Panel _detailPanel;
    readonly Label _detailHeader;
    readonly TextBox _partNoTextBox;
    readonly TextBox _familyTextBox;
    readonly TextBox _subfamilyTextBox;
    readonly TextBox _serviceGroupTextBox;
    readonly TextBox _formalDescTextBox;
    readonly TextBox _descriptionTextBox;
    readonly Button _generateDescButton;
    readonly CheckedListBox _cellCheckedListBox;
    readonly Button _deleteButton;
    readonly Button _saveButton;
    readonly Button _cancelButton;

    TLA? _editingTLA;
    bool _isNewTLA;

    public TLAMaintenanceControl(IServiceProvider services)
    {
        _queryRepo = services.GetRequiredService<ForReadingPEMetricsDimensions>();
        _tlaRepo = services.GetRequiredService<ForManagingPartNumbers>();
        _mappingRepo = services.GetRequiredService<ForMappingPartNumberToCells>();

        // Toolbar panel
        var toolbarPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 40,
            Padding = new Padding(5)
        };

        _inUseCheckbox = new CheckBox
        {
            Text = "In Use",
            Checked = true,
            AutoSize = true,
            Location = new Point(10, 10)
        };
        _inUseCheckbox.CheckedChanged += InUseCheckbox_CheckedChanged;

        _unusedCheckbox = new CheckBox
        {
            Text = "Unused",
            Checked = false,
            AutoSize = true,
            Location = new Point(75, 10)
        };
        _unusedCheckbox.CheckedChanged += UnusedCheckbox_CheckedChanged;

        var filterLabel = new Label
        {
            Text = "Filter:",
            AutoSize = true,
            Location = new Point(160, 12)
        };

        _filterTextBox = new TextBox
        {
            Location = new Point(200, 8),
            Width = 150,
            Font = MonoFont
        };
        _filterTextBox.TextChanged += (s, e) => ApplyFilter();

        var clearFilterButton = new Button
        {
            Text = "×",
            Location = new Point(352, 7),
            Width = 23,
            Height = 23,
            Font = new Font(Font.FontFamily, 9f)
        };
        clearFilterButton.Click += (s, e) => { _filterTextBox.Clear(); _filterTextBox.Focus(); };

        _newPartButton = new Button
        {
            Text = "+ New Part Number",
            AutoSize = true,
            Location = new Point(390, 6)
        };
        _newPartButton.Click += NewPartButton_Click;

        toolbarPanel.Controls.AddRange([_inUseCheckbox, _unusedCheckbox, filterLabel, _filterTextBox, clearFilterButton, _newPartButton]);

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
        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "Cells", HeaderText = "Cells", DataPropertyName = "CellsDisplay", FillWeight = 15 },
            new DataGridViewTextBoxColumn { Name = "PartNo", HeaderText = "Part No", DataPropertyName = "PartNo", FillWeight = 10 },
            new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "Description", DataPropertyName = "Description", FillWeight = 35 },
            new DataGridViewTextBoxColumn { Name = "Family", HeaderText = "Family", DataPropertyName = "Family", FillWeight = 15 },
            new DataGridViewTextBoxColumn { Name = "Subfamily", HeaderText = "Subfamily", DataPropertyName = "Subfamily", FillWeight = 25 }
        );
        _grid.CellDoubleClick += Grid_CellDoubleClick;

        // Detail panel (initially hidden)
        _detailPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 340,
            Visible = false,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(10)
        };

        _detailHeader = new Label
        {
            Text = "Edit Part Number",
            Font = new Font(Font.FontFamily, 10, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(10, 10)
        };

        // Row 1: Part No
        var partNoLabel = new Label { Text = "Part No:", Location = new Point(10, 40), AutoSize = true };
        _partNoTextBox = new TextBox { Location = new Point(95, 37), Width = 150, Font = MonoFont };

        // Row 2: Family, Service Group
        var familyLabel = new Label { Text = "Family:", Location = new Point(10, 70), AutoSize = true };
        _familyTextBox = new TextBox { Location = new Point(95, 67), Width = 150, Font = MonoFont };

        var serviceGroupLabel = new Label { Text = "Service Grp:", Location = new Point(270, 70), AutoSize = true };
        _serviceGroupTextBox = new TextBox { Location = new Point(350, 67), Width = 150, Font = MonoFont };

        // Row 3: Subfamily
        var subfamilyLabel = new Label { Text = "Subfamily:", Location = new Point(10, 100), AutoSize = true };
        _subfamilyTextBox = new TextBox { Location = new Point(95, 97), Width = 500, Font = MonoFont };

        // Row 4: Formal Description (multiline, 2 lines)
        var formalDescLabel = new Label { Text = "Formal Desc:", Location = new Point(10, 130), AutoSize = true };
        _formalDescTextBox = new TextBox
        {
            Location = new Point(95, 127),
            Width = 500,
            Height = 38,
            Font = MonoFont,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical
        };

        // Row 5: Description with generate button
        var descriptionLabel = new Label { Text = "Description:", Location = new Point(10, 178), AutoSize = true };
        _descriptionTextBox = new TextBox { Location = new Point(95, 175), Width = 468, Font = MonoFont };
        _generateDescButton = new Button
        {
            Text = "↻",
            Location = new Point(570, 174),
            Width = 28,
            Height = 23,
            Font = new Font(Font.FontFamily, 9f)
        };
        _generateDescButton.Click += GenerateDescButton_Click;

        // Row 6: Cells (CheckedListBox)
        var cellsLabel = new Label { Text = "Cells:", Location = new Point(10, 208), AutoSize = true };
        _cellCheckedListBox = new CheckedListBox
        {
            Location = new Point(95, 205),
            Width = 200,
            Height = 75,
            Font = MonoFont,
            CheckOnClick = true
        };

        // Buttons
        _deleteButton = new Button { Text = "Delete", Location = new Point(220, 288), Width = 80 };
        _deleteButton.Click += DeleteButton_Click;

        _cancelButton = new Button { Text = "Cancel", Location = new Point(310, 288), Width = 80 };
        _cancelButton.Click += (s, e) => HideDetailPanel();

        _saveButton = new Button { Text = "Save", Location = new Point(400, 288), Width = 80 };
        _saveButton.Click += SaveButton_Click;

        _detailPanel.Controls.AddRange(
        [
            _detailHeader,
            partNoLabel, _partNoTextBox,
            familyLabel, _familyTextBox,
            serviceGroupLabel, _serviceGroupTextBox,
            subfamilyLabel, _subfamilyTextBox,
            formalDescLabel, _formalDescTextBox,
            descriptionLabel, _descriptionTextBox, _generateDescButton,
            cellsLabel, _cellCheckedListBox,
            _deleteButton, _cancelButton, _saveButton
        ]);

        // Add controls in correct order (bottom to top for docking)
        Controls.Add(_grid);
        Controls.Add(_detailPanel);
        Controls.Add(toolbarPanel);
    }


    public async Task RefreshLookupsAsync()
    {
        var cellsTask = _queryRepo.GetCellsAsync();
        await Task.WhenAll(cellsTask).ConfigureAwait(true);
        _allCells = cellsTask.Result;
    }

    public async Task LoadDataAsync(string? selectPartNo = null)
    {
        var tlasTask = _queryRepo.GetTLACatalogAsync();
        var cellMappingsTask = _queryRepo.GetTLAToCellMappingsAsync();
        var refreshLookupsTask = RefreshLookupsAsync();

        await Task.WhenAll(tlasTask, cellMappingsTask, refreshLookupsTask).ConfigureAwait(true);

        _allTLAs = tlasTask.Result;
        _allCellMappings = cellMappingsTask.Result
            .GroupBy(m => m.PartNo)
            .SelectMany(g => g.Select(m => new CellByPartNo
            {
                PartNo = m.PartNo,
                CellId = m.CellId,
                CellName = m.CellName
            }))
            .ToImmutableList();

        ApplyFilter(selectPartNo);
    }

    void ApplyFilter(string? selectPartNo = null)
    {
        // Preserve current sort state
        var sortColumn = _grid.SortedColumn;
        var sortOrder = _grid.SortOrder;

        // Capture current selection if no override provided
        var partNoToSelect = selectPartNo
            ?? (_grid.CurrentRow?.DataBoundItem as TLAGridItem)?.PartNo;

        var filterText = _filterTextBox.Text.Trim();

        // Filter by In Use / Unused checkboxes
        // Both checked = show all, only In Use = show used, only Unused = show unused
        var showInUse = _inUseCheckbox.Checked;
        var showUnused = _unusedCheckbox.Checked;

        var filtered = _allTLAs
            .Where(t => (showInUse && showUnused) || (showInUse && t.IsUsed) || (showUnused && !t.IsUsed))
            .Where(t => string.IsNullOrEmpty(filterText) ||
                        (t.PartNo.Contains(filterText, StringComparison.OrdinalIgnoreCase)) ||
                        (t.Family?.Contains(filterText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (t.Subfamily?.Contains(filterText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (t.FormalDescription?.Contains(filterText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (t.Description?.Contains(filterText, StringComparison.OrdinalIgnoreCase) ?? false))
            .Select(t => new TLAGridItem(t, GetCellsDisplay(t.PartNo)))
            .ToList();

        _grid.DataSource = new SortableBindingList<TLAGridItem>(filtered);

        // Re-apply sort if one was active
        if (sortColumn != null && sortOrder != SortOrder.None)
        {
            var newColumn = _grid.Columns[sortColumn.Name];
            if (newColumn != null)
            {
                var direction = sortOrder == SortOrder.Ascending
                    ? System.ComponentModel.ListSortDirection.Ascending
                    : System.ComponentModel.ListSortDirection.Descending;
                _grid.Sort(newColumn, direction);
            }
        }

        // Re-select the target row
        SelectRowByPartNo(partNoToSelect);
    }

    string GetCellsDisplay(string partNo)
    {
        var cells = _allCellMappings
            .Where(m => m.PartNo == partNo)
            .Select(m => m.CellName)
            .Where(n => n != null)
            .OrderBy(n => n);
        return string.Join(", ", cells);
    }

    void SelectRowByPartNo(string? partNo)
    {
        if (_grid.Rows.Count == 0) return;

        if (!string.IsNullOrEmpty(partNo))
        {
            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (row.DataBoundItem is TLAGridItem item && item.PartNo == partNo)
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

        if (_grid.Rows[e.RowIndex].DataBoundItem is TLAGridItem item)
        {
            var tla = _allTLAs.FirstOrDefault(t => t.PartNo == item.PartNo);
            if (tla != null)
                ShowEditPanel(tla);
        }
    }

    void NewPartButton_Click(object? sender, EventArgs e)
    {
        ShowNewPanel();
    }

    void ShowEditPanel(TLA tla)
    {
        _isNewTLA = false;
        _editingTLA = tla;

        _detailHeader.Text = "Edit Part Number";
        _partNoTextBox.Text = tla.PartNo;
        _partNoTextBox.ReadOnly = true;
        _partNoTextBox.BackColor = SystemColors.Control;

        _familyTextBox.Text = tla.Family ?? "";
        _subfamilyTextBox.Text = tla.Subfamily ?? "";
        _serviceGroupTextBox.Text = tla.ServiceGroup ?? "";
        _formalDescTextBox.Text = tla.FormalDescription ?? "";
        _descriptionTextBox.Text = tla.Description ?? "";

        // Populate cell checklist with assigned cells sorted to top
        var assignedCellIds = _allCellMappings
            .Where(m => m.PartNo == tla.PartNo)
            .Select(m => m.CellId)
            .ToHashSet();
        PopulateCellChecklist(assignedCellIds);

        // Delete button enabled only if not in use
        _deleteButton.Visible = true;
        _deleteButton.Enabled = !tla.IsUsed;

        _detailPanel.Visible = true;
    }

    void ShowNewPanel()
    {
        _isNewTLA = true;
        _editingTLA = null;

        _detailHeader.Text = "New Part Number";
        _partNoTextBox.Text = "";
        _partNoTextBox.ReadOnly = false;
        _partNoTextBox.BackColor = SystemColors.Window;

        _familyTextBox.Text = "";
        _subfamilyTextBox.Text = "";
        _serviceGroupTextBox.Text = "";
        _formalDescTextBox.Text = "";
        _descriptionTextBox.Text = "";

        // Populate cell checklist with no assignments
        PopulateCellChecklist([]);

        // Hide delete button for new items
        _deleteButton.Visible = false;

        _detailPanel.Visible = true;
        _partNoTextBox.Focus();
    }

    void PopulateCellChecklist(HashSet<int> assignedCellIds)
    {
        _cellCheckedListBox.Items.Clear();

        var sorted = _allCells
            .Where(c => c.IsActive)
            .OrderByDescending(c => assignedCellIds.Contains(c.CellId))  // checked first
            .ThenBy(c => c.CellName);

        foreach (var cell in sorted)
        {
            _cellCheckedListBox.Items.Add(
                new CellCheckItem(cell),
                assignedCellIds.Contains(cell.CellId));
        }
    }

    void HideDetailPanel()
    {
        _detailPanel.Visible = false;
        _editingTLA = null;
    }

    void InUseCheckbox_CheckedChanged(object? sender, EventArgs e)
    {
        // If both unchecked, re-check "In Use" as default
        if (!_inUseCheckbox.Checked && !_unusedCheckbox.Checked)
        {
            _inUseCheckbox.Checked = true;
            return; // This will trigger another CheckedChanged, which will call ApplyFilter
        }
        ApplyFilter();
    }

    void UnusedCheckbox_CheckedChanged(object? sender, EventArgs e)
    {
        // If both unchecked, re-check "In Use" as default
        if (!_inUseCheckbox.Checked && !_unusedCheckbox.Checked)
        {
            _inUseCheckbox.Checked = true;
            return; // This will trigger another CheckedChanged, which will call ApplyFilter
        }
        ApplyFilter();
    }

    void GenerateDescButton_Click(object? sender, EventArgs e)
    {
        var partNo = _partNoTextBox.Text.Trim();
        var formalDesc = _formalDescTextBox.Text.Trim();

        var description = FormatDescription(partNo, formalDesc, elide: true);
        _descriptionTextBox.Text = description;
        _descriptionTextBox.Focus();
    }

    static string FormatDescription(string partNo, string formalDesc, bool elide)
    {
        if (string.IsNullOrEmpty(formalDesc))
            return partNo;

        // Replace commas with spaces
        var cleanedFormalDesc = formalDesc.Replace(",", " ");

        // Elide if needed
        if (elide && cleanedFormalDesc.Length > 40)
            cleanedFormalDesc = cleanedFormalDesc[..37] + "...";

        return $"{partNo} - [{cleanedFormalDesc}]";
    }

    async void DeleteButton_Click(object? sender, EventArgs e)
    {
        if (_editingTLA == null || _editingTLA.IsUsed)
            return;

        var result = MessageBox.Show(
            $"Are you sure you want to delete part number '{_editingTLA.PartNo}'?",
            "Confirm Delete",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result != DialogResult.Yes)
            return;

        try
        {
            _deleteButton.Enabled = false;
            _saveButton.Enabled = false;
            _cancelButton.Enabled = false;

            var success = await _tlaRepo.DeleteAsync(_editingTLA.PartNo);
            if (success)
            {
                HideDetailPanel();
                await LoadDataAsync();
            }
            else
            {
                MessageBox.Show("Failed to delete part number. Check the status bar for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        finally
        {
            _deleteButton.Enabled = true;
            _saveButton.Enabled = true;
            _cancelButton.Enabled = true;
        }
    }

    async void SaveButton_Click(object? sender, EventArgs e)
    {
        // Validate Part No
        var partNo = _partNoTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(partNo))
        {
            MessageBox.Show("Part No is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _partNoTextBox.Focus();
            return;
        }

        // Format description on save
        var description = _descriptionTextBox.Text.Trim();
        var formalDesc = _formalDescTextBox.Text.Trim();

        // Check if description needs eliding
        if (!string.IsNullOrEmpty(description))
        {
            // Replace commas with spaces
            var cleanedDesc = description.Replace(",", " ");

            // Check if the bracketed content is too long
            var bracketStart = cleanedDesc.IndexOf('[');
            var bracketEnd = cleanedDesc.LastIndexOf(']');
            if (bracketStart >= 0 && bracketEnd > bracketStart)
            {
                var bracketContent = cleanedDesc[(bracketStart + 1)..bracketEnd];
                if (bracketContent.Length > 40)
                {
                    var elidedContent = bracketContent[..37] + "...";
                    var elidedDesc = cleanedDesc[..(bracketStart + 1)] + elidedContent + "]";

                    var result = MessageBox.Show(
                        $"The description will be shortened to:\n\n{elidedDesc}\n\nProceed with shortened text?",
                        "Description Too Long",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                        description = elidedDesc;
                    // If No, keep original (with commas replaced)
                    else
                        description = cleanedDesc;
                }
                else
                {
                    description = cleanedDesc;
                }
            }
            else
            {
                description = cleanedDesc;
            }
        }

        var tla = new TLA
        {
            PartNo = partNo,
            Family = string.IsNullOrWhiteSpace(_familyTextBox.Text) ? null : _familyTextBox.Text.Trim(),
            Subfamily = string.IsNullOrWhiteSpace(_subfamilyTextBox.Text) ? null : _subfamilyTextBox.Text.Trim(),
            ServiceGroup = string.IsNullOrWhiteSpace(_serviceGroupTextBox.Text) ? null : _serviceGroupTextBox.Text.Trim(),
            FormalDescription = string.IsNullOrWhiteSpace(_formalDescTextBox.Text) ? null : _formalDescTextBox.Text.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description
        };

        // Get checked cell IDs
        var checkedCellIds = _cellCheckedListBox.CheckedItems
            .Cast<CellCheckItem>()
            .Select(item => item.CellId)
            .ToList();

        try
        {
            _saveButton.Enabled = false;
            _cancelButton.Enabled = false;
            _deleteButton.Enabled = false;

            bool success;
            if (_isNewTLA)
            {
                success = await _tlaRepo.InsertAsync(tla);
                if (!success)
                {
                    MessageBox.Show("Failed to create part number. Check the status bar for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                success = await _tlaRepo.UpdateAsync(tla);
                if (!success)
                {
                    MessageBox.Show("Failed to update part number. Check the status bar for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // Update cell mappings
            var mappingSuccess = await _mappingRepo.SetMappingsAsync(partNo, checkedCellIds);
            if (!mappingSuccess)
            {
                MessageBox.Show("Part number saved but failed to update cell mappings. Check the status bar for details.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            HideDetailPanel();
            await LoadDataAsync(partNo);
        }
        finally
        {
            _saveButton.Enabled = true;
            _cancelButton.Enabled = true;
            _deleteButton.Enabled = true;
        }
    }

    /// <summary>Grid display item wrapping TLA with computed Cells column.</summary>
    sealed class TLAGridItem(TLA tla, string cellsDisplay)
    {
        public string PartNo => tla.PartNo;
        public string? Family => tla.Family;
        public string? Subfamily => tla.Subfamily;
        public string? Description => tla.Description;
        public string CellsDisplay => cellsDisplay;
    }

    /// <summary>Helper class for Cell checklist items.</summary>
    sealed class CellCheckItem(Cell cell)
    {
        public int CellId => cell.CellId;
        public override string ToString() => cell.CellName;
    }
}
