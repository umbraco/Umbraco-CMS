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

        var database = new TestDatabase();
        var context = new MigrationContext(new TestPlan(), database, Mock.Of<ILogger<MigrationContext>>());
        var expression = new InsertDataExpression(context) { TableName = "umbracoLock" };
        expression.Rows.Add(
            new InsertionDataDefinition
            {
                new("id", -73001),
                new("ratio", 1.5m),
                new("factor", -2.25d),
                new("big", -9000000000L),
            });

        expression.Execute();

        var sql = database.Operations.Single().Sql;
        Assert.That(sql, Does.EndWith("VALUES (-73001,1.5,-2.25,-9000000000);"));
    }

    private class TestPlan : MigrationPlan
    {
        public TestPlan()
            : base("Test")
        {
        }
    }
}
