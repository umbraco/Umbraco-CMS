// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Tests.Common.TestHelpers;
using ColumnInfo = Umbraco.Cms.Infrastructure.Persistence.SqlSyntax.ColumnInfo;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Migrations;

/// <summary>
/// Tests the column helpers of <see cref="AsyncMigrationBase" /> against a <see cref="TestDatabase" />,
/// which records executed statements and throws on any attempt to read from the database.
/// A helper that is given the schema's column information must therefore not query the schema itself.
/// </summary>
[TestFixture]
public class MigrationColumnHelperTests
{
    private const string TableName = "testTable";

    private static readonly ColumnInfo[] ColumnsWithoutTestColumn =
    [
        new ColumnInfo(TableName, "id", 0, false, "int"),
        new ColumnInfo("otherTable", "id", 0, false, "int"),
    ];

    private static readonly ColumnInfo[] ColumnsWithTestColumn =
    [
        new ColumnInfo(TableName, "id", 0, false, "int"),
        new ColumnInfo(TableName, "testColumn", 1, false, "int"),
        new ColumnInfo("otherTable", "id", 0, false, "int"),
    ];

    [Test]
    public void Can_Add_Missing_Column_Using_Column_Information()
    {
        var migration = CreateMigration(out TestDatabase database);

        var added = migration.AddColumn<TestDto>(ColumnsWithoutTestColumn, "testColumn");

        Assert.IsTrue(added);
        Assert.AreEqual(1, database.Operations.Count);
        Assert.AreEqual("ALTER TABLE [testTable] ADD [testColumn] INTEGER NOT NULL", database.Operations[0].Sql);
    }

    [Test]
    public void Cannot_Add_Existing_Column_Using_Column_Information()
    {
        var migration = CreateMigration(out TestDatabase database);

        var added = migration.AddColumn<TestDto>(ColumnsWithTestColumn, "testColumn");

        Assert.IsFalse(added);
        Assert.IsEmpty(database.Operations);
    }

    [Test]
    public void Can_Add_Missing_Column_To_Specified_Table_Using_Column_Information()
    {
        var migration = CreateMigration(out TestDatabase database);

        var added = migration.AddColumn<TestDto>(ColumnsWithoutTestColumn, "otherTable", "testColumn");

        Assert.IsTrue(added);
        Assert.AreEqual(1, database.Operations.Count);
        Assert.AreEqual("ALTER TABLE [otherTable] ADD [testColumn] INTEGER NOT NULL", database.Operations[0].Sql);
    }

    [Test]
    public void Cannot_Add_Existing_Column_To_Specified_Table_Using_Column_Information()
    {
        var migration = CreateMigration(out TestDatabase database);

        var added = migration.AddColumn<TestDto>(ColumnsWithTestColumn, TableName, "testColumn");

        Assert.IsFalse(added);
        Assert.IsEmpty(database.Operations);
    }

    [Test]
    public void Cannot_Add_Existing_Column_With_Different_Casing()
    {
        var migration = CreateMigration(out TestDatabase database);

        var added = migration.AddColumn<TestDto>(ColumnsWithTestColumn, "TESTCOLUMN");

        Assert.IsFalse(added);
        Assert.IsEmpty(database.Operations);
    }

    [Test]
    public void Can_Add_Missing_Column_With_Different_Casing()
    {
        var migration = CreateMigration(out TestDatabase database);

        var added = migration.AddColumn<TestDto>(ColumnsWithoutTestColumn, "TESTCOLUMN");

        Assert.IsTrue(added);
        Assert.AreEqual(1, database.Operations.Count);
        Assert.AreEqual("ALTER TABLE [testTable] ADD [testColumn] INTEGER NOT NULL", database.Operations[0].Sql);
    }

    [Test]
    public void Cannot_Add_Column_Missing_From_Table_Definition()
    {
        var migration = CreateMigration(out TestDatabase database);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => migration.AddColumn<TestDto>(ColumnsWithoutTestColumn, "unknownColumn"));

        Assert.That(exception.Message, Does.Contain("testTable").And.Contain("unknownColumn"));
        Assert.IsEmpty(database.Operations);
    }

    [Test]
    public void Can_Add_Missing_Column_Using_Obsolete_AddColumnIfNotExists()
    {
        var migration = CreateMigration(out TestDatabase database);

        migration.AddColumnIfNotExists<TestDto>(ColumnsWithoutTestColumn, "testColumn");

        Assert.AreEqual(1, database.Operations.Count);
        Assert.AreEqual("ALTER TABLE [testTable] ADD [testColumn] INTEGER NOT NULL", database.Operations[0].Sql);
    }

    [Test]
    public void Cannot_Add_Existing_Column_Using_Obsolete_AddColumnIfNotExists()
    {
        var migration = CreateMigration(out TestDatabase database);

        migration.AddColumnIfNotExists<TestDto>(ColumnsWithTestColumn, "testColumn");

        Assert.IsEmpty(database.Operations);
    }

    [Test]
    public void Can_Check_Column_Exists_Using_Column_Information()
    {
        var migration = CreateMigration(out _);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(migration.ColumnExists(ColumnsWithTestColumn, "TESTTABLE", "TESTCOLUMN"));
            Assert.IsFalse(migration.ColumnExists(ColumnsWithoutTestColumn, TableName, "testColumn"));
            Assert.IsFalse(migration.ColumnExists(ColumnsWithTestColumn, TableName, "unknownColumn"));
        });
    }

    [Test]
    public void Can_Alter_Column_Using_Table_Definition_Name()
    {
        var migration = CreateMigration(out TestDatabase database);

        migration.AlterColumn<TestDto>("testColumn");

        Assert.AreEqual(1, database.Operations.Count);
        Assert.AreEqual("ALTER TABLE [testTable] ALTER COLUMN [testColumn] INTEGER NOT NULL", database.Operations[0].Sql);
    }

    [Test]
    public void Can_Replace_Column_Using_Table_Definition_Name()
    {
        var migration = CreateMigration(out TestDatabase database);

        migration.ReplaceColumn<TestDto>("oldColumn", "testColumn");

        Assert.AreEqual(2, database.Operations.Count);
        Assert.AreEqual("sp_rename 'testTable.oldColumn', 'testColumn', 'COLUMN'", database.Operations[0].Sql);
        Assert.AreEqual("ALTER TABLE [testTable] ALTER COLUMN [testColumn] INTEGER NOT NULL", database.Operations[1].Sql);
    }

    private static TestMigration CreateMigration(out TestDatabase database)
    {
        database = new TestDatabase();
        var context = new MigrationContext(new TestPlan(), database, Mock.Of<ILogger<MigrationContext>>());
        return new TestMigration(context);
    }

    private class TestPlan : MigrationPlan
    {
        public TestPlan()
            : base("Test")
        {
        }
    }

    [TableName(TableName)]
    [PrimaryKey("id", AutoIncrement = true)]
    [ExplicitColumns]
    private class TestDto
    {
        [Column("id")]
        [PrimaryKeyColumn(Name = "PK_testTable")]
        public int Id { get; set; }

        [Column("testColumn")]
        public int TestColumn { get; set; }
    }

    private class TestMigration : AsyncMigrationBase
    {
        public TestMigration(IMigrationContext context)
            : base(context)
        {
        }

        public new bool AddColumn<T>(IEnumerable<ColumnInfo> columns, string columnName)
            => base.AddColumn<T>(columns, columnName);

        public new bool AddColumn<T>(IEnumerable<ColumnInfo> columns, string tableName, string columnName)
            => base.AddColumn<T>(columns, tableName, columnName);

#pragma warning disable CS0618 // Type or member is obsolete
        public new void AddColumnIfNotExists<T>(IEnumerable<ColumnInfo> columns, string columnName)
            => base.AddColumnIfNotExists<T>(columns, columnName);
#pragma warning restore CS0618 // Type or member is obsolete

        public new bool ColumnExists(IEnumerable<ColumnInfo> columns, string tableName, string columnName)
            => AsyncMigrationBase.ColumnExists(columns, tableName, columnName);

        public new void AlterColumn<T>(string columnName)
            => base.AlterColumn<T>(columnName);

        public new void ReplaceColumn<T>(string currentName, string newName)
            => base.ReplaceColumn<T>(currentName, newName);

        protected override Task MigrateAsync() => Task.CompletedTask;
    }
}
