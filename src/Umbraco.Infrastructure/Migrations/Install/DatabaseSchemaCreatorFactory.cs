using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Infrastructure.Migrations.Install;

/// <summary>
///     Creates the initial database schema during install.
/// </summary>
public class DatabaseSchemaCreatorFactory
{
    private readonly IEventAggregator _eventAggregator;
    private readonly IOptionsMonitor<InstallDefaultDataSettings> _installDefaultDataSettings;
    private readonly ILogger<DatabaseSchemaCreator> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IUmbracoVersion _umbracoVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Migrations.Install.DatabaseSchemaCreatorFactory"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for <see cref="DatabaseSchemaCreator"/> operations.</param>
        /// <param name="loggerFactory">The factory used to create logger instances.</param>
        /// <param name="umbracoVersion">Provides information about the current Umbraco version.</param>
        /// <param name="eventAggregator">Handles event aggregation for install and migration events.</param>
        /// <param name="installDefaultDataSettings">Monitors configuration settings for installing default data.</param>
        public DatabaseSchemaCreatorFactory(
            ILogger<DatabaseSchemaCreator> logger,
            ILoggerFactory loggerFactory,
            IUmbracoVersion umbracoVersion,
            IEventAggregator eventAggregator,
            IOptionsMonitor<InstallDefaultDataSettings> installDefaultDataSettings)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
            _umbracoVersion = umbracoVersion;
            _eventAggregator = eventAggregator;
            _installDefaultDataSettings = installDefaultDataSettings;
        }

    /// <summary>
    ///     Creates a new instance of <see cref="DatabaseSchemaCreator" />.
    /// </summary>
    /// <param name="database">The database.</param>
    /// <returns>A new <see cref="DatabaseSchemaCreator" /> instance.</returns>
    public DatabaseSchemaCreator Create(IUmbracoDatabase? database) => new DatabaseSchemaCreator(
        database,
        _logger,
        _loggerFactory,
        _umbracoVersion,
        _eventAggregator,
        _installDefaultDataSettings);
}
