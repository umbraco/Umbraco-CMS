using System.Globalization;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Insert.Expressions;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;
using Umbraco.Cms.Tests.Common.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Migrations;

[TestFixture]
public class MigrationExpressionBaseTests
{
    private CultureInfo _originalCulture = null!;

    [SetUp]
    public void SetUp() => _originalCulture = CultureInfo.CurrentCulture;

    [TearDown]
    public void TearDown() => CultureInfo.CurrentCulture = _originalCulture;

    [TestCase("fo-FO")]
    [TestCase("sv-SE")]
    [TestCase("nb-NO")]
    [TestCase("fi-FI")]
    [TestCase("da-DK")]
    [TestCase("de-DE")]
    [TestCase("en-US")]
    public void Numeric_Values_Are_Formatted_Using_The_Invariant_Culture(string cultureName)
    {
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);

        var sql = ExecuteInsert(
            new InsertionDataDefinition
            {
                new("id", -73001),
                new("ratio", 1.5m),
                new("factor", -2.25d),
                new("big", -9000000000L),
            });

        Assert.That(sql, Does.EndWith("VALUES (-73001,1.5,-2.25,-9000000000);"));
    }

    [TestCase("fo-FO")]
    [TestCase("sv-SE")]
    [TestCase("nb-NO")]
    [TestCase("fi-FI")]
    [TestCase("da-DK")]
    [TestCase("de-DE")]
    [TestCase("en-US")]
    public void Date_And_Time_Values_Are_Formatted_By_The_Syntax_Provider_Independently_Of_The_Culture(string cultureName)
    {
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);

        var sql = ExecuteInsert(
            new InsertionDataDefinition
            {
                new("date", new DateOnly(2026, 11, 3)),
                new("time", new TimeOnly(10, 30, 15)),
                new("moment", new DateTimeOffset(2026, 11, 3, 10, 30, 15, TimeSpan.FromHours(-1))),
            });

        Assert.That(sql, Does.EndWith("VALUES ('20261103','10:30:15.0000000','2026-11-03T10:30:15.0000000-01:00');"));
    }

    [Test]
    public void Single_Quotes_In_String_Values_Are_Escaped()
    {
        var sql = ExecuteInsert(
            new InsertionDataDefinition
            {
                new("name", "O'Brien's 'quoted'"),
            });

        Assert.That(sql, Does.EndWith("VALUES ('O''Brien''s ''quoted''');"));
    }

    private static string ExecuteInsert(InsertionDataDefinition row)
    {
        var database = new TestDatabase();
        var context = new MigrationContext(new TestPlan(), database, Mock.Of<ILogger<MigrationContext>>());
        var expression = new InsertDataExpression(context) { TableName = "umbracoLock" };
        expression.Rows.Add(row);

        expression.Execute();

        return database.Operations.Single().Sql;
    }

    private class TestPlan : MigrationPlan
    {
        public TestPlan()
            : base("Test")
        {
        }
    }
}
