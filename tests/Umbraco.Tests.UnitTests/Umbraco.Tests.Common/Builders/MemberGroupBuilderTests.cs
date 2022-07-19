// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Tests.Common.Builders;

[TestFixture]
public class MemberGroupBuilderTests
{
    [Test]
    public void Is_Built_Correctly()
    {
        // Arrange
        const int testId = 6;
        const string testName = "Test Group";
        const int testCreatorId = 4;
        var testKey = Guid.NewGuid();
        var testCreateDate = DateTime.Now.AddHours(-1);
        var testUpdateDate = DateTime.Now;
        var testAdditionalData1 = new KeyValuePair<string, object>("test1", 123);
        var testAdditionalData2 = new KeyValuePair<string, object>("test2", "hello");

        var builder = new MemberGroupBuilder();

        // Act
        var group = builder
            .WithId(testId)
            .WithKey(testKey)
            .WithName(testName)
            .WithCreatorId(testCreatorId)
            .WithCreateDate(testCreateDate)
            .WithUpdateDate(testUpdateDate)
            .AddAdditionalData()
            .WithKeyValue(testAdditionalData1.Key, testAdditionalData1.Value)
            .WithKeyValue(testAdditionalData2.Key, testAdditionalData2.Value)
            .Done()
            .Build();

        // Assert
        Assert.AreEqual(testId, group.Id);
        Assert.AreEqual(testKey, group.Key);
        Assert.AreEqual(testName, group.Name);
        Assert.AreEqual(testCreateDate, group.CreateDate);
        Assert.AreEqual(testUpdateDate, group.UpdateDate);
        Assert.AreEqual(testCreatorId, group.CreatorId);

        // previousName is added as part of the MemberGroup construction, plus the 2 we've added.
        Assert.AreEqual(3, group.AdditionalData.Count);
        Assert.AreEqual(testAdditionalData1.Value, group.AdditionalData[testAdditionalData1.Key]);
        Assert.AreEqual(testAdditionalData2.Value, group.AdditionalData[testAdditionalData2.Key]);
    }
}
