using System;
using System.Configuration;
using System.Data.SqlServerCe;
using System.IO;
using System.Text.RegularExpressions;
using Moq;
using NUnit.Framework;
using SQLCE4Umbraco;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations.Initial;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Migrations.Upgrades
{
    [TestFixture]
    public class ValidateOlderSchemaTest
    {
        /// <summary>Regular expression that finds multiline block comments.</summary>
        private static readonly Regex FindComments = new Regex(@"\/\*.*?\*\/", RegexOptions.Singleline | RegexOptions.Compiled);

        [Test]
        public virtual void DatabaseSchemaCreation_Returns_DatabaseSchemaResult_Where_DetermineInstalledVersion_Is_4_7_0()
        {
            // Arrange
            var db = GetConfiguredDatabase();
            var schema = new DatabaseSchemaCreation(db, Mock.Of<ILogger>(), new SqlCeSyntaxProvider());

            //Create db schema and data from old Total.sql file for Sql Ce
            string statements = GetDatabaseSpecificSqlScript();
            // replace block comments by whitespace
            statements = FindComments.Replace(statements, " ");
            // execute all non-empty statements
            foreach (string statement in statements.Split(";".ToCharArray()))
            {
                string rawStatement = statement.Replace("GO", "").Trim();
                if (rawStatement.Length > 0)
                    db.Execute(new Sql(rawStatement));
            }

            // Act
            var result = schema.ValidateSchema();

            // Assert
            var expected = new Version(4, 7, 0);
            Assert.AreEqual(expected, result.DetermineInstalledVersion());
        }

        [SetUp]
        public virtual void Initialize()
        {
            TestHelper.InitializeContentDirectories();

            Path = TestHelper.CurrentAssemblyDirectory;
            AppDomain.CurrentDomain.SetData("DataDirectory", Path);

            //Delete database file before continueing
            string filePath = string.Concat(Path, "\\UmbracoPetaPocoTests.sdf");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            //Get the connectionstring settings from config
            var settings = ConfigurationManager.ConnectionStrings[Core.Configuration.GlobalSettings.UmbracoConnectionName];

            Resolution.Freeze();

            //Create the Sql CE database
            var engine = new SqlCeEngine(settings.ConnectionString);
            engine.CreateDatabase();

            SqlSyntaxContext.SqlSyntaxProvider = new SqlCeSyntaxProvider();
        }

        [TearDown]
        public virtual void TearDown()
        {
            SqlSyntaxContext.SqlSyntaxProvider = null;
            Resolution.Reset();

            TestHelper.CleanContentDirectories();

            Path = TestHelper.CurrentAssemblyDirectory;
            AppDomain.CurrentDomain.SetData("DataDirectory", null);

            //legacy API database connection close
            SqlCeContextGuardian.CloseBackgroundConnection();

            string filePath = string.Concat(Path, "\\UmbracoPetaPocoTests.sdf");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        
        public string Path { get; set; }

        public UmbracoDatabase GetConfiguredDatabase()
        {
            return new UmbracoDatabase("Datasource=|DataDirectory|UmbracoPetaPocoTests.sdf;Flush Interval=1;", "System.Data.SqlServerCe.4.0", Mock.Of<ILogger>());
        }

        public string GetDatabaseSpecificSqlScript()
        {
            return SqlScripts.SqlResources.SqlCeTotal_480;
        }
    }
}