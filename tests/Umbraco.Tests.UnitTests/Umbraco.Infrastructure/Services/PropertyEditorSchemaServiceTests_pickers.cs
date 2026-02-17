using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Services;

// tests that correct schema's are being returned by validating values against the schema for guids that support guids
public partial class PropertyEditorSchemaServiceTests
{
    #region Simple UUID Pickers (ContentPicker, MemberPicker, UserPicker)

    [TestCase("\"550e8400-e29b-41d4-a716-446655440000\"", true, TestName = "ValidGuidWithDashes_Succeeds")]
    [TestCase("\"550e8400e29b41d4a716446655440000\"", true, TestName = "ValidGuidNoDashes_Succeeds")]
    [TestCase("\"55xyz000-e29b-41d4-a716-446655440000\"", false, TestName = "InvalidGuid_Fails")]
    [TestCase("\"818\"", false, TestName = "NumberAsString_Fails")]
    public async Task ValidateValueAsync_ContentPicker_Validates_Uuid_Pattern(string jsonValue, bool shouldBeValid)
    {
        // Arrange
        var dataTypeKey = Guid.NewGuid();
        var contentPickerEditor = CreateContentPickerPropertyEditor();
        SetupDataEditors(contentPickerEditor);
        SetupDataType(dataTypeKey, Constants.PropertyEditors.Aliases.ContentPicker);

        // Act
        var result = await _sut.ValidateValueAsync(dataTypeKey, jsonValue);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(PropertyEditorSchemaOperationStatus.Success));

        if (shouldBeValid)
        {
            Assert.That(result.Result, Is.Empty, "Expected no validation errors for a valid GUID");
        }
        else
        {
            Assert.That(result.Result, Is.Not.Empty, "Expected validation errors for an invalid GUID");
            Assert.That(result.Result.First().Keyword, Is.EqualTo("pattern"));
        }
    }

    [TestCase("\"550e8400-e29b-41d4-a716-446655440000\"", true, TestName = "ValidGuidWithDashes_Succeeds")]
    [TestCase("\"550e8400e29b41d4a716446655440000\"", true, TestName = "ValidGuidNoDashes_Succeeds")]
    [TestCase("\"55xyz000-e29b-41d4-a716-446655440000\"", false, TestName = "InvalidGuid_Fails")]
    [TestCase("\"818\"", false, TestName = "NumberAsString_Fails")]
    public async Task ValidateValueAsync_MemberPicker_Validates_Uuid_Pattern(string jsonValue, bool shouldBeValid)
    {
        // Arrange
        var dataTypeKey = Guid.NewGuid();
        var memberPickerEditor = CreateMemberPickerPropertyEditor();
        SetupDataEditors(memberPickerEditor);
        SetupDataType(dataTypeKey, Constants.PropertyEditors.Aliases.MemberPicker);

        // Act
        var result = await _sut.ValidateValueAsync(dataTypeKey, jsonValue);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(PropertyEditorSchemaOperationStatus.Success));

        if (shouldBeValid)
        {
            Assert.That(result.Result, Is.Empty, "Expected no validation errors for a valid GUID");
        }
        else
        {
            Assert.That(result.Result, Is.Not.Empty, "Expected validation errors for an invalid GUID");
            Assert.That(result.Result.First().Keyword, Is.EqualTo("pattern"));
        }
    }

    [TestCase("\"550e8400-e29b-41d4-a716-446655440000\"", true, TestName = "ValidGuidWithDashes_Succeeds")]
    [TestCase("\"550e8400e29b41d4a716446655440000\"", true, TestName = "ValidGuidNoDashes_Succeeds")]
    [TestCase("\"55xyz000-e29b-41d4-a716-446655440000\"", false, TestName = "InvalidGuid_Fails")]
    [TestCase("\"818\"", false, TestName = "NumberAsString_Fails")]
    public async Task ValidateValueAsync_UserPicker_Validates_Uuid_Pattern(string jsonValue, bool shouldBeValid)
    {
        // Arrange
        var dataTypeKey = Guid.NewGuid();
        var userPickerEditor = CreateUserPickerPropertyEditor();
        SetupDataEditors(userPickerEditor);
        SetupDataType(dataTypeKey, Constants.PropertyEditors.Aliases.UserPicker);

        // Act
        var result = await _sut.ValidateValueAsync(dataTypeKey, jsonValue);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(PropertyEditorSchemaOperationStatus.Success));

        if (shouldBeValid)
        {
            Assert.That(result.Result, Is.Empty, "Expected no validation errors for a valid GUID");
        }
        else
        {
            Assert.That(result.Result, Is.Not.Empty, "Expected validation errors for an invalid GUID");
            Assert.That(result.Result.First().Keyword, Is.EqualTo("pattern"));
        }
    }

    #endregion

    #region MultiNodeTreePicker

    [TestCase("null", true, TestName = "Null_Succeeds")]
    [TestCase("[]", true, TestName = "EmptyArray_Succeeds")]
    [TestCase("[{\"type\":\"content\",\"unique\":\"550e8400-e29b-41d4-a716-446655440000\"}]", true, TestName = "ValidGuidWithDashes_Succeeds")]
    [TestCase("[{\"type\":\"content\",\"unique\":\"550e8400e29b41d4a716446655440000\"}]", true, TestName = "ValidGuidNoDashes_Succeeds")]
    [TestCase("[{\"type\":\"content\",\"unique\":\"invalid-guid\"}]", false, TestName = "InvalidGuid_Fails")]
    [TestCase("[{\"type\":\"content\",\"unique\":\"818\"}]", false, TestName = "NumberAsString_Fails")]
    [TestCase("[{\"type\":\"media\",\"unique\":\"550e8400-e29b-41d4-a716-446655440000\"},{\"type\":\"content\",\"unique\":\"660e8400-e29b-41d4-a716-446655440000\"}]", true, TestName = "MultipleValidItems_Succeeds")]
    public async Task ValidateValueAsync_MultiNodeTreePicker_Validates_Uuid_Pattern(string jsonValue, bool shouldBeValid)
    {
        // Arrange
        var dataTypeKey = Guid.NewGuid();
        var multiNodeTreePickerEditor = CreateMultiNodeTreePickerPropertyEditor();
        SetupDataEditors(multiNodeTreePickerEditor);
        SetupDataType(dataTypeKey, Constants.PropertyEditors.Aliases.MultiNodeTreePicker);

        // Act
        var result = await _sut.ValidateValueAsync(dataTypeKey, jsonValue);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(PropertyEditorSchemaOperationStatus.Success));

        if (shouldBeValid)
        {
            Assert.That(result.Result, Is.Empty, "Expected no validation errors for valid value");
        }
        else
        {
            Assert.That(result.Result, Is.Not.Empty, "Expected validation errors for invalid UUID value");
        }
    }

    #endregion

    #region MediaPicker3

    [TestCase("null", true, TestName = "Null_Succeeds")]
    [TestCase("[]", true, TestName = "EmptyArray_Succeeds")]
    [TestCase("[{\"key\":\"550e8400-e29b-41d4-a716-446655440000\",\"mediaKey\":\"660e8400-e29b-41d4-a716-446655440000\"}]", true, TestName = "ValidGuidsWithDashes_Succeeds")]
    [TestCase("[{\"key\":\"550e8400e29b41d4a716446655440000\",\"mediaKey\":\"660e8400e29b41d4a716446655440000\"}]", true, TestName = "ValidGuidsNoDashes_Succeeds")]
    [TestCase("[{\"key\":\"invalid-guid\",\"mediaKey\":\"660e8400-e29b-41d4-a716-446655440000\"}]", false, TestName = "InvalidKeyGuid_Fails")]
    [TestCase("[{\"key\":\"550e8400-e29b-41d4-a716-446655440000\",\"mediaKey\":\"invalid-guid\"}]", false, TestName = "InvalidMediaKeyGuid_Fails")]
    [TestCase("[{\"key\":\"818\",\"mediaKey\":\"660e8400-e29b-41d4-a716-446655440000\"}]", false, TestName = "NumberAsStringKey_Fails")]
    public async Task ValidateValueAsync_MediaPicker3_Validates_Uuid_Pattern(string jsonValue, bool shouldBeValid)
    {
        // Arrange
        var dataTypeKey = Guid.NewGuid();
        var mediaPicker3Editor = CreateMediaPicker3PropertyEditor();
        SetupDataEditors(mediaPicker3Editor);
        SetupDataType(dataTypeKey, Constants.PropertyEditors.Aliases.MediaPicker3);

        // Act
        var result = await _sut.ValidateValueAsync(dataTypeKey, jsonValue);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(PropertyEditorSchemaOperationStatus.Success));

        if (shouldBeValid)
        {
            Assert.That(result.Result, Is.Empty, "Expected no validation errors for valid value");
        }
        else
        {
            Assert.That(result.Result, Is.Not.Empty, "Expected validation errors for invalid UUID value");
        }
    }

    #endregion

    #region MultiUrlPicker

    [TestCase("null", true, TestName = "Null_Succeeds")]
    [TestCase("[]", true, TestName = "EmptyArray_Succeeds")]
    [TestCase("[{\"unique\":\"550e8400-e29b-41d4-a716-446655440000\",\"type\":\"document\"}]", true, TestName = "ValidGuidWithDashes_Succeeds")]
    [TestCase("[{\"unique\":\"550e8400e29b41d4a716446655440000\",\"type\":\"document\"}]", true, TestName = "ValidGuidNoDashes_Succeeds")]
    [TestCase("[{\"unique\":null,\"type\":\"external\",\"url\":\"https://example.com\"}]", true, TestName = "NullUniqueForExternal_Succeeds")]
    [TestCase("[{\"url\":\"https://example.com\",\"type\":\"external\"}]", true, TestName = "NoUniqueForExternal_Succeeds")]
    [TestCase("[{\"unique\":\"invalid-guid\",\"type\":\"document\"}]", false, TestName = "InvalidGuid_Fails")]
    [TestCase("[{\"unique\":\"818\",\"type\":\"document\"}]", false, TestName = "NumberAsString_Fails")]
    public async Task ValidateValueAsync_MultiUrlPicker_Validates_Uuid_Pattern(string jsonValue, bool shouldBeValid)
    {
        // Arrange
        var dataTypeKey = Guid.NewGuid();
        var multiUrlPickerEditor = CreateMultiUrlPickerPropertyEditor();
        SetupDataEditors(multiUrlPickerEditor);
        SetupDataType(dataTypeKey, Constants.PropertyEditors.Aliases.MultiUrlPicker);

        // Act
        var result = await _sut.ValidateValueAsync(dataTypeKey, jsonValue);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(PropertyEditorSchemaOperationStatus.Success));

        if (shouldBeValid)
        {
            Assert.That(result.Result, Is.Empty, "Expected no validation errors for valid value");
        }
        else
        {
            Assert.That(result.Result, Is.Not.Empty, "Expected validation errors for invalid UUID value");
        }
    }

    #endregion

    #region Picker Editor Factory Methods

    private static ContentPickerPropertyEditor CreateContentPickerPropertyEditor()
    {
        var dataValueEditorFactory = Mock.Of<IDataValueEditorFactory>(f =>
            f.Create<DataValueEditor>(It.IsAny<DataEditorAttribute>()) ==
                new DataValueEditor(
                    Mock.Of<IShortStringHelper>(),
                    Mock.Of<IJsonSerializer>(),
                    Mock.Of<IIOHelper>(),
                    new DataEditorAttribute(Constants.PropertyEditors.Aliases.ContentPicker)));

        return new ContentPickerPropertyEditor(dataValueEditorFactory, Mock.Of<IIOHelper>());
    }

    private static MemberPickerPropertyEditor CreateMemberPickerPropertyEditor()
    {
        var dataValueEditorFactory = Mock.Of<IDataValueEditorFactory>(f =>
            f.Create<DataValueEditor>(It.IsAny<DataEditorAttribute>()) ==
                new DataValueEditor(
                    Mock.Of<IShortStringHelper>(),
                    Mock.Of<IJsonSerializer>(),
                    Mock.Of<IIOHelper>(),
                    new DataEditorAttribute(Constants.PropertyEditors.Aliases.MemberPicker)));

        return new MemberPickerPropertyEditor(dataValueEditorFactory);
    }

    private static UserPickerPropertyEditor CreateUserPickerPropertyEditor()
    {
        var dataValueEditorFactory = Mock.Of<IDataValueEditorFactory>(f =>
            f.Create<DataValueEditor>(It.IsAny<DataEditorAttribute>()) ==
                new DataValueEditor(
                    Mock.Of<IShortStringHelper>(),
                    Mock.Of<IJsonSerializer>(),
                    Mock.Of<IIOHelper>(),
                    new DataEditorAttribute(Constants.PropertyEditors.Aliases.UserPicker)));

        return new UserPickerPropertyEditor(dataValueEditorFactory);
    }

    private static MultiNodeTreePickerPropertyEditor CreateMultiNodeTreePickerPropertyEditor()
    {
        var dataValueEditorFactory = Mock.Of<IDataValueEditorFactory>(f =>
            f.Create<DataValueEditor>(It.IsAny<DataEditorAttribute>()) ==
                new DataValueEditor(
                    Mock.Of<IShortStringHelper>(),
                    Mock.Of<IJsonSerializer>(),
                    Mock.Of<IIOHelper>(),
                    new DataEditorAttribute(Constants.PropertyEditors.Aliases.MultiNodeTreePicker)));

        return new MultiNodeTreePickerPropertyEditor(dataValueEditorFactory, Mock.Of<IIOHelper>());
    }

    private static MediaPicker3PropertyEditor CreateMediaPicker3PropertyEditor()
    {
        var dataValueEditorFactory = Mock.Of<IDataValueEditorFactory>(f =>
            f.Create<DataValueEditor>(It.IsAny<DataEditorAttribute>()) ==
                new DataValueEditor(
                    Mock.Of<IShortStringHelper>(),
                    Mock.Of<IJsonSerializer>(),
                    Mock.Of<IIOHelper>(),
                    new DataEditorAttribute(Constants.PropertyEditors.Aliases.MediaPicker3)));

        return new MediaPicker3PropertyEditor(dataValueEditorFactory, Mock.Of<IIOHelper>());
    }

    private static MultiUrlPickerPropertyEditor CreateMultiUrlPickerPropertyEditor()
    {
        var dataValueEditorFactory = Mock.Of<IDataValueEditorFactory>(f =>
            f.Create<DataValueEditor>(It.IsAny<DataEditorAttribute>()) ==
                new DataValueEditor(
                    Mock.Of<IShortStringHelper>(),
                    Mock.Of<IJsonSerializer>(),
                    Mock.Of<IIOHelper>(),
                    new DataEditorAttribute(Constants.PropertyEditors.Aliases.MultiUrlPicker)));

        return new MultiUrlPickerPropertyEditor(Mock.Of<IIOHelper>(), dataValueEditorFactory);
    }

    #endregion
}
