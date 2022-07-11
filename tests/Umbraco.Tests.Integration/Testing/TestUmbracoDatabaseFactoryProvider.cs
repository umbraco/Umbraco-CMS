// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;

namespace Umbraco.Cms.Tests.Integration.Testing;

/// <summary>
///     I want to be able to create a database for integration testsing without setting the connection string on the
///     singleton database factory forever.
/// </summary>
public class TestUmbracoDatabaseFactoryProvider
{
    private readonly IOptionsMonitor<ConnectionStrings> _connectionStrings;
    private readonly DatabaseSchemaCreatorFactory _databaseSchemaCreatorFactory;
    private readonly IDbProviderFactoryCreator _dbProviderFactoryCreator;
    private readonly IOptions<GlobalSettings> _globalSettings;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IMapperCollection _mappers;
    private readonly NPocoMapperCollection _npocoMappers;

    public TestUmbracoDatabaseFactoryProvider(
        ILoggerFactory loggerFactory,
        IOptions<GlobalSettings> globalSettings,
        IOptionsMonitor<ConnectionStrings> connectionStrings,
        IMapperCollection mappers,
        IDbProviderFactoryCreator dbProviderFactoryCreator,
        DatabaseSchemaCreatorFactory databaseSchemaCreatorFactory,
        NPocoMapperCollection npocoMappers)
    {
        _loggerFactory = loggerFactory;
        _globalSettings = globalSettings;
        _connectionStrings = connectionStrings;
        _mappers = mappers;
        _dbProviderFactoryCreator = dbProviderFactoryCreator;
        _databaseSchemaCreatorFactory = databaseSchemaCreatorFactory;
        _npocoMappers = npocoMappers;
    }

    public IUmbracoDatabaseFactory Create()
        => new UmbracoDatabaseFactory(
            _loggerFactory.CreateLogger<UmbracoDatabaseFactory>(),
            _loggerFactory,
            _globalSettings,
            _connectionStrings,
            _mappers,
            _dbProviderFactoryCreator,
            _databaseSchemaCreatorFactory,
            _npocoMappers);
}
