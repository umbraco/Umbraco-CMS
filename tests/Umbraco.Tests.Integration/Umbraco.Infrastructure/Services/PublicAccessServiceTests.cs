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
        ContentTypeService.Save(ct);

        _content = ContentBuilder.CreateSimpleContent(ct, "Test");
        ContentService.Save(_content);
    }

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IFileService FileService => GetRequiredService<IFileService>();

    private IPublicAccessService PublicAccessService => GetRequiredService<IPublicAccessService>();

    private Content _content;

    [Test]
    public async Task Can_Add_New_Entry()
    {
        // Arrange
        PublicAccessRule[] rules = { new PublicAccessRule { RuleType = "TestType", RuleValue = "TestVal" } };
        var entry = new PublicAccessEntry(_content, _content, _content, rules);

        // Act
        var result = await PublicAccessService.SaveAsync(entry);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(OperationResultType.Success, result.Result.Result);
        Assert.IsTrue(entry.HasIdentity);
        Assert.AreNotEqual(entry.Key, Guid.Empty);
        Assert.AreEqual(_content.Id, entry.LoginNodeId);
        Assert.AreEqual(_content.Id, entry.NoAccessNodeId);
        Assert.AreEqual(_content.Id, entry.ProtectedNodeId);
    }

    [Test]
    public async Task Can_Add_Rule()
    {
        // Arrange
        PublicAccessRule[] rules = { new PublicAccessRule { RuleType = "TestType", RuleValue = "TestVal" } };
        var entry = new PublicAccessEntry(_content, _content, _content, rules);
        await PublicAccessService.SaveAsync(entry);

        // Act
        var updated = await PublicAccessService.AddRuleAsync(_content, "TestType2", "AnotherVal");

        // re-get
        entry = await PublicAccessService.GetEntryForContentAsync(_content);

        // Assert
        Assert.IsTrue(updated.Success);
        Assert.AreEqual(OperationResultType.Success, updated.Result.Result);
        Assert.AreEqual(2, entry.Rules.Count());
    }

    [Test]
    public async Task Can_Add_Multiple_Value_For_Same_Rule_Type()
    {
        // Arrange
        PublicAccessRule[] rules = { new PublicAccessRule { RuleType = "TestType", RuleValue = "TestVal" } };
        var entry = new PublicAccessEntry(_content, _content, _content, rules);
        await PublicAccessService.SaveAsync(entry);

        // Act
        var updated1 = await PublicAccessService.AddRuleAsync(_content, "TestType", "AnotherVal1");
        var updated2 = await PublicAccessService.AddRuleAsync(_content, "TestType", "AnotherVal2");

        // re-get
        entry = await PublicAccessService.GetEntryForContentAsync(_content);

        // Assert
        Assert.IsTrue(updated1.Success);
        Assert.IsTrue(updated2.Success);
        Assert.AreEqual(OperationResultType.Success, updated1.Result.Result);
        Assert.AreEqual(OperationResultType.Success, updated2.Result.Result);
        Assert.AreEqual(3, entry.Rules.Count());
    }

    [Test]
    public async Task Can_Remove_Rule()
    {
        // Arrange
        PublicAccessRule[] rules =
        {
            new PublicAccessRule {RuleType = "TestType", RuleValue = "TestValue1"},
            new PublicAccessRule {RuleType = "TestType", RuleValue = "TestValue2"}
        };
        var entry = new PublicAccessEntry(_content, _content, _content, rules);
        await PublicAccessService.SaveAsync(entry);

        // Act
        var removed = await PublicAccessService.RemoveRuleAsync(_content, "TestType", "TestValue1");

        // re-get
        entry = await PublicAccessService.GetEntryForContentAsync(_content);

        // Assert
        Assert.IsTrue(removed.Success);
        Assert.AreEqual(OperationResultType.Success, removed.Result.Result);
        Assert.AreEqual(1, entry.Rules.Count());
        Assert.AreEqual("TestValue2", entry.Rules.ElementAt(0).RuleValue);
    }
}
