using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Umbraco.Configuration.Models;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Tests.Integration.Testing;

namespace Umbraco.Tests.Integration.Implementations
{
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Creates a LocalDb instance to use for the test
        /// </summary>
        /// <param name="app"></param>
        /// <param name="dbFilePath"></param>
        /// <param name="createNewDb">
        /// Default is true - meaning a brand new database is created for this test. If this is false it will try to
        /// re-use an existing database that was already created as part of this test fixture.
        /// // TODO Implement the 'false' behavior
        /// </param>
        /// <param name="initializeSchema">
        /// Default is true - meaning a database schema will be created for this test if it's a new database. If this is false
        /// it will just create an empty database. 
        /// </param>
        /// <returns></returns>
        public static IApplicationBuilder UseTestLocalDb(this IApplicationBuilder app,
            string dbFilePath,
            bool createNewDb = true,
            bool initializeSchema = true)
        {
            // need to manually register this factory
            DbProviderFactories.RegisterFactory(Constants.DbProviderNames.SqlServer, SqlClientFactory.Instance);

            if (!Directory.Exists(dbFilePath))
                Directory.CreateDirectory(dbFilePath);

            var db = LocalDbTestDatabase.Get(dbFilePath,
                app.ApplicationServices.GetRequiredService<ILogger>(),
                app.ApplicationServices.GetRequiredService<IGlobalSettings>(),
                app.ApplicationServices.GetRequiredService<IUmbracoDatabaseFactory>());

            if (initializeSchema)
            {
                // New DB + Schema
                db.AttachSchema();

                // In the case that we've initialized the schema, it means that we are installed so we'll want to ensure that
                // the runtime state is configured correctly so we'll force update the configuration flag and re-run the
                // runtime state checker.
                // TODO: This wouldn't be required if we don't store the Umbraco version in config

                // right now we are an an 'Install' state
                var runtimeState = (RuntimeState)app.ApplicationServices.GetRequiredService<IRuntimeState>();
                Assert.AreEqual(RuntimeLevel.Install, runtimeState.Level);

                // dynamically change the config status
                var umbVersion = app.ApplicationServices.GetRequiredService<IUmbracoVersion>();
                var config = app.ApplicationServices.GetRequiredService<IConfiguration>();
                config[GlobalSettings.Prefix + "ConfigurationStatus"] = umbVersion.SemanticVersion.ToString();

                // re-run the runtime level check
                var dbFactory = app.ApplicationServices.GetRequiredService<IUmbracoDatabaseFactory>();
                var profilingLogger = app.ApplicationServices.GetRequiredService<IProfilingLogger>();
                runtimeState.DetermineRuntimeLevel(dbFactory, profilingLogger);

                Assert.AreEqual(RuntimeLevel.Run, runtimeState.Level);

            }
            else
            {
                db.AttachEmpty();
            }

            return app;
        }

    }


}
