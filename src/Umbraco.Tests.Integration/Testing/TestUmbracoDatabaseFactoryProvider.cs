// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
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
        private readonly IOptions<ConnectionStrings> _connectionStrings;
        private readonly Lazy<IMapperCollection> _mappers;
        private readonly IDbProviderFactoryCreator _dbProviderFactoryCreator;
        private readonly DatabaseSchemaCreatorFactory _databaseSchemaCreatorFactory;

        public TestUmbracoDatabaseFactoryProvider(
            ILoggerFactory loggerFactory,
            IOptions<GlobalSettings> globalSettings,
            IOptions<ConnectionStrings> connectionStrings,
            Lazy<IMapperCollection> mappers,
            IDbProviderFactoryCreator dbProviderFactoryCreator,
            DatabaseSchemaCreatorFactory databaseSchemaCreatorFactory)
        {
            _loggerFactory = loggerFactory;
            _globalSettings = globalSettings;
            _connectionStrings = connectionStrings;
            _mappers = mappers;
            _dbProviderFactoryCreator = dbProviderFactoryCreator;
            _databaseSchemaCreatorFactory = databaseSchemaCreatorFactory;
        }

        public IUmbracoDatabaseFactory Create()
        {
            // ReSharper disable once ArrangeMethodOrOperatorBody
            return new UmbracoDatabaseFactory(
                _loggerFactory.CreateLogger<UmbracoDatabaseFactory>(),
                _loggerFactory,
                _globalSettings.Value,
                _connectionStrings.Value,
                _mappers,
                _dbProviderFactoryCreator,
                _databaseSchemaCreatorFactory);
        }
    }
}
