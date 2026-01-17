using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using PEMetrics.DataApi.Models;
using PEMetrics.DataApi.Ports;

namespace DimensionManagement.Controls;

/// <summary>User control for PC Station maintenance with inline search/add.</summary>
public sealed class PCStationMaintenanceControl : UserControl
{
    static readonly Font MonoFont = new("Lucida Console", 8f);

    readonly ForReadingPEMetricsDimensions _queryRepo;
    readonly ForManagingPCStations _pcStationRepo;

    // Toolbar
    readonly TextBox _searchTextBox;
    readonly Button _clearButton;
    readonly Button _addButton;

    // List
    readonly ListBox _listBox;
    ImmutableList<PCStation> _allStations = [];

    public PCStationMaintenanceControl(IServiceProvider services)
    {
        _queryRepo = services.GetRequiredService<ForReadingPEMetricsDimensions>();
        _pcStationRepo = services.GetRequiredService<ForManagingPCStations>();

        // Toolbar panel
        var toolbarPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 40,
            Padding = new Padding(5)
        };

        var searchLabel = new Label
        {
            Text = "Search:",
            AutoSize = true,
            Location = new Point(10, 12)
        };

        _searchTextBox = new TextBox
        {
            Location = new Point(70, 8),
            Width = 200,
            Font = MonoFont
        };
        _searchTextBox.TextChanged += (s, e) => ApplyFilter();

        _clearButton = new Button
        {
            Text = "Clear",
            Location = new Point(280, 6),
            Width = 60
        };
        _clearButton.Click += (s, e) =>
        {
            _searchTextBox.Clear();
            _searchTextBox.Focus();
        };

        _addButton = new Button
        {
            Text = "Add",
            Location = new Point(350, 6),
            Width = 60,
            Enabled = false
        };
        _addButton.Click += AddButton_Click;

        toolbarPanel.Controls.AddRange([searchLabel, _searchTextBox, _clearButton, _addButton]);

        // ListBox for PC names (no header needed)
        _listBox = new ListBox
        {
            Dock = DockStyle.Fill,
            Font = MonoFont,
            BorderStyle = BorderStyle.FixedSingle,
            SelectionMode = SelectionMode.One
        };

        // Add controls (bottom to top for docking)
        Controls.Add(_listBox);
        Controls.Add(toolbarPanel);
    }

    public async Task LoadDataAsync()
    {
        _allStations = await _queryRepo.GetPCStationsAsync();
        ApplyFilter();
    }

    void ApplyFilter()
    {
        var searchText = _searchTextBox.Text.Trim();

        var filtered = string.IsNullOrEmpty(searchText)
            ? _allStations
            : _allStations.Where(s => s.PcName.Contains(searchText, StringComparison.OrdinalIgnoreCase)).ToImmutableList();

        _listBox.BeginUpdate();
        _listBox.Items.Clear();
        foreach (var station in filtered)
        {
            _listBox.Items.Add(station.PcName);
        }
        _listBox.EndUpdate();

        // Enable Add button only when: search text is non-empty AND zero matches
        _addButton.Enabled = !string.IsNullOrWhiteSpace(searchText) && filtered.IsEmpty;
    }

    async void AddButton_Click(object? sender, EventArgs e)
    {
        var pcName = _searchTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(pcName))
            return;

        try
        {
            _addButton.Enabled = false;
            _searchTextBox.Enabled = false;
            _clearButton.Enabled = false;

            var success = await _pcStationRepo.InsertAsync(pcName);
            if (success)
            {
                // Reload data with same search term - new item will appear as confirmation
                await LoadDataAsync();
            }
            else
            {
                MessageBox.Show("Failed to add PC station. Check the status bar for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        finally
        {
            _searchTextBox.Enabled = true;
            _clearButton.Enabled = true;
            _searchTextBox.Focus();
            // ApplyFilter (called by LoadDataAsync) handles Add button state
        }
    }
}
