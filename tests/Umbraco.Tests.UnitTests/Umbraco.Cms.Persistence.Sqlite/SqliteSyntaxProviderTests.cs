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
    private static readonly DateTime _dateTime = new(2026, 11, 3, 10, 30, 15, 250);
    private static readonly DateTimeOffset _dateTimeOffset = new(2026, 11, 3, 10, 30, 15, 250, TimeSpan.FromHours(-1));
    private static readonly DateOnly _date = new(2026, 11, 3);
    private static readonly TimeOnly _time = new(10, 30, 15, 250);
    private static readonly TimeOnly _wholeSecondTime = new(10, 30, 15);

    private CultureInfo _originalCulture = null!;

    // fi-FI formats dates, times, decimals and negative signs differently from the invariant culture,
    // so any formatting that depends on the current culture shows up in the results.
    [SetUp]
    public void SetUp()
    {
        _originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fi-FI");
    }

    [TearDown]
    public void TearDown() => CultureInfo.CurrentCulture = _originalCulture;

    [Test]
    public void Can_Format_Guid_Uppercase()
    {
        var result = CreateSut().FormatGuid(new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"));

        Assert.That(result, Is.EqualTo("A1B2C3D4-E5F6-7890-ABCD-EF1234567890"));
    }

    [TestCase(true, ExpectedResult = "20261103 10:30:15")]
    [TestCase(false, ExpectedResult = "20261103")]
    public string Can_Format_DateTime(bool includeTime) => CreateSut().FormatDateTime(_dateTime, includeTime);

    [Test]
    public void Can_Format_DateTimeOffset()
        => Assert.That(CreateSut().FormatDateTimeOffset(_dateTimeOffset), Is.EqualTo("2026-11-03 10:30:15.25-01:00"));

    [Test]
    public void Can_Format_DateOnly()
        => Assert.That(CreateSut().FormatDateOnly(_date), Is.EqualTo("2026-11-03"));

    [Test]
    public void Can_Format_TimeOnly()
        => Assert.That(CreateSut().FormatTimeOnly(_time), Is.EqualTo("10:30:15.2500000"));

    [Test]
    public void Can_Format_TimeOnly_Without_Fractional_Seconds()
        => Assert.That(CreateSut().FormatTimeOnly(_wholeSecondTime), Is.EqualTo("10:30:15"));

    [Test]
    public void Can_Quote_And_Escape_A_String_Value()
        => Assert.That(CreateSut().GetQuotedValue("O'Brien's"), Is.EqualTo("'O''Brien''s'"));

    [Test]
    public void Date_And_Time_Values_Are_Formatted_As_Sqlite_Stores_Parameter_Values()
    {
        SqliteSyntaxProvider sut = CreateSut();
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        Assert.Multiple(() =>
        {
            Assert.That(sut.FormatDateTimeOffset(_dateTimeOffset), Is.EqualTo(GetStoredText(connection, _dateTimeOffset)));
            Assert.That(sut.FormatDateOnly(_date), Is.EqualTo(GetStoredText(connection, _date)));
            Assert.That(sut.FormatTimeOnly(_time), Is.EqualTo(GetStoredText(connection, _time)));
            Assert.That(sut.FormatTimeOnly(_wholeSecondTime), Is.EqualTo(GetStoredText(connection, _wholeSecondTime)));
        });
    }

    private static SqliteSyntaxProvider CreateSut()
        => new(Options.Create(new GlobalSettings()), Mock.Of<ILogger<SqliteSyntaxProvider>>());

    private static string GetStoredText(SqliteConnection connection, object value)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT @value";
        command.Parameters.AddWithValue("@value", value);
        return (string)command.ExecuteScalar()!;
    }
}
