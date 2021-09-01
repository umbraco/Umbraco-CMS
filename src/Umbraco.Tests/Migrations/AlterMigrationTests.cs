using System;
using System.Diagnostics;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations;
using Umbraco.Tests.Migrations.Stubs;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Migrations
{
    [TestFixture]
    public class AlterMigrationTests
    {
        private ILogger _logger;

        [SetUp]
        public void Setup()
        {
            _logger = Mock.Of<ILogger>();
        }

        [Test]
        public void Drop_Foreign_Key()
        {
            // Arrange
            var database = new TestDatabase();
            var context = new MigrationContext(database, _logger);
            var stub = new DropForeignKeyMigrationStub(context);

            // Act
            stub.Migrate();

            foreach (var op in database.Operations)
                Console.WriteLine("{0}\r\n\t{1}", op.Text, op.Sql);

            // Assert
            Assert.That(database.Operations.Count, Is.EqualTo(1));
            Assert.That(database.Operations[0].Sql, Is.EqualTo("ALTER TABLE [umbracoUser2app] DROP CONSTRAINT [FK_umbracoUser2app_umbracoUser_id]"));

        }

        [Test]
        public void CreateColumn()
        {
            var database = new TestDatabase();
            var context = new MigrationContext(database, _logger);
            var migration = new CreateColumnMigration(context);

            migration.Migrate();

            foreach (var op in database.Operations)
                Console.WriteLine("{0}\r\n\t{1}", op.Text, op.Sql);

            Assert.That(database.Operations.Count, Is.EqualTo(1));
            Assert.That(database.Operations[0].Sql, Is.EqualTo("ALTER TABLE [bar] ADD [foo] UniqueIdentifier NOT NULL"));
        }

        public class CreateColumnMigration : MigrationBase
        {
            public CreateColumnMigration(IMigrationContext context)
                : base(context)
            { }

            public override void Migrate()
            {
                Alter.Table("bar").AddColumn("foo").AsGuid().Do();
            }
        }

        [Test]
        public void AlterColumn()
        {
            var database = new TestDatabase();
            var context = new MigrationContext(database, _logger);
            var migration = new AlterColumnMigration(context);

            migration.Migrate();

            foreach (var op in database.Operations)
                Console.WriteLine("{0}\r\n\t{1}", op.Text, op.Sql);

            Assert.That(database.Operations.Count, Is.EqualTo(1));
            Assert.That(database.Operations[0].Sql, Is.EqualTo("ALTER TABLE [bar] ALTER COLUMN [foo] UniqueIdentifier NOT NULL"));
        }

        public class AlterColumnMigration : MigrationBase
        {
            public AlterColumnMigration(IMigrationContext context)
                : base(context)
            { }

            public override void Migrate()
            {
                // bad/good syntax...
                //Alter.Column("foo").OnTable("bar").AsGuid().NotNullable();
                Alter.Table("bar").AlterColumn("foo").AsGuid().NotNullable().Do();
            }
        }

        [NUnit.Framework.Ignore("this doesn't actually test anything")]
        [Test]
        public void Can_Get_Up_Migration_From_MigrationStub()
        {
            // Arrange
            var database = new TestDatabase();
            var context = new MigrationContext(database, _logger);
            var stub = new AlterUserTableMigrationStub(context);

            // Act
            stub.Migrate();

            // Assert
            Assert.That(database.Operations.Any(), Is.True);

            //Console output
            Debug.Print("Number of expressions in context: {0}", database.Operations.Count);
            Debug.Print("");
            foreach (var expression in database.Operations)
            {
                Debug.Print(expression.ToString());
            }
        }
    }
}
