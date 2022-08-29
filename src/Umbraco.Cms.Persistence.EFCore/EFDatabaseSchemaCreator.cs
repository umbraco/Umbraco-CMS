using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Migrations.Install;

namespace Umbraco.Cms.Persistence.EFCore;

public class EFDatabaseSchemaCreator : IDatabaseSchemaCreator
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

    public async Task InitializeDatabaseSchema()
    {
        await _dbContextFactory.ExecuteWithContextAsync<Task>(async db =>
        {

            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                await db.Database.MigrateAsync();

                await transaction.CommitAsync();
            }
        });


        await _databaseDataCreator.SeedDataAsync();
    }



}
