using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Extensions;

namespace Umbraco.Cms.Persistence.EFCore;

internal class EFDatabaseSchemaCreator : IDatabaseSchemaCreator
{
    private readonly ILogger<EFDatabaseSchemaCreator> _logger;
    private readonly UmbracoDbContextFactory _dbContextFactory;
    private readonly IDatabaseDataCreator _databaseDataCreator;

    public EFDatabaseSchemaCreator(
        ILogger<EFDatabaseSchemaCreator> logger,
        UmbracoDbContextFactory dbContextFactory,
        IDatabaseDataCreator databaseDataCreator)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _databaseDataCreator = databaseDataCreator;
    }

    public async Task InitializeDatabaseSchema(bool includeData = true)
    {
        await _dbContextFactory.ExecuteWithContextAsync<Task>(async db =>
        {
            await db.MigrateDatabaseAsync("20220823084205_InitialState");
        });

        if (includeData)
        {
            await _databaseDataCreator.SeedDataAsync();
        }
    }
}
