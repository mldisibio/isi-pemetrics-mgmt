using System.Collections.Immutable;
using DimensionManagement.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using PEMetrics.DataApi.Models;
using PEMetrics.DataApi.Ports;

namespace DimensionManagement.Controls;

/// <summary>User control for Software Test maintenance.</summary>
public sealed class SwTestMaintenanceControl : UserControl
{
    static readonly Font MonoFont = new("Lucida Console", 8f);

    readonly ForReadingPEMetricsDimensions _queryRepo;
    readonly ForManagingSwTests _testRepo;
    readonly ForMappingSwTestsToCells _mappingRepo;

    // Top toolbar
    readonly CheckBox _activeOnlyCheckbox;
    readonly TextBox _filterTextBox;
    readonly Button _newTestButton;

    // Grid
    readonly DataGridView _grid;
    ImmutableList<SwTestMap> _allTests = [];
    ImmutableList<CellBySwTest> _allCellMappings = [];

    // Reference data
    ImmutableList<Cell> _allCells = [];

    // Detail panel
    readonly Panel _detailPanel;
    readonly Label _detailHeader;
    readonly TextBox _swTestMapIdTextBox;
    readonly TextBox _reportKeyTextBox;
    readonly Button _generateReportKeyButton;
    readonly TextBox _testIdTextBox;
    readonly TextBox _testNameTextBox;
    readonly TextBox _applicationTextBox;
    readonly TextBox _lastRunTextBox;
    readonly TextBox _directoryTextBox;
    readonly TextBox _relPathTextBox;
    readonly TextBox _notesTextBox;
    readonly CheckedListBox _cellCheckedListBox;
    readonly Button _saveButton;
    readonly Button _cancelButton;

    SwTestMap? _editingTest;
    bool _isNewTest;

    public SwTestMaintenanceControl(IServiceProvider services)
    {
        _queryRepo = services.GetRequiredService<ForReadingPEMetricsDimensions>();
        _testRepo = services.GetRequiredService<ForManagingSwTests>();
        _mappingRepo = services.GetRequiredService<ForMappingSwTestsToCells>();

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

        var filterLabel = new Label
        {
            Text = "Filter:",
            AutoSize = true,
            Location = new Point(120, 12)
        };

        _filterTextBox = new TextBox
        {
            Location = new Point(160, 8),
            Width = 150,
            Font = MonoFont
        };
        _filterTextBox.TextChanged += (s, e) => ApplyFilter();

        var clearFilterButton = new Button
        {
            Text = "×",
            Location = new Point(312, 7),
            Width = 23,
            Height = 23,
            Font = new Font(Font.FontFamily, 9f)
        };
        clearFilterButton.Click += (s, e) => { _filterTextBox.Clear(); _filterTextBox.Focus(); };

        _newTestButton = new Button
        {
            Text = "+ New Test",
            AutoSize = true,
            Location = new Point(345, 6)
        };
        _newTestButton.Click += NewTestButton_Click;

        toolbarPanel.Controls.AddRange([_activeOnlyCheckbox, filterLabel, _filterTextBox, clearFilterButton, _newTestButton]);

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
            new DataGridViewTextBoxColumn { Name = "Cells", HeaderText = "Cells", DataPropertyName = "CellsDisplay", FillWeight = 12 },
            new DataGridViewTextBoxColumn { Name = "SwTestMapId", HeaderText = "ID", DataPropertyName = "SwTestMapId", Width = 20, FillWeight = 5 },
            new DataGridViewTextBoxColumn { Name = "ReportKey", HeaderText = "Report Key", DataPropertyName = "ReportKey", FillWeight = 30 },
            new DataGridViewTextBoxColumn { Name = "TestId", HeaderText = "Cfg Id", DataPropertyName = "ConfiguredTestId", FillWeight = 5 },
            new DataGridViewTextBoxColumn { Name = "TestName", HeaderText = "Test Name", DataPropertyName = "TestName", FillWeight = 25 },
            new DataGridViewTextBoxColumn { Name = "TestApplication", HeaderText = "Application", DataPropertyName = "TestApplication", FillWeight = 15 },
            new DataGridViewTextBoxColumn { Name = "LastRun", HeaderText = "Last Run", DataPropertyName = "LastRun", Width = 80, FillWeight = 8, DefaultCellStyle = dateStyle }
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
            Text = "Edit Test",
            Font = new Font(Font.FontFamily, 10, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(10, 10)
        };

        // Row 1: SwTestMapId (readonly), Report Key with generate button
        var swTestMapIdLabel = new Label { Text = "SwTestMapId:", Location = new Point(10, 40), AutoSize = true };
        _swTestMapIdTextBox = new TextBox { Location = new Point(95, 37), Width = 60, ReadOnly = true, BackColor = SystemColors.Control, Font = MonoFont };

        var reportKeyLabel = new Label { Text = "Report Key:", Location = new Point(180, 40), AutoSize = true };
        _reportKeyTextBox = new TextBox { Location = new Point(260, 37), Width = 180, Font = MonoFont };
        _generateReportKeyButton = new Button
        {
            Text = "↻",
            Location = new Point(445, 36),
            Width = 28,
            Height = 23,
            Font = new Font(Font.FontFamily, 9f)
        };
        _generateReportKeyButton.Click += GenerateReportKeyButton_Click;

        // Row 2: Test Id (ConfiguredTestId), Test Name
        var testIdLabel = new Label { Text = "Test Id:", Location = new Point(10, 70), AutoSize = true };
        _testIdTextBox = new TextBox { Location = new Point(95, 67), Width = 60, Font = MonoFont };

        var testNameLabel = new Label { Text = "Test Name:", Location = new Point(180, 70), AutoSize = true };
        _testNameTextBox = new TextBox { Location = new Point(260, 67), Width = 213, Font = MonoFont };

        // Row 3: Application, Last Run
        var applicationLabel = new Label { Text = "Application:", Location = new Point(10, 100), AutoSize = true };
        _applicationTextBox = new TextBox { Location = new Point(95, 97), Width = 150, Font = MonoFont };

        var lastRunLabel = new Label { Text = "Last Run:", Location = new Point(270, 100), AutoSize = true };
        _lastRunTextBox = new TextBox { Location = new Point(340, 97), Width = 100, Font = MonoFont };
        var lastRunHint = new Label { Text = "(YYYY-MM-DD)", Location = new Point(445, 100), AutoSize = true, ForeColor = SystemColors.GrayText };

        // Row 4: Directory
        var directoryLabel = new Label { Text = "Directory:", Location = new Point(10, 130), AutoSize = true };
        _directoryTextBox = new TextBox { Location = new Point(95, 127), Width = 378, Font = MonoFont };

        // Row 5: Relative Path
        var relPathLabel = new Label { Text = "Rel. Path:", Location = new Point(10, 160), AutoSize = true };
        _relPathTextBox = new TextBox { Location = new Point(95, 157), Width = 378, Font = MonoFont };

        // Row 6: Notes
        var notesLabel = new Label { Text = "Notes:", Location = new Point(10, 190), AutoSize = true };
        _notesTextBox = new TextBox { Location = new Point(95, 187), Width = 378, Font = MonoFont };

        // Row 7: Cells (CheckedListBox)
        var cellsLabel = new Label { Text = "Cells:", Location = new Point(10, 220), AutoSize = true };
        _cellCheckedListBox = new CheckedListBox
        {
            Location = new Point(95, 217),
            Width = 200,
            Height = 75,
            Font = MonoFont,
            CheckOnClick = true
        };

        // Buttons
        _cancelButton = new Button { Text = "Cancel", Location = new Point(300, 270), Width = 80 };
        _cancelButton.Click += (s, e) => HideDetailPanel();

        _saveButton = new Button { Text = "Save", Location = new Point(390, 270), Width = 80 };
        _saveButton.Click += SaveButton_Click;

        _detailPanel.Controls.AddRange(
        [
            _detailHeader,
            swTestMapIdLabel, _swTestMapIdTextBox,
            reportKeyLabel, _reportKeyTextBox, _generateReportKeyButton,
            testIdLabel, _testIdTextBox,
            testNameLabel, _testNameTextBox,
            applicationLabel, _applicationTextBox,
            lastRunLabel, _lastRunTextBox, lastRunHint,
            directoryLabel, _directoryTextBox,
            relPathLabel, _relPathTextBox,
            notesLabel, _notesTextBox,
            cellsLabel, _cellCheckedListBox,
            _cancelButton, _saveButton
        ]);

        // Add controls in correct order (bottom to top for docking)
        Controls.Add(_grid);
        Controls.Add(_detailPanel);
        Controls.Add(toolbarPanel);
    }

    public async Task LoadDataAsync(int? selectTestId = null)
    {
        var testsTask = _queryRepo.GetSwTestsAsync();
        var cellMappingsTask = _queryRepo.GetSwTestToCellMappingsAsync();
        var cellsTask = _queryRepo.GetCellsAsync();

        await Task.WhenAll(testsTask, cellMappingsTask, cellsTask).ConfigureAwait(true);

        _allTests = testsTask.Result;
        _allCellMappings = cellMappingsTask.Result
            .GroupBy(m => m.SwTestMapId)
            .SelectMany(g => g.Select(m => new CellBySwTest
            {
                SwTestMapId = m.SwTestMapId,
                CellId = m.CellId,
                CellName = m.CellName
            }))
            .ToImmutableList();
        _allCells = cellsTask.Result;

        ApplyFilter(selectTestId);
    }

    void ApplyFilter(int? selectTestId = null)
    {
        // Preserve current sort state
        var sortColumn = _grid.SortedColumn;
        var sortOrder = _grid.SortOrder;

        // Capture current selection if no override provided
        var idToSelect = selectTestId
            ?? (_grid.CurrentRow?.DataBoundItem as SwTestGridItem)?.SwTestMapId;

        var filterText = _filterTextBox.Text.Trim();

        var filtered = _allTests
            .Where(t => !_activeOnlyCheckbox.Checked || t.IsActive)
            .Where(t => string.IsNullOrEmpty(filterText) ||
                        (t.ReportKey?.Contains(filterText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (t.TestName?.Contains(filterText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (t.TestApplication?.Contains(filterText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (t.ConfiguredTestId?.Contains(filterText, StringComparison.OrdinalIgnoreCase) ?? false))
            .Select(t => new SwTestGridItem(t, GetCellsDisplay(t.SwTestMapId)))
            .ToList();

        _grid.DataSource = new SortableBindingList<SwTestGridItem>(filtered);

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
        SelectRowByTestId(idToSelect);
    }

    string GetCellsDisplay(int swTestMapId)
    {
        var cells = _allCellMappings
            .Where(m => m.SwTestMapId == swTestMapId)
            .Select(m => m.CellName)
            .Where(n => n != null)
            .OrderBy(n => n);
        return string.Join(", ", cells);
    }

    void SelectRowByTestId(int? testId)
    {
        if (_grid.Rows.Count == 0) return;

        if (testId.HasValue)
        {
            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (row.DataBoundItem is SwTestGridItem item && item.SwTestMapId == testId.Value)
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

        if (_grid.Rows[e.RowIndex].DataBoundItem is SwTestGridItem item)
        {
            var test = _allTests.FirstOrDefault(t => t.SwTestMapId == item.SwTestMapId);
            if (test != null)
                ShowEditPanel(test);
        }
    }

    void NewTestButton_Click(object? sender, EventArgs e)
    {
        ShowNewPanel();
    }

    void ShowEditPanel(SwTestMap test)
    {
        _isNewTest = false;
        _editingTest = test;

        _detailHeader.Text = $"Edit Test (ID: {test.SwTestMapId})";
        _swTestMapIdTextBox.Text = test.SwTestMapId.ToString();
        _reportKeyTextBox.Text = test.ReportKey ?? "";
        _testIdTextBox.Text = test.ConfiguredTestId ?? "";
        _testNameTextBox.Text = test.TestName ?? "";
        _applicationTextBox.Text = test.TestApplication ?? "";
        _lastRunTextBox.Text = test.LastRun?.ToString("yyyy-MM-dd") ?? "";
        _directoryTextBox.Text = test.TestDirectory ?? "";
        _relPathTextBox.Text = test.RelativePath ?? "";
        _notesTextBox.Text = test.Notes ?? "";

        // Populate cell checklist with assigned cells sorted to top
        var assignedCellIds = _allCellMappings
            .Where(m => m.SwTestMapId == test.SwTestMapId)
            .Select(m => m.CellId)
            .ToHashSet();
        PopulateCellChecklist(assignedCellIds);

        _detailPanel.Visible = true;
    }

    void ShowNewPanel()
    {
        _isNewTest = true;
        _editingTest = null;

        _detailHeader.Text = "New Test";
        _swTestMapIdTextBox.Text = "(auto)";
        _reportKeyTextBox.Text = "";
        _testIdTextBox.Text = "";
        _testNameTextBox.Text = "";
        _applicationTextBox.Text = "";
        _lastRunTextBox.Text = "";
        _directoryTextBox.Text = "";
        _relPathTextBox.Text = "";
        _notesTextBox.Text = "";

        // Populate cell checklist with no assignments
        PopulateCellChecklist([]);

        _detailPanel.Visible = true;
        _testIdTextBox.Focus();
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
        _editingTest = null;
    }

    void GenerateReportKeyButton_Click(object? sender, EventArgs e)
    {
        var testId = _testIdTextBox.Text.Trim();
        var testName = _testNameTextBox.Text.Trim();

        var reportKey = string.IsNullOrEmpty(testName)
            ? testId
            : $"{testId} - {ShortenTestName(testName)}";

        _reportKeyTextBox.Text = reportKey;
        _reportKeyTextBox.Focus();

        static string ShortenTestName(string fullName)
            => fullName.Replace(" test", "", StringComparison.OrdinalIgnoreCase)
                       .Replace(" check", "", StringComparison.OrdinalIgnoreCase)
                       .Trim();
    }

    async void SaveButton_Click(object? sender, EventArgs e)
    {
        // Parse LastRun date (optional)
        DateOnly? lastRun = null;
        if (!string.IsNullOrWhiteSpace(_lastRunTextBox.Text))
        {
            if (!DateOnly.TryParse(_lastRunTextBox.Text.Trim(), out var parsedLastRun))
            {
                MessageBox.Show("Last Run must be a valid date (YYYY-MM-DD) or blank.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _lastRunTextBox.Focus();
                return;
            }
            lastRun = parsedLastRun;
        }

        var test = new SwTestMap
        {
            SwTestMapId = _isNewTest ? 0 : _editingTest!.SwTestMapId,
            ConfiguredTestId = string.IsNullOrWhiteSpace(_testIdTextBox.Text) ? null : _testIdTextBox.Text.Trim(),
            TestApplication = string.IsNullOrWhiteSpace(_applicationTextBox.Text) ? null : _applicationTextBox.Text.Trim(),
            TestName = string.IsNullOrWhiteSpace(_testNameTextBox.Text) ? null : _testNameTextBox.Text.Trim(),
            ReportKey = string.IsNullOrWhiteSpace(_reportKeyTextBox.Text) ? null : _reportKeyTextBox.Text.Trim(),
            TestDirectory = string.IsNullOrWhiteSpace(_directoryTextBox.Text) ? null : _directoryTextBox.Text.Trim(),
            RelativePath = string.IsNullOrWhiteSpace(_relPathTextBox.Text) ? null : _relPathTextBox.Text.Trim(),
            LastRun = lastRun,
            Notes = string.IsNullOrWhiteSpace(_notesTextBox.Text) ? null : _notesTextBox.Text.Trim()
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

            int testId;
            if (_isNewTest)
            {
                testId = await _testRepo.InsertAsync(test);
                if (testId <= 0)
                {
                    MessageBox.Show("Failed to create test. Check the status bar for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                testId = test.SwTestMapId;
                var success = await _testRepo.UpdateAsync(test);
                if (!success)
                {
                    MessageBox.Show("Failed to update test. Check the status bar for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // Update cell mappings
            var mappingSuccess = await _mappingRepo.SetMappingsAsync(testId, checkedCellIds);
            if (!mappingSuccess)
            {
                MessageBox.Show("Test saved but failed to update cell mappings. Check the status bar for details.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            HideDetailPanel();
            await LoadDataAsync(testId);
        }
        finally
        {
            _saveButton.Enabled = true;
            _cancelButton.Enabled = true;
        }
    }

    /// <summary>Grid display item wrapping SwTestMap with computed Cells column.</summary>
    sealed class SwTestGridItem(SwTestMap test, string cellsDisplay)
    {
        public int SwTestMapId => test.SwTestMapId;
        public string? ReportKey => test.ReportKey;
        public int? ConfiguredTestId => test.ConfiguredTestId != null ? int.TryParse(test.ConfiguredTestId, out var id) ? id : null : null;
        public string? TestName => test.TestName;
        public string? TestApplication => test.TestApplication;
        public DateOnly? LastRun => test.LastRun;
        public string CellsDisplay => cellsDisplay;
    }

    /// <summary>Helper class for Cell checklist items.</summary>
    sealed class CellCheckItem(Cell cell)
    {
        public int CellId => cell.CellId;
        public override string ToString() => cell.CellName;
    }
}
