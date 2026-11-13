// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
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

    [Test]
    public void Can_Order_Integers_Numerically_By_Orderable_String()
    {
        long[] values = [3, -5, 123456789, 123456780, -6, 0, int.MaxValue, int.MinValue, -40, -100000000, 100000000];

        IEnumerable<long> ordered = OrderByExpression(CreateSut().ConvertIntegerToOrderableString, "INTEGER", values.Cast<object>(), x => (long)x);

        Assert.That(ordered, Is.EqualTo(values.Order()));
    }

    [Test]
    public void Can_Order_Decimals_Numerically_By_Orderable_String()
    {
        // Decimals are stored as text, so this covers the whole System.Decimal range, including digits a floating
        // point conversion would lose.
        decimal[] values =
        [
            0m, 7m, -5m, 1.5m, 1.25m, -1.25m, -1.5m, 2.25m, 10.1m, -0.25m, -12345.678m,
            0.000001m, -0.000001m, 0.0000000000000000000000000001m, -0.0000000000000000000000000001m,
            99999999998.999999m, 99999999998.999998m, -99999999998.999999m, -99999999998.999998m,
            decimal.MaxValue, decimal.MinValue, decimal.MaxValue - 1, decimal.MinValue + 1,
        ];

        IEnumerable<decimal> ordered = OrderByExpression(
            CreateSut().ConvertDecimalToOrderableString,
            "TEXT",
            values.Select(x => (object)x.ToString(CultureInfo.InvariantCulture)),
            x => decimal.Parse((string)x, CultureInfo.InvariantCulture));

        Assert.That(ordered, Is.EqualTo(values.Order()));
    }

    private static SqliteSyntaxProvider CreateSut()
        => new(Options.Create(new GlobalSettings()), Mock.Of<ILogger<SqliteSyntaxProvider>>());

    private static IEnumerable<T> OrderByExpression<T>(string expression, string columnType, IEnumerable<object> values, Func<object, T> read)
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        using (SqliteCommand create = connection.CreateCommand())
        {
            create.CommandText = $"CREATE TABLE sortTest (sortValue {columnType})";
            create.ExecuteNonQuery();
        }

        foreach (var value in values)
        {
            using SqliteCommand insert = connection.CreateCommand();
            insert.CommandText = "INSERT INTO sortTest (sortValue) VALUES (@value)";
            insert.Parameters.AddWithValue("@value", value);
            insert.ExecuteNonQuery();
        }

        using SqliteCommand select = connection.CreateCommand();
        select.CommandText = $"SELECT sortValue FROM sortTest ORDER BY {string.Format(expression, "sortValue")}";
        using SqliteDataReader reader = select.ExecuteReader();

        var ordered = new List<T>();
        while (reader.Read())
        {
            ordered.Add(read(reader.GetValue(0)));
        }

        return ordered;
    }
}
