// Copyright (c) Umbraco.
// See LICENSE for more details.

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
        var testDataTypeKey = Guid.NewGuid();
        var testCreateDate = DateTime.UtcNow.AddHours(-1);
        var testUpdateDate = DateTime.UtcNow;
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
            .WithDataTypeKey(testDataTypeKey)
            .WithCreateDate(testCreateDate)
            .WithUpdateDate(testUpdateDate)
            .WithDescription(testDescription)
            .WithKey(testKey)
            .WithPropertyGroupId(testPropertyGroupId)
            .WithMandatory(testMandatory, testMandatoryMessage)
            .WithValidationRegExp(testValidationRegExp, testValidationRegExpMessage)
            .Build();

        // Assert
        Assert.That(propertyType.Id, Is.EqualTo(testId));
        Assert.That(propertyType.PropertyEditorAlias, Is.EqualTo(testPropertyEditorAlias));
        Assert.That(propertyType.ValueStorageType, Is.EqualTo(testValueStorageType));
        Assert.That(propertyType.Alias, Is.EqualTo(testAlias));
        Assert.That(propertyType.Name, Is.EqualTo(testName));
        Assert.That(propertyType.SortOrder, Is.EqualTo(testSortOrder));
        Assert.That(propertyType.DataTypeId, Is.EqualTo(testDataTypeId));
        Assert.That(propertyType.DataTypeKey, Is.EqualTo(testDataTypeKey));
        Assert.That(propertyType.CreateDate, Is.EqualTo(testCreateDate));
        Assert.That(propertyType.UpdateDate, Is.EqualTo(testUpdateDate));
        Assert.That(propertyType.Description, Is.EqualTo(testDescription));
        Assert.That(propertyType.Key, Is.EqualTo(testKey));
        Assert.That(propertyType.PropertyGroupId.Value, Is.EqualTo(testPropertyGroupId));
        Assert.That(propertyType.Mandatory, Is.EqualTo(testMandatory));
        Assert.That(propertyType.MandatoryMessage, Is.EqualTo(testMandatoryMessage));
        Assert.That(propertyType.ValidationRegExp, Is.EqualTo(testValidationRegExp));
        Assert.That(propertyType.ValidationRegExpMessage, Is.EqualTo(testValidationRegExpMessage));
    }
}
