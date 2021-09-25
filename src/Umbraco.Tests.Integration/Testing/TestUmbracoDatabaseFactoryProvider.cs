// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;

namespace Umbraco.Cms.Tests.Integration.Testing
{
    /// <summary>
    /// I want to be able to create a database for integration testsing without setting the connection string on the
    /// singleton database factory forever.
    /// </summary>
    public class TestUmbracoDatabaseFactoryProvider
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IOptions<GlobalSettings> _globalSettings;
        private readonly IMapperCollection _mappers;
        private readonly IConfiguration _configuration;
        private readonly IDbProviderFactoryCreator _dbProviderFactoryCreator;
        private readonly DatabaseSchemaCreatorFactory _databaseSchemaCreatorFactory;
        private readonly NPocoMapperCollection _npocoMappers;

        public TestUmbracoDatabaseFactoryProvider(
            ILoggerFactory loggerFactory,
            IOptions<GlobalSettings> globalSettings,
            IMapperCollection mappers,
            IConfiguration configuration,
            IDbProviderFactoryCreator dbProviderFactoryCreator,
            DatabaseSchemaCreatorFactory databaseSchemaCreatorFactory,
            NPocoMapperCollection npocoMappers)
        {
            _loggerFactory = loggerFactory;
            _globalSettings = globalSettings;
            _mappers = mappers;
            _configuration = configuration;
            _dbProviderFactoryCreator = dbProviderFactoryCreator;
            _databaseSchemaCreatorFactory = databaseSchemaCreatorFactory;
            _npocoMappers = npocoMappers;
        }

        public IUmbracoDatabaseFactory Create()
            => new UmbracoDatabaseFactory(
                _loggerFactory.CreateLogger<UmbracoDatabaseFactory>(),
                _loggerFactory,
                _globalSettings,
                _configuration,
                _mappers,
                _dbProviderFactoryCreator,
                _databaseSchemaCreatorFactory,
                _npocoMappers);
    }
}
