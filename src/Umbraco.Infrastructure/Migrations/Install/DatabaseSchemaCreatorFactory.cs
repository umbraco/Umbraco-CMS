using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Infrastructure.Migrations.Install
{
    /// <summary>
    /// Creates the initial database schema during install.
    /// </summary>
    public class DatabaseSchemaCreatorFactory
    {
        private readonly ILogger<DatabaseSchemaCreator> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IEventAggregator _eventAggregator;
        private readonly IOptionsMonitor<InstallDefaultDataSettings> _installDefaultDataSettings;

        public DatabaseSchemaCreatorFactory(
            ILogger<DatabaseSchemaCreator> logger,
            ILoggerFactory loggerFactory,
            IEventAggregator eventAggregator,
            IOptionsMonitor<InstallDefaultDataSettings> installDefaultDataSettings)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
            _eventAggregator = eventAggregator;
            _installDefaultDataSettings = installDefaultDataSettings;
        }

        public DatabaseSchemaCreator Create(IUmbracoDatabase database)
            => new DatabaseSchemaCreator(database, _logger, _loggerFactory, _eventAggregator, _installDefaultDataSettings);
    }
}
