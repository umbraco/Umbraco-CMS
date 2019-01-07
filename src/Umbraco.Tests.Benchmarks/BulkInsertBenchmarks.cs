using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations.Install;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Scoping;
using Umbraco.Tests.Benchmarks.Config;
using Umbraco.Tests.TestHelpers;
using ILogger = Umbraco.Core.Logging.ILogger;

namespace Umbraco.Tests.Benchmarks
{
    [QuickRunWithMemoryDiagnoserConfig]
    public class BulkInsertBenchmarks
    {
        private static byte[] _initDbBytes;

        // fixme - should run on LocalDb same as NPoco tests!

        private IUmbracoDatabase GetSqlServerDatabase(ILogger logger)
        {
            IScopeProvider f = null;
            var l = new Lazy<IScopeProvider>(() => f);
            var factory = new UmbracoDatabaseFactory(
                "server=.\\SQLExpress;database=YOURDB;user id=YOURUSER;password=YOURPASS",
                Constants.DatabaseProviders.SqlServer,
                logger,
                new Lazy<IMapperCollection>(() => new MapperCollection(Enumerable.Empty<BaseMapper>())));
            return factory.CreateDatabase();
        }

        private IUmbracoDatabase GetSqlCeDatabase(string cstr, ILogger logger)
        {
            var f = new UmbracoDatabaseFactory(
                cstr,
                Constants.DatabaseProviders.SqlCe,
                logger,
                new Lazy<IMapperCollection>(() => new MapperCollection(Enumerable.Empty<BaseMapper>())));
            return f.CreateDatabase();
        }

        [GlobalSetup]
        public void Setup()
        {
            var logger = new DebugDiagnosticsLogger();
            var path = TestHelper.CurrentAssemblyDirectory;

            SetupSqlCe(path, logger);
            SetupSqlServer(logger);


        }

        private void SetupSqlServer(ILogger logger)
        {
            //create the db
            _dbSqlServer = GetSqlServerDatabase(logger);

            //drop the table
            // note: DROP TABLE IF EXISTS is SQL 2016+
            _dbSqlServer.Execute("IF OBJECT_ID('dbo.umbracoServer', 'U') IS NOT NULL DROP TABLE [umbracoServer]");

            //re-create it
            _dbSqlServer.Execute(@"CREATE TABLE [umbracoServer](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [address] [nvarchar](500) NOT NULL,
    [computerName] [nvarchar](255) NOT NULL,
    [registeredDate] [datetime] NOT NULL CONSTRAINT [DF_umbracoServer_registeredDate]  DEFAULT (getdate()),
    [lastNotifiedDate] [datetime] NOT NULL,
    [isActive] [bit] NOT NULL,
    [isMaster] [bit] NOT NULL,
 CONSTRAINT [PK_umbracoServer] PRIMARY KEY CLUSTERED
(
    [id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
)");
        }

        private void SetupSqlCe(string path, ILogger logger)
        {
            var dbName = string.Concat("Umb", Guid.NewGuid(), ".sdf");
            AppDomain.CurrentDomain.SetData("DataDirectory", path);
            var sqlCeConnectionString = $"Datasource=|DataDirectory|\\{dbName};Flush Interval=1;";

            _dbFile = Path.Combine(path, dbName);

            //only create the db one time
            if (_initDbBytes == null)
            {
                using (var engine = new SqlCeEngine(sqlCeConnectionString))
                {
                    engine.CreateDatabase();
                }

                //use the db  to create the initial schema so we can reuse in each bench
                using (_dbSqlCe = GetSqlCeDatabase(sqlCeConnectionString, logger))
                {
                    var creation = new DatabaseSchemaCreator(_dbSqlCe, logger);
                    creation.InitializeDatabaseSchema();
                }
                _initDbBytes = File.ReadAllBytes(_dbFile);
            }
            else
            {
                File.WriteAllBytes(_dbFile, _initDbBytes);
            }

            //create the db
            _dbSqlCe = GetSqlCeDatabase(sqlCeConnectionString, logger);
        }

        private List<ServerRegistrationDto> GetData()
        {
            var data = new List<ServerRegistrationDto>();
            for (var i = 0; i < 1000; i++)
            {
                data.Add(new ServerRegistrationDto
                {
                    ServerAddress = "address" + Guid.NewGuid(),
                    ServerIdentity = "computer" + Guid.NewGuid(),
                    DateRegistered = DateTime.Now,
                    IsActive = true,
                    DateAccessed = DateTime.Now
                });
            }
            return data;
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _dbSqlCe.Dispose();
            _dbSqlServer.Dispose();
            File.Delete(_dbFile);
        }

        private string _dbFile;
        private IUmbracoDatabase _dbSqlCe;
        private IUmbracoDatabase _dbSqlServer;

        /// <summary>
        /// Tests updating the existing XML way
        /// </summary>
        [Benchmark(Baseline = true)]
        public void SqlCeOneByOne()
        {
            using (var tr = _dbSqlCe.GetTransaction())
            {
                _dbSqlCe.BulkInsertRecords(GetData(), false);
                tr.Complete();
            }
        }

        /// <summary>
        /// Tests updating with only the object graph
        /// </summary>
        [Benchmark]
        public void SqlCeTableDirect()
        {
            using (var tr = _dbSqlCe.GetTransaction())
            {
                _dbSqlCe.BulkInsertRecords(GetData());
                tr.Complete();
            }
        }

        [Benchmark]
        public void SqlServerBulkInsertStatements()
        {
            using (var tr = _dbSqlServer.GetTransaction())
            {
                _dbSqlServer.BulkInsertRecords(GetData(), false);
                tr.Complete();
            }
        }

        [Benchmark]
        public void SqlServerBulkCopy()
        {
            using (var tr = _dbSqlServer.GetTransaction())
            {
                _dbSqlServer.BulkInsertRecords(GetData());
                tr.Complete();
            }
        }

    }
}
