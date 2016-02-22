using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence.SyntaxProvider
{
    //[NUnit.Framework.Ignore("This doesn't actually test anything")]
    [TestFixture]
    public class MySqlSyntaxProviderTests : BaseUsingSqlCeSyntax
    {
        [SetUp]
        public void SetUp()
        {
            SqlSyntaxContext.SqlSyntaxProvider = new MySqlSyntaxProvider(Mock.Of<ILogger>());
        }

        [Test]
        [NUnit.Framework.Ignore]
        public void Can_Generate_Create_Table_Statement()
        {
            var type = typeof(TagRelationshipDto);
            var definition = DefinitionFactory.GetTableDefinition(type);

            string create = SqlSyntaxContext.SqlSyntaxProvider.Format(definition);
            string primaryKey = SqlSyntaxContext.SqlSyntaxProvider.FormatPrimaryKey(definition);
            var indexes = SqlSyntaxContext.SqlSyntaxProvider.Format(definition.Indexes);
            var keys = SqlSyntaxContext.SqlSyntaxProvider.Format(definition.ForeignKeys);

            Console.WriteLine(create);
            Console.WriteLine(primaryKey);
            foreach (var sql in keys)
            {
                Console.WriteLine(sql);
            }

            foreach (var sql in indexes)
            {
                Console.WriteLine(sql);
            }
        }

        [TearDown]
        public void TearDown()
        {
            SqlSyntaxContext.SqlSyntaxProvider = null;
        }

        [Test]
        public void U4_8017()
        {
            var logger = Mock.Of<ILogger>();
            var migration = new TestMigration(SqlSyntaxContext.SqlSyntaxProvider, logger);
            var context = new MigrationContext(DatabaseProviders.MySql, null, logger);
            migration.Context = context;
            migration.Up();
            var x = context.Expressions;
            Assert.AreEqual(1, x.Count);
            var e = x.First().ToString();

            // SQLCE provider *does* use UniqueIdentifier
            // MySql using GUID...? because InitColumnTypeMap() missing in provider, fixed

            Assert.AreEqual("ALTER TABLE `cmsPropertyTypeGroup` ADD COLUMN `uniqueID` char(36) NOT NULL", e);
        }

        [Migration("1.0.0", 0, "Test")]
        private class TestMigration : MigrationBase
        {
            public TestMigration(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
            {}

            public override void Up()
            {
                Create.Column("uniqueID").OnTable("cmsPropertyTypeGroup").AsGuid().NotNullable().WithDefault(SystemMethods.NewGuid);
            }

            public override void Down()
            {
                throw new NotImplementedException();
            }
        }
    }
}