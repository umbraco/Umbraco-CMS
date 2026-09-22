// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Persistence.Sqlite.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Persistence.Sqlite;

[TestFixture]
public class SqliteSyntaxProviderTests
{
    [Test]
    public void Can_Format_Guid_Uppercase()
    {
        var sut = new SqliteSyntaxProvider(
            Options.Create(new GlobalSettings()),
            Mock.Of<ILogger<SqliteSyntaxProvider>>());

        var result = sut.FormatGuid(new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"));

        Assert.That(result, Is.EqualTo("A1B2C3D4-E5F6-7890-ABCD-EF1234567890"));
    }

    /// <summary>
    ///     A column added to a table that already holds rows has to give those rows a value, so a non-nullable
    ///     column needs a default even when the type it defaults to has no SQLite equivalent.
    /// </summary>
    [TestCase(typeof(ContentVersionDto), ContentVersionDto.KeyColumnName)]
    [TestCase(typeof(DomainDto), DomainDto.KeyColumnName)]
    public void Add_Column_Gives_A_Non_Nullable_Column_A_Default(Type dtoType, string columnName)
    {
        var sut = new SqliteSyntaxProvider(
            Options.Create(new GlobalSettings()),
            Mock.Of<ILogger<SqliteSyntaxProvider>>());

        TableDefinition table = DefinitionFactory.GetTableDefinition(dtoType, sut);
        ColumnDefinition column = table.Columns.First(x => x.Name == columnName);

        var sql = sut.FormatAddColumn(column);

        Assert.Multiple(() =>
        {
            Assert.That(sql, Does.Contain("NOT NULL"));
            Assert.That(sql, Does.Contain("DEFAULT"), "SQLite rejects a NOT NULL column with no default on a populated table");
        });
    }

    /// <summary>
    ///     The formatted definition has to be something SQLite will actually accept for a table that already
    ///     holds rows, which is the case the statement is generated for.
    /// </summary>
    [Test]
    public void Add_Column_Statement_Runs_Against_A_Populated_Table()
    {
        var sut = new SqliteSyntaxProvider(
            Options.Create(new GlobalSettings()),
            Mock.Of<ILogger<SqliteSyntaxProvider>>());

        TableDefinition table = DefinitionFactory.GetTableDefinition(typeof(ContentVersionDto), sut);
        ColumnDefinition column = table.Columns.First(x => x.Name == ContentVersionDto.KeyColumnName);

        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        Execute(connection, "CREATE TABLE [umbracoContentVersion] ([id] INTEGER PRIMARY KEY AUTOINCREMENT, [nodeId] INTEGER NOT NULL)");
        Execute(connection, "INSERT INTO [umbracoContentVersion] ([nodeId]) VALUES (1)");

        var sql = string.Format(
            sut.AddColumn,
            sut.GetQuotedTableName(ContentVersionDto.TableName),
            sut.FormatAddColumn(column));

        Assert.DoesNotThrow(() => Execute(connection, sql), sql);
    }

    private static void Execute(SqliteConnection connection, string sql)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}
