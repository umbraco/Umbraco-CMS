// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Tests.Common.Builders;

[TestFixture]
public class PropertyTypeBuilderTests
{
    [Test]
    public void Is_Built_Correctly()
    {
        // Arrange
        const int testId = 3;
        var testKey = Guid.NewGuid();
        const string testPropertyEditorAlias = "TestPropertyEditor";
        const ValueStorageType testValueStorageType = ValueStorageType.Nvarchar;
        const string testAlias = "test";
        const string testName = "Test";
        const int testSortOrder = 9;
        const int testDataTypeId = 5;
        var testCreateDate = DateTime.Now.AddHours(-1);
        var testUpdateDate = DateTime.Now;
        const string testDescription = "testing";
        const int testPropertyGroupId = 11;
        const bool testMandatory = true;
        const string testMandatoryMessage = "Field is required";
        const string testValidationRegExp = "xxxx";
        const string testValidationRegExpMessage = "Field must match pattern";

        var builder = new PropertyTypeBuilder();

        // Act
        var propertyType = builder
            .WithId(testId)
            .WithPropertyEditorAlias(testPropertyEditorAlias)
            .WithValueStorageType(testValueStorageType)
            .WithAlias(testAlias)
            .WithName(testName)
            .WithSortOrder(testSortOrder)
            .WithDataTypeId(testDataTypeId)
            .WithCreateDate(testCreateDate)
            .WithUpdateDate(testUpdateDate)
            .WithDescription(testDescription)
            .WithKey(testKey)
            .WithPropertyGroupId(testPropertyGroupId)
            .WithMandatory(testMandatory, testMandatoryMessage)
            .WithValidationRegExp(testValidationRegExp, testValidationRegExpMessage)
            .Build();

        // Assert
        Assert.AreEqual(testId, propertyType.Id);
        Assert.AreEqual(testPropertyEditorAlias, propertyType.PropertyEditorAlias);
        Assert.AreEqual(testValueStorageType, propertyType.ValueStorageType);
        Assert.AreEqual(testAlias, propertyType.Alias);
        Assert.AreEqual(testName, propertyType.Name);
        Assert.AreEqual(testSortOrder, propertyType.SortOrder);
        Assert.AreEqual(testDataTypeId, propertyType.DataTypeId);
        Assert.AreEqual(testCreateDate, propertyType.CreateDate);
        Assert.AreEqual(testUpdateDate, propertyType.UpdateDate);
        Assert.AreEqual(testDescription, propertyType.Description);
        Assert.AreEqual(testKey, propertyType.Key);
        Assert.AreEqual(testPropertyGroupId, propertyType.PropertyGroupId.Value);
        Assert.AreEqual(testMandatory, propertyType.Mandatory);
        Assert.AreEqual(testMandatoryMessage, propertyType.MandatoryMessage);
        Assert.AreEqual(testValidationRegExp, propertyType.ValidationRegExp);
        Assert.AreEqual(testValidationRegExpMessage, propertyType.ValidationRegExpMessage);
    }
}
