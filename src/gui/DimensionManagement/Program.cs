using DimensionManagement.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PEMetrics.DataApi.Infrastructure;
using PEMetrics.DataApi.Infrastructure.Mapping;
using PEMetrics.DataApi.Ports;
using PEMetrics.DataCache.Configuration;
using PEMetrics.DataCache.Infrastructure;
using PEMetrics.DataCache.Repositories;
using PEMetrics.DataCache.Services;
using PEMetrics.ProductionStore;

namespace DimensionManagement;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var mainForm = new MainForm();
        var services = ConfigureServices(configuration, mainForm);
        mainForm.Services = services;

        // Cache initialization happens in MainForm.Shown event
        Application.Run(mainForm);

        // Cleanup on exit
        services.GetService<CacheRefreshService>()?.Dispose();
        (services.GetService<ForCreatingDuckDbConnections>() as IDisposable)?.Dispose();
    }

    static IServiceProvider ConfigureServices(IConfiguration configuration, MainForm mainForm)
    {
        var services = new ServiceCollection();

        // Configuration
        services.AddSingleton<IConfiguration>(configuration);
        var cacheConfig = configuration.GetSection("CacheConfiguration").Get<CacheConfiguration>() ?? new CacheConfiguration();
        services.AddSingleton(cacheConfig);

        // Error notifier (wired to MainForm status bar)
        var errorNotifier = new UIErrorNotifier(mainForm.ShowStatusMessage, mainForm.SetOfflineMode);
        services.AddSingleton<ForNotifyingDataCommunicationErrors>(errorNotifier);

        // Mappers
        services.AddSingleton<ForMappingDataModels, DataModelMappers>();

        // Connection factories
        services.AddSingleton<ForCreatingSqlServerConnections, SqlConnectionFactory>();
        services.AddSingleton<ForCreatingDuckDbConnections, DuckDbConnectionFactory>();

        // Infrastructure
        services.AddSingleton<TablePopulationTracker>();
        services.AddSingleton<DuckDbInitializer>();
        services.AddSingleton<ProductionStoreHealthCheck>();
        services.AddSingleton<CachePathResolver>();

        // Cache refresh service - creates its own internal channel
        services.AddSingleton<CacheRefreshService>();

        // Data change notification handler - uses the Writer from CacheRefreshService
        services.AddSingleton<ForNotifyingDataChanges>(sp =>
            new DataChangeNotificationHandler(sp.GetRequiredService<CacheRefreshService>().Writer));

        // Query repository (reads from DuckDB cache)
        services.AddSingleton<ForReadingPEMetricsDimensions, DuckDbQueryRepository>();

        // Command repositories (write to SQL Server)
        services.AddSingleton<ForManagingCells, CellRepository>();
        services.AddSingleton<ForManagingPCStations, PCStationRepository>();
        services.AddSingleton<ForMappingPCStationToCell, CellByPCStationRepository>();
        services.AddSingleton<ForManagingSwTests, SwTestMapRepository>();
        services.AddSingleton<ForMappingSwTestsToCells, CellBySwTestRepository>();
        services.AddSingleton<ForManagingPartNumbers, TLARepository>();
        services.AddSingleton<ForMappingPartNumberToCells, CellByPartNoRepository>();

        return services.BuildServiceProvider();
    }
}