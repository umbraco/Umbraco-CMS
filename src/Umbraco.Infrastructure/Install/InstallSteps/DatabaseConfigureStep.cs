﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations.Install;
using Umbraco.Web.Install.Models;
using Umbraco.Core.Configuration.Models;
using Microsoft.Extensions.Options;

namespace Umbraco.Web.Install.InstallSteps
{
    [InstallSetupStep(InstallationType.NewInstall,
        "DatabaseConfigure", "database", 10, "Setting up a database, so Umbraco has a place to store your website",
        PerformsAppRestart = true)]
    public class DatabaseConfigureStep : InstallSetupStep<DatabaseModel>
    {
        private readonly DatabaseBuilder _databaseBuilder;
        private readonly ILogger _logger;
        private readonly ConnectionStrings _connectionStrings;

        public DatabaseConfigureStep(DatabaseBuilder databaseBuilder, IOptions<ConnectionStrings> connectionStrings)
        {
            _databaseBuilder = databaseBuilder;
            _connectionStrings = connectionStrings.Value ?? throw new ArgumentNullException(nameof(connectionStrings));
        }

        public override Task<InstallSetupResult> ExecuteAsync(DatabaseModel database)
        {
            //if the database model is null then we will apply the defaults
            if (database == null)
            {
                database = new DatabaseModel();
            }

            if (_databaseBuilder.CanConnect(database.DatabaseType.ToString(), database.ConnectionString, database.Server, database.DatabaseName, database.Login, database.Password, database.IntegratedAuth) == false)
            {
                throw new InstallException("Could not connect to the database");
            }
            ConfigureConnection(database);
            return Task.FromResult<InstallSetupResult>(null);
        }

        private void ConfigureConnection(DatabaseModel database)
        {
            if (database.ConnectionString.IsNullOrWhiteSpace() == false)
            {
                _databaseBuilder.ConfigureDatabaseConnection(database.ConnectionString);
            }
            else if (database.DatabaseType == DatabaseType.SqlCe)
            {
                _databaseBuilder.ConfigureEmbeddedDatabaseConnection();
            }
            else if (database.IntegratedAuth)
            {
                _databaseBuilder.ConfigureIntegratedSecurityDatabaseConnection(database.Server, database.DatabaseName);
            }
            else
            {
                var password = database.Password.Replace("'", "''");
                password = string.Format("'{0}'", password);

                _databaseBuilder.ConfigureDatabaseConnection(
                    database.Server, database.DatabaseName, database.Login, password,
                    database.DatabaseType.ToString());
            }
        }

        public override object ViewModel
        {
            get
            {
                var databases = new List<object>()
                {
                    new { name = "Microsoft SQL Server", id = 1 },
                    new { name = "Microsoft SQL Azure", id = 3 },
                    new { name = "Custom connection string", id = -1 },
                };

                if (IsSqlCeAvailable())
                {
                    databases.Insert(0,  new { name = "Microsoft SQL Server Compact (SQL CE)", id = 0 });
                }

                return new
                {
                    databases = databases
                };
            }
        }

        public static bool IsSqlCeAvailable()
        {
            // NOTE: Type.GetType will only return types that are currently loaded into the appdomain. In this case
            // that is ok because we know if this is availalbe we will have manually loaded it into the appdomain.
            // Else we'd have to use Assembly.LoadFrom and need to know the DLL location here which we don't need to do.
            return !(Type.GetType("Umbraco.Persistance.SqlCe.SqlCeSyntaxProvider, Umbraco.Persistance.SqlCe") is null);
        }

        public override string View => ShouldDisplayView() ? base.View : "";


        public override bool RequiresExecution(DatabaseModel model)
        {
            return ShouldDisplayView();
        }

        private bool ShouldDisplayView()
        {
            //If the connection string is already present in web.config we don't need to show the settings page and we jump to installing/upgrading.
            var databaseSettings = _connectionStrings[Constants.System.UmbracoConnectionName];

            if (databaseSettings.IsConnectionStringConfigured())
            {
                try
                {
                    //Since a connection string was present we verify the db can connect and query
                    _ = _databaseBuilder.ValidateSchema();
                    return false;
                }
                catch (Exception ex)
                {
                    _logger.Error<DatabaseConfigureStep>(ex, "An error occurred, reconfiguring...");
                    //something went wrong, could not connect so probably need to reconfigure
                    return true;
                }
            }

            return true;
        }
    }
}
