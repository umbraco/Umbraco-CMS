// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Persistence.SqlServer.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Persistence.SqlServer;

[TestFixture]
public class SqlServerSyntaxProviderTests
{
    private static readonly DateTime _dateTime = new(2026, 11, 3, 10, 30, 15, 250);
    private static readonly DateTimeOffset _dateTimeOffset = new(2026, 11, 3, 10, 30, 15, 250, TimeSpan.FromHours(-1));
    private static readonly DateOnly _date = new(2026, 11, 3);
    private static readonly TimeOnly _time = new(10, 30, 15, 250);

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
    public void Can_Format_Guid_Unchanged()
    {
        var result = CreateSut().FormatGuid(new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"));

        Assert.That(result, Is.EqualTo("a1b2c3d4-e5f6-7890-abcd-ef1234567890"));
    }

    [TestCase(true, ExpectedResult = "20261103 10:30:15")]
    [TestCase(false, ExpectedResult = "20261103")]
    public string Can_Format_DateTime(bool includeTime) => CreateSut().FormatDateTime(_dateTime, includeTime);

    [Test]
    public void Can_Format_DateTimeOffset()
        => Assert.That(CreateSut().FormatDateTimeOffset(_dateTimeOffset), Is.EqualTo("2026-11-03T10:30:15.2500000-01:00"));

    [Test]
    public void Can_Format_DateOnly()
        => Assert.That(CreateSut().FormatDateOnly(_date), Is.EqualTo("20261103"));

    [Test]
    public void Can_Format_TimeOnly()
        => Assert.That(CreateSut().FormatTimeOnly(_time), Is.EqualTo("10:30:15.2500000"));

    [Test]
    public void Can_Quote_And_Escape_A_String_Value()
        => Assert.That(CreateSut().GetQuotedValue("O'Brien's"), Is.EqualTo("'O''Brien''s'"));

    [TestCase("", SqlServerSyntaxProvider.VersionName.Invalid)]
    [TestCase("13.0.7050.2", SqlServerSyntaxProvider.VersionName.V2016)]
    [TestCase("14.0.3485.1", SqlServerSyntaxProvider.VersionName.V2017)]
    [TestCase("15.0.4430.1", SqlServerSyntaxProvider.VersionName.V2019)]
    [TestCase("16.0.4195.2", SqlServerSyntaxProvider.VersionName.V2022)]
    [TestCase("17.0.1000.7", SqlServerSyntaxProvider.VersionName.V2025)]
    [TestCase("99.0.0.0", SqlServerSyntaxProvider.VersionName.Other)]
    public void Can_Map_Product_Version_To_Version_Name(string productVersion, SqlServerSyntaxProvider.VersionName expected)
    {
        var result = new SqlServerSyntaxProvider.ServerVersionInfo(
            "Developer Edition (64-bit)",
            "MSSQLSERVER",
            productVersion,
            SqlServerSyntaxProvider.EngineEdition.Enterprise,
            "localhost",
            "RTM");

        Assert.That(result.ProductVersionName, Is.EqualTo(expected));
    }

    private static SqlServerSyntaxProvider CreateSut() => new(Options.Create(new GlobalSettings()));
}
