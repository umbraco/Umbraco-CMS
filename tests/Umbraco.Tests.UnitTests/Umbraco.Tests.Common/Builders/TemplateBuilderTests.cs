// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Tests.Common.Builders;

[TestFixture]
public class TemplateBuilderTests
{
    [Test]
    public void Is_Built_Correctly()
    {
        // Arrange
        const int testId = 3;
        const string testAlias = "test";
        const string testName = "Test";
        var testKey = Guid.NewGuid();
        var testCreateDate = DateTime.UtcNow.AddHours(-1);
        var testUpdateDate = DateTime.UtcNow;
        const string testPath = "-1,3";
        const string testContent = "blah";
        const string testLayoutTemplateAlias = "master";
        const int testLayoutTemplateId = 88;

        var builder = new TemplateBuilder();

        // Act
        var template = builder
            .WithId(3)
            .WithAlias(testAlias)
            .WithName(testName)
            .WithCreateDate(testCreateDate)
            .WithUpdateDate(testUpdateDate)
            .WithKey(testKey)
            .WithPath(testPath)
            .WithContent(testContent)
            .AsLayoutTemplate(testLayoutTemplateAlias, testLayoutTemplateId)
            .Build();

        // Assert
        Assert.That(template.Id, Is.EqualTo(testId));
        Assert.That(template.Alias, Is.EqualTo(testAlias));
        Assert.That(template.Name, Is.EqualTo(testName));
        Assert.That(template.CreateDate, Is.EqualTo(testCreateDate));
        Assert.That(template.UpdateDate, Is.EqualTo(testUpdateDate));
        Assert.That(template.Key, Is.EqualTo(testKey));
        Assert.That(template.Path, Is.EqualTo(testPath));
        Assert.That(template.Content, Is.EqualTo(testContent));
        Assert.That(template.IsLayoutTemplate, Is.True);
        Assert.That(template.LayoutTemplateAlias, Is.EqualTo(testLayoutTemplateAlias));
        Assert.That(((Template)template).LayoutTemplateId.Value, Is.EqualTo(testLayoutTemplateId));
    }
}
