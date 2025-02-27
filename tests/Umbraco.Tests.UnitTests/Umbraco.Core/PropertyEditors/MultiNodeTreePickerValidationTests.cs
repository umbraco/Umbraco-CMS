using System.Data;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class MultiNodeTreePickerValidationTests
{
    // Remember 0 = no limit
    [TestCase(0, true, "[{\"type\":\"document\",\"unique\":\"86eb02a7-793f-4406-9152-9736b6b64bee\"}]")]
    [TestCase(1, true, "[{\"type\":\"document\",\"unique\":\"86eb02a7-793f-4406-9152-9736b6b64bee\"}]")]
    [TestCase(2, true, "[{\"type\":\"document\",\"unique\":\"86eb02a7-793f-4406-9152-9736b6b64bee\"},{\"type\":\"document\",\"unique\":\"25ef6fd2-db48-450a-8c48-df3ad75adf4b\"}]")]
    [TestCase(3, false, "[{\"type\":\"document\",\"unique\":\"86eb02a7-793f-4406-9152-9736b6b64bee\"},{\"type\":\"document\",\"unique\":\"86eb02a7-793f-4406-9152-9736b6b64bee\"}]")]
    [TestCase(2, false, "[{\"type\":\"document\",\"unique\":\"86eb02a7-793f-4406-9152-9736b6b64bee\"}]")]
    public void Validates_Minimum_Entries(int min, bool shouldSucceed, string value)
    {
        var (valueEditor, _) = CreateValueEditor();
        valueEditor.ConfigurationObject = new MultiNodePickerConfiguration { MinNumber = min};

        var result = valueEditor.Validate(value, false, null, PropertyValidationContext.Empty());

        if (shouldSucceed)
        {
            Assert.IsEmpty(result);
        }
        else
        {
            Assert.IsNotEmpty(result);
        }
    }

    [TestCase(0, true, "[]")]
    [TestCase(1, true, "[{\"type\":\"document\",\"unique\":\"86eb02a7-793f-4406-9152-9736b6b64bee\"}]")]
    [TestCase(0, true, "[{\"type\":\"document\",\"unique\":\"86eb02a7-793f-4406-9152-9736b6b64bee\"}]")]
    [TestCase(1, false, "[{\"type\":\"document\",\"unique\":\"86eb02a7-793f-4406-9152-9736b6b64bee\"},{\"type\":\"document\",\"unique\":\"25ef6fd2-db48-450a-8c48-df3ad75adf4b\"}]")]
    [TestCase(3, true, "[{\"type\":\"document\",\"unique\":\"86eb02a7-793f-4406-9152-9736b6b64bee\"},{\"type\":\"document\",\"unique\":\"86eb02a7-793f-4406-9152-9736b6b64bee\"}]")]
    [TestCase(2, true, "[{\"type\":\"document\",\"unique\":\"86eb02a7-793f-4406-9152-9736b6b64bee\"}]")]
    public void Validates_Maximum_Entries(int max, bool shouldSucceed, string value)
    {
        var (valueEditor, _) = CreateValueEditor();
        valueEditor.ConfigurationObject = new MultiNodePickerConfiguration { MaxNumber = max };

        var result = valueEditor.Validate(value, false, null, PropertyValidationContext.Empty());

        if (shouldSucceed)
        {
            Assert.IsEmpty(result);
        }
        else
        {
            Assert.IsNotEmpty(result);
        }
    }

    private Dictionary<Guid, Guid> EntityTypeMap = new()
    {
        { Constants.ObjectTypes.Document, Guid.Parse("08035A7E-AE9C-4D36-BA2E-63F639005758") },
        { Constants.ObjectTypes.Media, Guid.Parse("AAF97C7D-A586-45CC-AC7F-CE0A80BCFEE3") },
        { Constants.ObjectTypes.Member, Guid.Parse("E477804E-C903-470B-B7EC-67DCAF71E37C") },
    };

    private class ObjectTypeTestSetup
    {
        public ObjectTypeTestSetup(string expectedObjectType, bool shouldSucceed, string value)
        {
            ExpectedObjectType = expectedObjectType;
            ShouldSucceed = shouldSucceed;
            Value = value;
        }

        public string ExpectedObjectType;
        public bool ShouldSucceed;
        public string Value;
    }

    private void SetupEntityServiceForObjectTypeTest(Mock<IEntityService> entityServiceMock)
    {
        foreach (var objectTypeEntity in EntityTypeMap)
        {
            var entity = new Mock<IEntitySlim>();
            entity.Setup(x => x.NodeObjectType).Returns(objectTypeEntity.Key);
            entityServiceMock.Setup(x => x.Get(objectTypeEntity.Value)).Returns(entity.Object);
        }
    }

    private const string DocumentObjectType = "content";
    private const string MediaObjectType = "media";
    private const string MemberObjectType = "member";

    private IEnumerable<ObjectTypeTestSetup> GetObjectTypeTestSetup() =>
    [
        new(DocumentObjectType, true, "[]"),
        new(DocumentObjectType, true, $"[{{\"type\":\"document\",\"unique\":\"{EntityTypeMap[Constants.ObjectTypes.Document]}\"}}]"),
        new(DocumentObjectType, false, $"[{{\"type\":\"document\",\"unique\":\"{EntityTypeMap[Constants.ObjectTypes.Media]}\"}}]"),
        new(DocumentObjectType, false, $"[{{\"type\":\"document\",\"unique\":\"{EntityTypeMap[Constants.ObjectTypes.Member]}\"}}]"),
        new(MediaObjectType, false, $"[{{\"type\":\"document\",\"unique\":\"{EntityTypeMap[Constants.ObjectTypes.Document]}\"}}]"),
        new(MemberObjectType, false, $"[{{\"type\":\"document\",\"unique\":\"{EntityTypeMap[Constants.ObjectTypes.Document]}\"}}]"),
        new(DocumentObjectType, false, $"[{{\"type\":\"document\",\"unique\":\"{EntityTypeMap[Constants.ObjectTypes.Document]}\"}}, {{\"type\":\"document\",\"unique\":\"{EntityTypeMap[Constants.ObjectTypes.Media]}\"}}]"),
        new(MediaObjectType, false, $"[{{\"type\":\"document\",\"unique\":\"{EntityTypeMap[Constants.ObjectTypes.Document]}\"}}]"),
        new(MediaObjectType, false, $"[{{\"type\":\"document\",\"unique\":\"{EntityTypeMap[Constants.ObjectTypes.Member]}\"}}]"),
        new(MemberObjectType, false, $"[{{\"type\":\"document\",\"unique\":\"{EntityTypeMap[Constants.ObjectTypes.Document]}\"}}]"),
        new(MemberObjectType, false, $"[{{\"type\":\"document\",\"unique\":\"{EntityTypeMap[Constants.ObjectTypes.Media]}\"}}]"),
    ];

    [Test]
    public void Validates_Object_Type()
    {
        var setups = GetObjectTypeTestSetup();

        foreach (var setup in setups)
        {
            var (valueEditor, entityServiceMock) = CreateValueEditor();
            SetupEntityServiceForObjectTypeTest(entityServiceMock);
            valueEditor.ConfigurationObject = new MultiNodePickerConfiguration { TreeSource = new MultiNodePickerConfigurationTreeSource() { ObjectType = setup.ExpectedObjectType } };
            var result = valueEditor.Validate(setup.Value, false, null, PropertyValidationContext.Empty());
            if (setup.ShouldSucceed)
            {
                Assert.IsEmpty(result, $"Expected validation to succeed, expected object type: {setup.ExpectedObjectType}, value: {setup.Value}");
            }
            else
            {
                Assert.IsNotEmpty(result, $"Expected validation to fail, expected object type: {setup.ExpectedObjectType}, value: {setup.Value}");
            }
        }
    }

    private static (MultiNodeTreePickerPropertyEditor.MultiNodeTreePickerPropertyValueEditor valueEditor, Mock<IEntityService> EntityService) CreateValueEditor()
    {
        var entityServiceMock = new Mock<IEntityService>();
        var mockScope = new Mock<IScope>();
        var mockScopeProvider = new Mock<ICoreScopeProvider>();
        mockScopeProvider
            .Setup(x => x.CreateCoreScope(
                It.IsAny<IsolationLevel>(),
                It.IsAny<RepositoryCacheMode>(),
                It.IsAny<IEventDispatcher>(),
                It.IsAny<IScopedNotificationPublisher>(),
                It.IsAny<bool?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(mockScope.Object);

        var valueEditor = new MultiNodeTreePickerPropertyEditor.MultiNodeTreePickerPropertyValueEditor(
            Mock.Of<IShortStringHelper>(),
            new SystemTextJsonSerializer(),
            Mock.Of<IIOHelper>(),
            new DataEditorAttribute("alias"),
            Mock.Of<ILocalizedTextService>(),
            entityServiceMock.Object,
            mockScopeProvider.Object)
        {
            ConfigurationObject = new MultiNodePickerConfiguration(),
        };

        return (valueEditor, entityServiceMock);
    }
}
