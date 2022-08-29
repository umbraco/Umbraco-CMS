using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Persistence.EFCore;

public class EFDatabaseSchemaCreatorFactory: IDatabaseSchemaCreatorFactory
{
    private readonly ILogger<EFDatabaseSchemaCreator> _logger;
    private readonly UmbracoDbContextFactory _dbContextFactory;
    private readonly IDatabaseDataCreator _databaseDataCreator;

    public EFDatabaseSchemaCreatorFactory(
        ILogger<EFDatabaseSchemaCreator> logger,
        UmbracoDbContextFactory dbContextFactory,
        IDatabaseDataCreator databaseDataCreator)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _databaseDataCreator = databaseDataCreator;
    }
    public IDatabaseSchemaCreator Create(IUmbracoDatabase? database)
    {
        return new EFDatabaseSchemaCreator(_logger, _dbContextFactory, _databaseDataCreator);
    }
}
