// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Tests.Common.TestHelpers;
using Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Migrations.Stubs;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Migrations;

    /// <summary>
    /// Contains unit tests for verifying the behavior of database schema alterations in migrations.
    /// </summary>
[TestFixture]
public class AlterMigrationTests
{
    private readonly ILogger<MigrationContext> _logger = Mock.Of<ILogger<MigrationContext>>();

    private class TestPlan : MigrationPlan
    {
    /// <summary>
    /// Initializes a new instance of the <see cref="TestPlan"/> class.
    /// </summary>
        public TestPlan()
            : base("Test")
        {
        }
    }

    private MigrationContext GetMigrationContext(out TestDatabase db)
    {
        db = new TestDatabase();
        return new MigrationContext(new TestPlan(), db, _logger);
    }

    /// <summary>
    /// Tests that a foreign key can be dropped correctly by the migration.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task Drop_Foreign_Key()
    {
        // Arrange
        var context = GetMigrationContext(out var database);
        var stub = new DropForeignKeyMigrationStub(context);

        // Act
        await stub.RunAsync().ConfigureAwait(false);

        foreach (var op in database.Operations)
        {
            Console.WriteLine("{0}\r\n\t{1}", op.Text, op.Sql);
        }

        // Assert
        Assert.That(database.Operations.Count, Is.EqualTo(1));
        Assert.That(database.Operations[0].Sql, Is.EqualTo("ALTER TABLE [umbracoUser2app] DROP CONSTRAINT [FK_umbracoUser2app_umbracoUser_id]"));
    }

    /// <summary>
    /// Unit test that verifies the creation of a new column in a database table using a migration.
    /// Ensures that the correct SQL statement is generated to add a unique identifier column.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CreateColumn()
    {
        var context = GetMigrationContext(out var database);
        var migration = new CreateColumnMigration(context);

        await migration.RunAsync().ConfigureAwait(false);

        foreach (var op in database.Operations)
        {
            Console.WriteLine("{0}\r\n\t{1}", op.Text, op.Sql);
        }

        Assert.That(database.Operations.Count, Is.EqualTo(1));
        Assert.That(
            database.Operations[0].Sql,
            Is.EqualTo("ALTER TABLE [bar] ADD [foo] UniqueIdentifier NOT NULL"));
    }

    /// <summary>
    /// Represents a test migration used to create a new column in the database schema.
    /// This class is intended for use in unit tests.
    /// </summary>
    public class CreateColumnMigration : MigrationBase
    {
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateColumnMigration"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
        public CreateColumnMigration(IMigrationContext context)
            : base(context)
        {
        }

        protected override void Migrate() => Alter.Table("bar").AddColumn("foo").AsGuid().Do();
    }

    /// <summary>
    /// Tests that the <see cref="AlterColumnMigration"/> correctly alters a column in the database schema.
    /// Verifies that the generated SQL operation matches the expected ALTER TABLE statement for modifying a column type.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Test]
    public async Task AlterColumn()
    {
        var context = GetMigrationContext(out var database);
        var migration = new AlterColumnMigration(context);

        await migration.RunAsync().ConfigureAwait(false);

        foreach (var op in database.Operations)
        {
            Console.WriteLine("{0}\r\n\t{1}", op.Text, op.Sql);
        }

        Assert.That(database.Operations.Count, Is.EqualTo(1));
        Assert.That(
            database.Operations[0].Sql,
            Is.EqualTo("ALTER TABLE [bar] ALTER COLUMN [foo] UniqueIdentifier NOT NULL"));
    }

    /// <summary>
    /// Contains unit tests for verifying the behavior of altering columns within database migrations
    /// in the Umbraco CMS infrastructure.
    /// </summary>
    public class AlterColumnMigration : MigrationBase
    {
    /// <summary>
    /// Initializes a new instance of the <see cref="AlterColumnMigration"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
        public AlterColumnMigration(IMigrationContext context)
            : base(context)
        {
        }

        protected override void Migrate() =>

            // bad/good syntax...
            //// Alter.Column("foo").OnTable("bar").AsGuid().NotNullable();
            Alter.Table("bar").AlterColumn("foo").AsGuid().NotNullable().Do();
    }

    /// <summary>
    /// Verifies that the up migration can be obtained and executed from the <see cref="AlterUserTableMigrationStub"/>.
    /// The test asserts that running the migration results in at least one database operation being performed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Ignore("this doesn't actually test anything")]
    [Test]
    public async Task Can_Get_Up_Migration_From_MigrationStub()
    {
        // Arrange
        var context = GetMigrationContext(out var database);
        var stub = new AlterUserTableMigrationStub(context);

        // Act
        await stub.RunAsync().ConfigureAwait(false);

        // Assert
        Assert.That(database.Operations.Any(), Is.True);

        // Console output
        Debug.Print("Number of expressions in context: {0}", database.Operations.Count);
        Debug.Print(string.Empty);
        foreach (var expression in database.Operations)
        {
            Debug.Print(expression.ToString());
        }
    }
}
