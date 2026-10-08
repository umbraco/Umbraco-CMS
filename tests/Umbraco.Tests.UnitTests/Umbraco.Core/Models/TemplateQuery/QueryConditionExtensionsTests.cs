using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Models.TemplateQuery;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models.TemplateQuery;

[TestFixture]
public class QueryConditionExtensionsTests
{
    [TestCase("Name", "string")]
    [TestCase("Id", "int")]
    [TestCase("CreateDate", "datetime")]
    [TestCase("UpdateDate", "datetime")]
    public void Can_Build_Condition_For_Properties_Used_In_Template_Query_Builder(string alias, string type)
    {
        var condition = new QueryCondition
        {
            Property = new PropertyModel { Alias = alias, Type = type },
            Term = new OperatorTerm { Operator = Operator.Equals },
            ConstraintValue = type switch
            {
                "int" => "123",
                "datetime" => "2024-01-01",
                _ => "Test",
            },
        };

        Assert.That(() => condition.BuildCondition<IPublishedContent>("x"), Throws.Nothing);
    }

    [Test]
    public void Can_Build_And_Evaluate_Name_Equals_Condition()
    {
        var condition = new QueryCondition
        {
            Property = new PropertyModel { Alias = "Name", Type = "string" },
            Term = new OperatorTerm { Operator = Operator.Equals },
            ConstraintValue = "Test",
        };

        Func<IPublishedContent, bool> predicate = condition.BuildCondition<IPublishedContent>("x").Compile();

        Assert.Multiple(() =>
        {
            Assert.That(predicate(MockContent(name: "Test")), Is.True);
            Assert.That(predicate(MockContent(name: "Other")), Is.False);
        });
    }

    [Test]
    public void Can_Build_And_Evaluate_Id_Equals_Condition()
    {
        var condition = new QueryCondition
        {
            Property = new PropertyModel { Alias = "Id", Type = "int" },
            Term = new OperatorTerm { Operator = Operator.Equals },
            ConstraintValue = "123",
        };

        Func<IPublishedContent, bool> predicate = condition.BuildCondition<IPublishedContent>("x").Compile();

        Assert.Multiple(() =>
        {
            Assert.That(predicate(MockContent(id: 123)), Is.True);
            Assert.That(predicate(MockContent(id: 456)), Is.False);
        });
    }

    [TestCase("CreateDate")]
    [TestCase("UpdateDate")]
    public void Can_Build_And_Evaluate_Date_GreaterThan_Condition(string alias)
    {
        var condition = new QueryCondition
        {
            Property = new PropertyModel { Alias = alias, Type = "datetime" },
            Term = new OperatorTerm { Operator = Operator.GreaterThan },
            ConstraintValue = "2024-01-01",
        };

        Func<IPublishedContent, bool> predicate = condition.BuildCondition<IPublishedContent>("x").Compile();

        Assert.Multiple(() =>
        {
            Assert.That(predicate(MockContentWithDate(alias, new DateTime(2024, 06, 01))), Is.True);
            Assert.That(predicate(MockContentWithDate(alias, new DateTime(2023, 06, 01))), Is.False);
        });
    }

    private static IPublishedContent MockContent(string name = "", int id = 0)
    {
        var content = new Mock<IPublishedContent>();
        content.SetupGet(x => x.Name).Returns(name);
        content.SetupGet(x => x.Id).Returns(id);
        return content.Object;
    }

    private static IPublishedContent MockContentWithDate(string alias, DateTime date)
    {
        var content = new Mock<IPublishedContent>();
        switch (alias)
        {
            case "CreateDate":
                content.SetupGet(x => x.CreateDate).Returns(date);
                break;
            case "UpdateDate":
                content.SetupGet(x => x.UpdateDate).Returns(date);
                break;
        }

        return content.Object;
    }
}
