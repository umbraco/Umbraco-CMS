using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnostics.Windows;
using BenchmarkDotNet.Jobs;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.Migrations.Initial;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Tests.TestHelpers;
using ILogger = Umbraco.Core.Logging.ILogger;

namespace Umbraco.Tests.Benchmarks
{
    [Config(typeof(Config))]
    public class BulkInsertBenchmarks
    {
        private class Config : ManualConfig
        {
            public Config()
            {
                Add(new MemoryDiagnoser());
                //Add(ExecutionValidator.FailOnError);

                //The 'quick and dirty' settings, so it runs a little quicker
                // see benchmarkdotnet FAQ
                Add(Job.Default
                    .WithLaunchCount(1) // benchmark process will be launched only once
                    .WithIterationTime(100) // 100ms per iteration
                    .WithWarmupCount(3) // 3 warmup iteration
                    .WithTargetCount(3)); // 3 target iteration                
            }
        }

        private static byte[] _initDbBytes;

        // fixme - should run on LocalDb same as NPoco tests!

        private UmbracoDatabase GetSqlServerDatabase(ILogger logger)
        {
            IDatabaseFactory f = null;
            var l = new Lazy<IDatabaseFactory>(() => f);
            f = new DefaultDatabaseFactory(
                "server=.\\SQLExpress;database=YOURDB;user id=YOURUSER;password=YOURPASS",
                Constants.DatabaseProviders.SqlServer,
                new [] { new SqlServerSyntaxProvider(l) },
                logger,
                new ThreadStaticUmbracoDatabaseAccessor(), 
                new MapperCollection(Enumerable.Empty<BaseMapper>()));
            return f.GetDatabase();
        }

        private UmbracoDatabase GetSqlCeDatabase(string cstr, ILogger logger)
        {
            var f = new DefaultDatabaseFactory(
                cstr,
                Constants.DatabaseProviders.SqlCe,
                new[] { new SqlCeSyntaxProvider() },
                logger,
                new ThreadStaticUmbracoDatabaseAccessor(),
                new MapperCollection(Enumerable.Empty<BaseMapper>()));
            return f.GetDatabase();
        }

        [Setup]
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
                    var creation = new DatabaseSchemaCreation(_dbSqlCe, logger);
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

        [Cleanup]
        public void Cleanup()
        {
            _dbSqlCe.Dispose();
            _dbSqlServer.Dispose();
            File.Delete(_dbFile);
        }

        private string _dbFile;
        private UmbracoDatabase _dbSqlCe;
        private UmbracoDatabase _dbSqlServer;

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