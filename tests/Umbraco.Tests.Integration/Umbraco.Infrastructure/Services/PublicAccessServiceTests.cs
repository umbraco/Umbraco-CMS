// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class PublicAccessServiceTests : UmbracoIntegrationTest
{
    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();
    [SetUp]
    public async Task CreateTestData()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey); // else, FK violation on contentType!

        var ct = ContentTypeBuilder.CreateSimpleContentType("blah", "Blah", defaultTemplateId: template.Id);
        await ContentTypeService.CreateAsync(ct, Constants.Security.SuperUserKey);

        _content = ContentBuilder.CreateSimpleContent(ct, "Test");
        ContentService.Save(_content);
    }

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IPublicAccessService PublicAccessService => GetRequiredService<IPublicAccessService>();

    private Content _content;

    [Test]
    public void Can_Add_New_Entry()
    {
        // Arrange
        PublicAccessRule[] rules = { new PublicAccessRule { RuleType = "TestType", RuleValue = "TestVal" } };
        var entry = new PublicAccessEntry(_content, _content, _content, rules);

        // Act
        var result = PublicAccessService.Save(entry);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result.Result, Is.EqualTo(OperationResultType.Success));
        Assert.That(entry.HasIdentity, Is.True);
        Assert.That(Guid.Empty, Is.Not.EqualTo(entry.Key));
        Assert.That(entry.LoginNodeId, Is.EqualTo(_content.Id));
        Assert.That(entry.NoAccessNodeId, Is.EqualTo(_content.Id));
        Assert.That(entry.ProtectedNodeId, Is.EqualTo(_content.Id));
    }

    [Test]
    public void Can_Add_Rule()
    {
        // Arrange
        PublicAccessRule[] rules = { new PublicAccessRule { RuleType = "TestType", RuleValue = "TestVal" } };
        var entry = new PublicAccessEntry(_content, _content, _content, rules);
        PublicAccessService.Save(entry);

        // Act
        var updated = PublicAccessService.AddRule(_content, "TestType2", "AnotherVal");

        // re-get
        entry = PublicAccessService.GetEntryForContent(_content);

        // Assert
        Assert.That(updated.Success, Is.True);
        Assert.That(updated.Result.Result, Is.EqualTo(OperationResultType.Success));
        Assert.That(entry.Rules.Count(), Is.EqualTo(2));
    }

    [Test]
    public void Can_Add_Multiple_Value_For_Same_Rule_Type()
    {
        // Arrange
        PublicAccessRule[] rules = { new PublicAccessRule { RuleType = "TestType", RuleValue = "TestVal" } };
        var entry = new PublicAccessEntry(_content, _content, _content, rules);
        PublicAccessService.Save(entry);

        // Act
        var updated1 = PublicAccessService.AddRule(_content, "TestType", "AnotherVal1");
        var updated2 = PublicAccessService.AddRule(_content, "TestType", "AnotherVal2");

        // re-get
        entry = PublicAccessService.GetEntryForContent(_content);

        // Assert
        Assert.That(updated1.Success, Is.True);
        Assert.That(updated2.Success, Is.True);
        Assert.That(updated1.Result.Result, Is.EqualTo(OperationResultType.Success));
        Assert.That(updated2.Result.Result, Is.EqualTo(OperationResultType.Success));
        Assert.That(entry.Rules.Count(), Is.EqualTo(3));
    }

    [Test]
    public void Can_Remove_Rule()
    {
        // Arrange
        PublicAccessRule[] rules =
        {
            new PublicAccessRule {RuleType = "TestType", RuleValue = "TestValue1"},
            new PublicAccessRule {RuleType = "TestType", RuleValue = "TestValue2"}
        };
        var entry = new PublicAccessEntry(_content, _content, _content, rules);
        PublicAccessService.Save(entry);

        // Act
        var removed = PublicAccessService.RemoveRule(_content, "TestType", "TestValue1");

        // re-get
        entry = PublicAccessService.GetEntryForContent(_content);

        // Assert
        Assert.That(removed.Success, Is.True);
        Assert.That(removed.Result.Result, Is.EqualTo(OperationResultType.Success));
        Assert.That(entry.Rules.Count(), Is.EqualTo(1));
        Assert.That(entry.Rules.ElementAt(0).RuleValue, Is.EqualTo("TestValue2"));
    }
}
