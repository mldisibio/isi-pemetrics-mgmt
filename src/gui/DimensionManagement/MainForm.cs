using System.ComponentModel;
using DimensionManagement.Controls;
using Microsoft.Extensions.DependencyInjection;
using PEMetrics.DataCache.Infrastructure;
using PEMetrics.DataCache.Services;

namespace DimensionManagement;

public partial class MainForm : Form
{
    readonly TabControl _tabControl;
    readonly StatusStrip _statusStrip;
    readonly ToolStripStatusLabel _statusLabel;
    readonly ToolStripStatusLabel _modeLabel;
    bool _isOffline;
    CellMaintenanceControl? _cellControl;
    PCStationMaintenanceControl? _pcStationControl;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IServiceProvider? Services { get; set; }

    public MainForm()
    {
        InitializeComponent();

        Text = "PE Metrics Dimension Management";
        Size = new Size(1200, 800);
        StartPosition = FormStartPosition.CenterScreen;

        // Status strip at bottom
        _statusStrip = new StatusStrip();
        _statusLabel = new ToolStripStatusLabel("Initializing...")
        {
            Spring = true,
            TextAlign = ContentAlignment.MiddleLeft
        };
        _modeLabel = new ToolStripStatusLabel("Online")
        {
            BorderSides = ToolStripStatusLabelBorderSides.Left,
            BorderStyle = Border3DStyle.Etched
        };
        _statusStrip.Items.Add(_statusLabel);
        _statusStrip.Items.Add(_modeLabel);
        Controls.Add(_statusStrip);

        // Tab control fills the rest
        _tabControl = new TabControl
        {
            Dock = DockStyle.Fill
        };
        Controls.Add(_tabControl);

        // Load event to initialize tabs after services are wired
        Load += MainForm_Load;
        Shown += MainForm_Shown;
    }

    void MainForm_Load(object? sender, EventArgs e)
    {
        if (Services == null)
            return;

        // Add Cells tab
        var cellsTab = new TabPage("Cells");
        _cellControl = new CellMaintenanceControl(Services)
        {
            Dock = DockStyle.Fill
        };
        cellsTab.Controls.Add(_cellControl);
        _tabControl.TabPages.Add(cellsTab);

        // Add PC Stations tab
        var pcStationsTab = new TabPage("PC Stations");
        _pcStationControl = new PCStationMaintenanceControl(Services)
        {
            Dock = DockStyle.Fill
        };
        pcStationsTab.Controls.Add(_pcStationControl);
        _tabControl.TabPages.Add(pcStationsTab);

        // Placeholder tabs for other features
        _tabControl.TabPages.Add(new TabPage("PC to Cell"));
        _tabControl.TabPages.Add(new TabPage("Software Tests"));
        _tabControl.TabPages.Add(new TabPage("Part Numbers"));
    }

    async void MainForm_Shown(object? sender, EventArgs e)
    {
        if (Services == null)
            return;

        try
        {
            // Run heavy I/O on background thread to keep UI responsive
            await Task.Run(InitializeCacheAsync);

            if (!_isOffline)
            {
                if (_cellControl != null)
                    await _cellControl.LoadDataAsync();
                if (_pcStationControl != null)
                    await _pcStationControl.LoadDataAsync();
            }
        }
        catch (Exception ex)
        {
            ShowStatusMessage($"Initialization error: {ex.Message}");
        }
    }

    async Task InitializeCacheAsync()
    {
        var healthCheck = Services!.GetRequiredService<ProductionStoreHealthCheck>();
        var duckDbInitializer = Services!.GetRequiredService<DuckDbInitializer>();
        var cacheRefreshService = Services!.GetRequiredService<CacheRefreshService>();

        ShowStatusMessage("Checking SQL Server connectivity...");

        var isOnline = await healthCheck.TestConnectivityAsync();
        if (!isOnline)
        {
            SetOfflineMode(true);
            ShowStatusMessage("SQL Server unavailable. Running in offline mode.");
            return;
        }

        ShowStatusMessage("Initializing cache...");
        await duckDbInitializer.InitializeAsync();

        ShowStatusMessage("Populating cache from SQL Server...");
        await cacheRefreshService.PopulateAllTablesAsync();

        cacheRefreshService.Start();
        ShowStatusMessage("Ready");
    }

    public void ShowStatusMessage(string message)
    {
        if (InvokeRequired)
        {
            Invoke(() => ShowStatusMessage(message));
            return;
        }
        _statusLabel.Text = message;
    }

    public void SetOfflineMode(bool offline)
    {
        if (InvokeRequired)
        {
            Invoke(() => SetOfflineMode(offline));
            return;
        }
        _isOffline = offline;
        _modeLabel.Text = offline ? "Offline" : "Online";
        _modeLabel.ForeColor = offline ? Color.Red : Color.Green;
    }

    void InitializeComponent()
    {
        SuspendLayout();
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1184, 761);
        Name = "MainForm";
        ResumeLayout(false);
    }
}
