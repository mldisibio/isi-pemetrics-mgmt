namespace PEMetrics.IntegrationTests.Fixtures;

/// <summary>xUnit collection definition for shared SQL Server container.</summary>
[CollectionDefinition("SqlServerCollection")]
public sealed class DatabaseFixtureCollection : ICollectionFixture<SqlServerContainerFixture>
{
    // This class has no code; it just anchors the collection definition
}
