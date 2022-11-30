// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
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
        var testCreateDate = DateTime.Now.AddHours(-1);
        var testUpdateDate = DateTime.Now;
        const string testPath = "-1,3";
        const string testContent = "blah";
        const string testMasterTemplateAlias = "master";
        const int testMasterTemplateId = 88;

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
            .AsMasterTemplate(testMasterTemplateAlias, testMasterTemplateId)
            .Build();

        // Assert
        Assert.AreEqual(testId, template.Id);
        Assert.AreEqual(testAlias, template.Alias);
        Assert.AreEqual(testName, template.Name);
        Assert.AreEqual(testCreateDate, template.CreateDate);
        Assert.AreEqual(testUpdateDate, template.UpdateDate);
        Assert.AreEqual(testKey, template.Key);
        Assert.AreEqual(testPath, template.Path);
        Assert.AreEqual(testContent, template.Content);
        Assert.IsTrue(template.IsMasterTemplate);
        Assert.AreEqual(testMasterTemplateAlias, template.MasterTemplateAlias);
        Assert.AreEqual(testMasterTemplateId, ((Template)template).MasterTemplateId.Value);
    }
}
