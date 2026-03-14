using System.ComponentModel.DataAnnotations;
using System.Data;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

/// <summary>
/// Unit tests for validating the MultiNodeTreePicker property editor.
/// </summary>
[TestFixture]
public class MultiNodeTreePickerValidationTests
{
    // Remember 0 = no limit
    [TestCase(0, true, "[{\"type\":\"document\",\"unique\":\"86eb02a7-793f-4406-9152-9736b6b64bee\"}]")]
    [TestCase(1, true, "[{\"type\":\"document\",\"unique\":\"86eb02a7-793f-4406-9152-9736b6b64bee\"}]")]
    [TestCase(2, true, "[{\"type\":\"document\",\"unique\":\"86eb02a7-793f-4406-9152-9736b6b64bee\"},{\"type\":\"document\",\"unique\":\"25ef6fd2-db48-450a-8c48-df3ad75adf4b\"}]")]
    [TestCase(3, false, "[{\"type\":\"document\",\"unique\":\"86eb02a7-793f-4406-9152-9736b6b64bee\"},{\"type\":\"document\",\"unique\":\"86eb02a7-793f-4406-9152-9736b6b64bee\"}]")]
    [TestCase(2, false, "[{\"type\":\"document\",\"unique\":\"86eb02a7-793f-4406-9152-9736b6b64bee\"}]")]
    [TestCase(1, false, null)]
    [TestCase(0, true, null)]
    public void Validates_Minimum_Entries(int min, bool shouldSucceed, string? value)
    {
        var (valueEditor, _, _, _, _) = CreateValueEditor();
        valueEditor.ConfigurationObject = new MultiNodePickerConfiguration { MinNumber = min};

        var result = valueEditor.Validate(value, false, null, PropertyValidationContext.Empty());

        TestShouldSucceed(shouldSucceed, result);
    }

    private static void TestShouldSucceed(bool shouldSucceed, IEnumerable<ValidationResult> result)
    {
        if (shouldSucceed)
        {
            Assert.IsEmpty(result);
        }
        else
        {
            Assert.IsNotEmpty(result);
        }
    }

    /// <summary>
    /// Tests that the MultiNodePicker value editor correctly validates the maximum number of entries allowed.
    /// </summary>
    /// <param name="max">The maximum number of entries allowed.</param>
    /// <param name="shouldSucceed">Indicates whether the validation is expected to succeed.</param>
    /// <param name="value">The JSON string representing the selected nodes.</param>
    [TestCase(0, true, "[]")]
    [TestCase(1, true, "[{\"type\":\"document\",\"unique\":\"86eb02a7-793f-4406-9152-9736b6b64bee\"}]")]
    [TestCase(0, true, "[{\"type\":\"document\",\"unique\":\"86eb02a7-793f-4406-9152-9736b6b64bee\"}]")]
    [TestCase(1, false, "[{\"type\":\"document\",\"unique\":\"86eb02a7-793f-4406-9152-9736b6b64bee\"},{\"type\":\"document\",\"unique\":\"25ef6fd2-db48-450a-8c48-df3ad75adf4b\"}]")]
    [TestCase(3, true, "[{\"type\":\"document\",\"unique\":\"86eb02a7-793f-4406-9152-9736b6b64bee\"},{\"type\":\"document\",\"unique\":\"86eb02a7-793f-4406-9152-9736b6b64bee\"}]")]
    [TestCase(2, true, "[{\"type\":\"document\",\"unique\":\"86eb02a7-793f-4406-9152-9736b6b64bee\"}]")]
    public void Validates_Maximum_Entries(int max, bool shouldSucceed, string value)
    {
        var (valueEditor, _, _, _, _) = CreateValueEditor();
        valueEditor.ConfigurationObject = new MultiNodePickerConfiguration { MaxNumber = max };

        var result = valueEditor.Validate(value, false, null, PropertyValidationContext.Empty());

        TestShouldSucceed(shouldSucceed, result);
    }

    private readonly Dictionary<Guid, Guid> _entityTypeMap = new()
    {
        { Constants.ObjectTypes.Document, Guid.Parse("08035A7E-AE9C-4D36-BA2E-63F639005758") },
        { Constants.ObjectTypes.Media, Guid.Parse("AAF97C7D-A586-45CC-AC7F-CE0A80BCFEE3") },
        { Constants.ObjectTypes.Member, Guid.Parse("E477804E-C903-470B-B7EC-67DCAF71E37C") },
    };

    private class ObjectTypeTestSetup
    {
    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectTypeTestSetup"/> class.
    /// </summary>
    /// <param name="expectedObjectType">The expected object type.</param>
    /// <param name="shouldSucceed">Indicates whether the test should succeed.</param>
    /// <param name="value">The value to test.</param>
        public ObjectTypeTestSetup(string expectedObjectType, bool shouldSucceed, string value)
        {
            ExpectedObjectType = expectedObjectType;
            ShouldSucceed = shouldSucceed;
            Value = value;
        }

    /// <summary>
    /// Gets the expected object type for the test setup.
    /// </summary>
        public string ExpectedObjectType { get; }

    /// <summary>
    /// Gets a value indicating whether the test setup is expected to succeed.
    /// </summary>
        public bool ShouldSucceed { get; }

        public string Value { get; }
    }

    private void SetupEntityServiceForObjectTypeTest(Mock<IEntityService> entityServiceMock)
    {
        foreach (var objectTypeEntity in _entityTypeMap)
        {
            var entity = new Mock<IEntitySlim>();
            entity.Setup(x => x.NodeObjectType).Returns(objectTypeEntity.Key);
            entityServiceMock.Setup(x => x.Get(objectTypeEntity.Value)).Returns(entity.Object);
        }
    }

    private IEnumerable<ObjectTypeTestSetup> GetObjectTypeTestSetup() =>
    [
        new(MultiNodeTreePickerPropertyEditor.MultiNodeTreePickerPropertyValueEditor.DocumentObjectType, true, "[]"),
        new(MultiNodeTreePickerPropertyEditor.MultiNodeTreePickerPropertyValueEditor.DocumentObjectType, true, $"[{{\"type\":\"document\",\"unique\":\"{_entityTypeMap[Constants.ObjectTypes.Document]}\"}}]"),
        new(MultiNodeTreePickerPropertyEditor.MultiNodeTreePickerPropertyValueEditor.DocumentObjectType, false, $"[{{\"type\":\"document\",\"unique\":\"{_entityTypeMap[Constants.ObjectTypes.Media]}\"}}]"),
        new(MultiNodeTreePickerPropertyEditor.MultiNodeTreePickerPropertyValueEditor.DocumentObjectType, false, $"[{{\"type\":\"document\",\"unique\":\"{_entityTypeMap[Constants.ObjectTypes.Member]}\"}}]"),
        new(MultiNodeTreePickerPropertyEditor.MultiNodeTreePickerPropertyValueEditor.MediaObjectType, false, $"[{{\"type\":\"document\",\"unique\":\"{_entityTypeMap[Constants.ObjectTypes.Document]}\"}}]"),
        new(MultiNodeTreePickerPropertyEditor.MultiNodeTreePickerPropertyValueEditor.MemberObjectType, false, $"[{{\"type\":\"document\",\"unique\":\"{_entityTypeMap[Constants.ObjectTypes.Document]}\"}}]"),
        new(MultiNodeTreePickerPropertyEditor.MultiNodeTreePickerPropertyValueEditor.DocumentObjectType, false, $"[{{\"type\":\"document\",\"unique\":\"{_entityTypeMap[Constants.ObjectTypes.Document]}\"}}, {{\"type\":\"document\",\"unique\":\"{_entityTypeMap[Constants.ObjectTypes.Media]}\"}}]"),
        new(MultiNodeTreePickerPropertyEditor.MultiNodeTreePickerPropertyValueEditor.MediaObjectType, false, $"[{{\"type\":\"document\",\"unique\":\"{_entityTypeMap[Constants.ObjectTypes.Document]}\"}}]"),
        new(MultiNodeTreePickerPropertyEditor.MultiNodeTreePickerPropertyValueEditor.MediaObjectType, false, $"[{{\"type\":\"document\",\"unique\":\"{_entityTypeMap[Constants.ObjectTypes.Member]}\"}}]"),
        new(MultiNodeTreePickerPropertyEditor.MultiNodeTreePickerPropertyValueEditor.MemberObjectType, false, $"[{{\"type\":\"document\",\"unique\":\"{_entityTypeMap[Constants.ObjectTypes.Document]}\"}}]"),
        new(MultiNodeTreePickerPropertyEditor.MultiNodeTreePickerPropertyValueEditor.MemberObjectType, false, $"[{{\"type\":\"document\",\"unique\":\"{_entityTypeMap[Constants.ObjectTypes.Media]}\"}}]"),
    ];

    /// <summary>
    /// Tests that the MultiNodePicker value editor correctly validates values against the expected object type.
    /// </summary>
    [Test]
    public void Validates_Object_Type()
    {
        var setups = GetObjectTypeTestSetup();

        foreach (var setup in setups)
        {
            var (valueEditor, entityServiceMock, _, _, _) = CreateValueEditor();
            SetupEntityServiceForObjectTypeTest(entityServiceMock);
            valueEditor.ConfigurationObject = new MultiNodePickerConfiguration { TreeSource = new MultiNodePickerConfigurationTreeSource() { ObjectType = setup.ExpectedObjectType } };
            var result = valueEditor.Validate(setup.Value, false, null, PropertyValidationContext.Empty());

            TestShouldSucceed(setup.ShouldSucceed, result);
        }
    }

    /// <summary>
    /// Verifies that the MultiNodeTreePicker correctly validates whether a selected entity is of an allowed type.
    /// </summary>
    /// <param name="shouldSucceed">True if the validation is expected to pass; otherwise, false.</param>
    /// <param name="hasAllowedType">True if the mocked content entity has the allowed type key; otherwise, false.</param>
    /// <param name="findsContent">True if the content entity is found by the service; otherwise, false.</param>
    /// <param name="objectType">The object type being validated (e.g., document, media, member), or null to default to document.</param>
    [TestCase(true, true, true, MultiNodeTreePickerPropertyEditor.MultiNodeTreePickerPropertyValueEditor.DocumentObjectType)]
    [TestCase(true, true, true, MultiNodeTreePickerPropertyEditor.MultiNodeTreePickerPropertyValueEditor.MediaObjectType)]
    [TestCase(true, true, true, MultiNodeTreePickerPropertyEditor.MultiNodeTreePickerPropertyValueEditor.MemberObjectType)]
    [TestCase(false, false, true, MultiNodeTreePickerPropertyEditor.MultiNodeTreePickerPropertyValueEditor.DocumentObjectType)]
    [TestCase(false, false, true, MultiNodeTreePickerPropertyEditor.MultiNodeTreePickerPropertyValueEditor.MediaObjectType)]
    [TestCase(false, false, true, MultiNodeTreePickerPropertyEditor.MultiNodeTreePickerPropertyValueEditor.MemberObjectType)]
    [TestCase(false, true, false, null)] // Null object type should be treated as being for document.
    [TestCase(false, true, false, MultiNodeTreePickerPropertyEditor.MultiNodeTreePickerPropertyValueEditor.DocumentObjectType)]
    [TestCase(false, true, false, MultiNodeTreePickerPropertyEditor.MultiNodeTreePickerPropertyValueEditor.MediaObjectType)]
    [TestCase(false, true, false, MultiNodeTreePickerPropertyEditor.MultiNodeTreePickerPropertyValueEditor.MemberObjectType)]
    public void Validates_Allowed_Type(bool shouldSucceed, bool hasAllowedType, bool findsContent, string? objectType)
    {
        var (valueEditor, _, contentService, mediaService, memberService) = CreateValueEditor();

        var expectedEntityKey = Guid.NewGuid();
        var allowedTypeKey = Guid.NewGuid();
        valueEditor.ConfigurationObject = new MultiNodePickerConfiguration()
        {
            Filter = $"{allowedTypeKey}",
            TreeSource = new MultiNodePickerConfigurationTreeSource { ObjectType = objectType },
        };

        var contentTypeMock = new Mock<ISimpleContentType>();
        contentTypeMock.Setup(x => x.Key).Returns(() => hasAllowedType ? allowedTypeKey : Guid.NewGuid());

        var contentMock = new Mock<IContent>();
        contentMock.Setup(x => x.ContentType).Returns(contentTypeMock.Object);
        contentService.Setup(x => x.GetById(expectedEntityKey)).Returns(contentMock.Object);

        var mediaMock = new Mock<IMedia>();
        mediaMock.Setup(x => x.ContentType).Returns(contentTypeMock.Object);
        mediaService.Setup(x => x.GetById(expectedEntityKey)).Returns(mediaMock.Object);

        var memberMock = new Mock<IMember>();
        memberMock.Setup(x => x.ContentType).Returns(contentTypeMock.Object);
        memberService.Setup(x => x.GetById(expectedEntityKey)).Returns(memberMock.Object);

        var actualkey = findsContent ? expectedEntityKey : Guid.NewGuid();
        var value = $"[{{\"type\":\"document\",\"unique\":\"{actualkey}\"}}]";

        var result = valueEditor.Validate(value, false, null, PropertyValidationContext.Empty());
        TestShouldSucceed(shouldSucceed, result);
    }

    private static (MultiNodeTreePickerPropertyEditor.MultiNodeTreePickerPropertyValueEditor ValueEditor,
        Mock<IEntityService> EntityService,
        Mock<IContentService> ContentService,
        Mock<IMediaService> MediaService,
        Mock<IMemberService> MemberService) CreateValueEditor()
    {
        var entityServiceMock = new Mock<IEntityService>();
        var contentServiceMock = new Mock<IContentService>();
        var mediaServiceMock = new Mock<IMediaService>();
        var memberServiceMock = new Mock<IMemberService>();

        var mockScope = new Mock<ICoreScope>();
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
            new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()),
            Mock.Of<IIOHelper>(),
            new DataEditorAttribute("alias"),
            Mock.Of<ILocalizedTextService>(),
            entityServiceMock.Object,
            mockScopeProvider.Object,
            contentServiceMock.Object,
            mediaServiceMock.Object,
            memberServiceMock.Object)
        {
            ConfigurationObject = new MultiNodePickerConfiguration(),
        };

        return (valueEditor, entityServiceMock, contentServiceMock, mediaServiceMock, memberServiceMock);
    }
}
