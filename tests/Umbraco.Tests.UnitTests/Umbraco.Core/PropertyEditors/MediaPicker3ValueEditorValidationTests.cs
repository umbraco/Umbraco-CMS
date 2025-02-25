using System.ComponentModel.DataAnnotations;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
internal class MediaPicker3ValueEditorValidationTests
{
    [TestCase(true, true)]
    [TestCase(false, false)]
    public void Validates_Start_Node_Immediate_Parent(bool shouldSucceed, bool hasValidParentKey)
    {
        var (valueEditor, mediaTypeServiceMock, mediaNavigationQueryServiceMock) = CreateValueEditor();

        Guid? validParentKey = Guid.NewGuid();
        var mediaKey = Guid.NewGuid();

        if (hasValidParentKey)
        {
            mediaNavigationQueryServiceMock.Setup(x => x.TryGetParentKey(mediaKey, out validParentKey)).Returns(true);
        }
        else
        {
            Guid? invalidParentKey = Guid.NewGuid();
            mediaNavigationQueryServiceMock.Setup(x => x.TryGetParentKey(mediaKey, out invalidParentKey)).Returns(true);
        }

        valueEditor.ConfigurationObject = new MediaPicker3Configuration { StartNodeId = validParentKey };

        var value = "[ {\n  \" key\" : \"20266ebe-1f7e-4cf3-a694-7a5fb210223b\",\n  \"mediaKey\" : \"" + mediaKey + "\",\n  \"mediaTypeAlias\" : \"\",\n  \"crops\" : [ ],\n  \"focalPoint\" : null\n} ]";

        var result = valueEditor.Validate(value, false, null, PropertyValidationContext.Empty());

        ValidateResult(shouldSucceed, result);
    }

    [Test]
    public void Validates_Start_Node_Parent_Not_Found()
    {
        var (valueEditor, mediaTypeServiceMock, mediaNavigationQueryServiceMock) = CreateValueEditor();

        Guid? parentKey = null;
        var mediaKey = Guid.NewGuid();

        mediaNavigationQueryServiceMock.Setup(x => x.TryGetParentKey(mediaKey, out parentKey)).Returns(true);

        valueEditor.ConfigurationObject = new MediaPicker3Configuration { StartNodeId = Guid.NewGuid() };

        var value = "[ {\n  \" key\" : \"20266ebe-1f7e-4cf3-a694-7a5fb210223b\",\n  \"mediaKey\" : \"" + mediaKey + "\",\n  \"mediaTypeAlias\" : \"\",\n  \"crops\" : [ ],\n  \"focalPoint\" : null\n} ]";

        var result = valueEditor.Validate(value, false, null, PropertyValidationContext.Empty());

        ValidateResult(false, result);
    }

    [Test]
    [TestCase(true, true, true)]
    [TestCase(false, false, true)]
    [TestCase(false, true, false)]
    public void Validates_Start_Node_Ancestor(bool shouldSucceed, bool findsAncestor, bool hasValidAncestorKey)
    {
        var (valueEditor, mediaTypeServiceMock, mediaNavigationQueryServiceMock) = CreateValueEditor();

        Guid ancestorKey = Guid.NewGuid();
        Guid? parentKey = Guid.NewGuid();
        var mediaKey = Guid.NewGuid();
        IEnumerable<Guid> ancestorKeys = findsAncestor is false ? [] : hasValidAncestorKey ? [ancestorKey] : [Guid.NewGuid()];

        mediaNavigationQueryServiceMock.Setup(x => x.TryGetParentKey(mediaKey, out parentKey)).Returns(true);
        mediaNavigationQueryServiceMock.Setup(x => x.TryGetAncestorsKeys(parentKey.Value, out ancestorKeys)).Returns(true);

        valueEditor.ConfigurationObject = new MediaPicker3Configuration { StartNodeId = ancestorKey };

        var value = "[ {\n  \" key\" : \"20266ebe-1f7e-4cf3-a694-7a5fb210223b\",\n  \"mediaKey\" : \"" + mediaKey + "\",\n  \"mediaTypeAlias\" : \"\",\n  \"crops\" : [ ],\n  \"focalPoint\" : null\n} ]";

        var result = valueEditor.Validate(value, false, null, PropertyValidationContext.Empty());

        ValidateResult(shouldSucceed, result);
    }

    [TestCase(true, true)]
    [TestCase(false, true)]
    [TestCase(false, false)]
    public void Validates_Allowed_Type(bool succeed, bool findsMediaType)
    {
        var (valueEditor, mediaTypeServiceMock, mediaNavigationQueryServiceMock) = CreateValueEditor();

        var mediaKey = Guid.NewGuid();
        var mediaTypeKey = Guid.NewGuid();
        var mediaTypeAlias = "Alias";
        valueEditor.ConfigurationObject = new MediaPicker3Configuration() { Filter = $"{mediaTypeKey}" };
        var mediaTypeMock = new Mock<IMediaType>();

        if (succeed)
        {
            mediaTypeMock.Setup(x => x.Key).Returns(mediaTypeKey);
        }
        else
        {
            mediaTypeMock.Setup(x => x.Key).Returns(Guid.NewGuid());
        }

        if (findsMediaType)
        {
            mediaTypeServiceMock.Setup(x => x.Get(mediaTypeAlias)).Returns(mediaTypeMock.Object);
        }
        else
        {
            mediaTypeServiceMock.Setup(x => x.Get(It.IsAny<string>())).Returns((IMediaType)null);
        }

        var value = "[ {\n  \" key\" : \"20266ebe-1f7e-4cf3-a694-7a5fb210223b\",\n  \"mediaKey\" : \"" + mediaKey + "\",\n  \"mediaTypeAlias\" : \"" + mediaTypeAlias + "\",\n  \"crops\" : [ ],\n  \"focalPoint\" : null\n} ]";
        var result = valueEditor.Validate(value, false, null, PropertyValidationContext.Empty());

        ValidateResult(succeed, result);
    }

    [TestCase("[ {\n  \" key\" : \"20266ebe-1f7e-4cf3-a694-7a5fb210223b\",\n  \"mediaKey\" : \"7AD39018-0920-4818-89D3-26F47DBCE62E\",\n  \"mediaTypeAlias\" : \"\",\n  \"crops\" : [ ],\n  \"focalPoint\" : null\n} ]", false, true)]
    [TestCase("[ {\n  \" key\" : \"20266ebe-1f7e-4cf3-a694-7a5fb210223b\",\n  \"mediaKey\" : \"7AD39018-0920-4818-89D3-26F47DBCE62E\",\n  \"mediaTypeAlias\" : \"\",\n  \"crops\" : [ ],\n  \"focalPoint\" : null\n}, {\n  \" key\" : \"1C70519E-C3AE-4D45-8E48-30B3D02E455E\",\n  \"mediaKey\" : \"E243A7E2-8D2E-4DC9-88FB-822350A40142\",\n  \"mediaTypeAlias\" : \"\",\n  \"crops\" : [ ],\n  \"focalPoint\" : null\n} ]", false, false)]
    [TestCase("[ {\n  \" key\" : \"20266ebe-1f7e-4cf3-a694-7a5fb210223b\",\n  \"mediaKey\" : \"7AD39018-0920-4818-89D3-26F47DBCE62E\",\n  \"mediaTypeAlias\" : \"\",\n  \"crops\" : [ ],\n  \"focalPoint\" : null\n}, {\n  \" key\" : \"1C70519E-C3AE-4D45-8E48-30B3D02E455E\",\n  \"mediaKey\" : \"E243A7E2-8D2E-4DC9-88FB-822350A40142\",\n  \"mediaTypeAlias\" : \"\",\n  \"crops\" : [ ],\n  \"focalPoint\" : null\n} ]", true, true)]
    [TestCase("[]", true, true)]
    [TestCase("[]", false, true)]
    public void Validates_Multiple(string value, bool multiple, bool succeed)
    {
        var (valueEditor, mediaTypeServiceMock, mediaNavigationQueryServiceMock) = CreateValueEditor();

        valueEditor.ConfigurationObject = new MediaPicker3Configuration() { Multiple = multiple };

        var result = valueEditor.Validate(value, false, null, PropertyValidationContext.Empty());
        ValidateResult(succeed, result);
    }

    [TestCase("[ {\n  \" key\" : \"20266ebe-1f7e-4cf3-a694-7a5fb210223b\",\n  \"mediaKey\" : \"7AD39018-0920-4818-89D3-26F47DBCE62E\",\n  \"mediaTypeAlias\" : \"\",\n  \"crops\" : [ ],\n  \"focalPoint\" : null\n} ]", 2, false)]
    [TestCase("[ {\n  \" key\" : \"20266ebe-1f7e-4cf3-a694-7a5fb210223b\",\n  \"mediaKey\" : \"7AD39018-0920-4818-89D3-26F47DBCE62E\",\n  \"mediaTypeAlias\" : \"\",\n  \"crops\" : [ ],\n  \"focalPoint\" : null\n}, {\n  \" key\" : \"1C70519E-C3AE-4D45-8E48-30B3D02E455E\",\n  \"mediaKey\" : \"E243A7E2-8D2E-4DC9-88FB-822350A40142\",\n  \"mediaTypeAlias\" : \"\",\n  \"crops\" : [ ],\n  \"focalPoint\" : null\n} ]", 1, true)]
    [TestCase("[ {\n  \" key\" : \"20266ebe-1f7e-4cf3-a694-7a5fb210223b\",\n  \"mediaKey\" : \"7AD39018-0920-4818-89D3-26F47DBCE62E\",\n  \"mediaTypeAlias\" : \"\",\n  \"crops\" : [ ],\n  \"focalPoint\" : null\n}, {\n  \" key\" : \"1C70519E-C3AE-4D45-8E48-30B3D02E455E\",\n  \"mediaKey\" : \"E243A7E2-8D2E-4DC9-88FB-822350A40142\",\n  \"mediaTypeAlias\" : \"\",\n  \"crops\" : [ ],\n  \"focalPoint\" : null\n} ]", 2, true)]
    [TestCase("[ {\n  \" key\" : \"20266ebe-1f7e-4cf3-a694-7a5fb210223b\",\n  \"mediaKey\" : \"7AD39018-0920-4818-89D3-26F47DBCE62E\",\n  \"mediaTypeAlias\" : \"\",\n  \"crops\" : [ ],\n  \"focalPoint\" : null\n}, {\n  \" key\" : \"1C70519E-C3AE-4D45-8E48-30B3D02E455E\",\n  \"mediaKey\" : \"E243A7E2-8D2E-4DC9-88FB-822350A40142\",\n  \"mediaTypeAlias\" : \"\",\n  \"crops\" : [ ],\n  \"focalPoint\" : null\n} ]", 3, false)]
    [TestCase("[]", 1, false)]
    [TestCase("[]", 0, true)]
    public void Validates_Min_Limit(string value, int min, bool succeed)
    {
        var (valueEditor, mediaTypeServiceMock, mediaNavigationQueryServiceMock) = CreateValueEditor();

        valueEditor.ConfigurationObject = new MediaPicker3Configuration() { Multiple = true, ValidationLimit = new MediaPicker3Configuration.NumberRange { Min = min } };

        var result = valueEditor.Validate(value, false, null, PropertyValidationContext.Empty());

        ValidateResult(succeed, result);
    }

    [TestCase("[ {\n  \" key\" : \"20266ebe-1f7e-4cf3-a694-7a5fb210223b\",\n  \"mediaKey\" : \"7AD39018-0920-4818-89D3-26F47DBCE62E\",\n  \"mediaTypeAlias\" : \"\",\n  \"crops\" : [ ],\n  \"focalPoint\" : null\n} ]", 1, true)]
    [TestCase("[ {\n  \" key\" : \"20266ebe-1f7e-4cf3-a694-7a5fb210223b\",\n  \"mediaKey\" : \"7AD39018-0920-4818-89D3-26F47DBCE62E\",\n  \"mediaTypeAlias\" : \"\",\n  \"crops\" : [ ],\n  \"focalPoint\" : null\n} ]", 0, false)]
    [TestCase("[ {\n  \" key\" : \"20266ebe-1f7e-4cf3-a694-7a5fb210223b\",\n  \"mediaKey\" : \"7AD39018-0920-4818-89D3-26F47DBCE62E\",\n  \"mediaTypeAlias\" : \"\",\n  \"crops\" : [ ],\n  \"focalPoint\" : null\n}, {\n  \" key\" : \"1C70519E-C3AE-4D45-8E48-30B3D02E455E\",\n  \"mediaKey\" : \"E243A7E2-8D2E-4DC9-88FB-822350A40142\",\n  \"mediaTypeAlias\" : \"\",\n  \"crops\" : [ ],\n  \"focalPoint\" : null\n} ]", 1, false)]
    [TestCase("[ {\n  \" key\" : \"20266ebe-1f7e-4cf3-a694-7a5fb210223b\",\n  \"mediaKey\" : \"7AD39018-0920-4818-89D3-26F47DBCE62E\",\n  \"mediaTypeAlias\" : \"\",\n  \"crops\" : [ ],\n  \"focalPoint\" : null\n}, {\n  \" key\" : \"1C70519E-C3AE-4D45-8E48-30B3D02E455E\",\n  \"mediaKey\" : \"E243A7E2-8D2E-4DC9-88FB-822350A40142\",\n  \"mediaTypeAlias\" : \"\",\n  \"crops\" : [ ],\n  \"focalPoint\" : null\n} ]", 2, true)]
    [TestCase("[ {\n  \" key\" : \"20266ebe-1f7e-4cf3-a694-7a5fb210223b\",\n  \"mediaKey\" : \"7AD39018-0920-4818-89D3-26F47DBCE62E\",\n  \"mediaTypeAlias\" : \"\",\n  \"crops\" : [ ],\n  \"focalPoint\" : null\n}, {\n  \" key\" : \"1C70519E-C3AE-4D45-8E48-30B3D02E455E\",\n  \"mediaKey\" : \"E243A7E2-8D2E-4DC9-88FB-822350A40142\",\n  \"mediaTypeAlias\" : \"\",\n  \"crops\" : [ ],\n  \"focalPoint\" : null\n} ]", 3, true)]
    [TestCase("[]", 1, true)]
    [TestCase("[]", 0, true)]
    public void Validates_Max_Limit(string value, int max, bool succeed)
    {
        var (valueEditor, mediaTypeServiceMock, mediaNavigationQueryServiceMock) = CreateValueEditor();

        valueEditor.ConfigurationObject = new MediaPicker3Configuration() { Multiple = true, ValidationLimit = new MediaPicker3Configuration.NumberRange { Max = max } };

        var result = valueEditor.Validate(value, false, null, PropertyValidationContext.Empty());
        ValidateResult(succeed, result);
    }

    private static void ValidateResult(bool succeed, IEnumerable<ValidationResult> result)
    {
        if (succeed)
        {
            Assert.IsEmpty(result);
        }
        else
        {
            Assert.That(result.Count(), Is.EqualTo(1));
        }
    }

    private static (MediaPicker3PropertyEditor.MediaPicker3PropertyValueEditor ValueEditor, Mock<IMediaTypeService> MediaTypeServiceMock, Mock<IMediaNavigationQueryService> MediaNavigationQueryServiceMock) CreateValueEditor()
    {
        var mediaTypeServiceMock = new Mock<IMediaTypeService>();
        var mediaNavigationQueryServiceMock = new Mock<IMediaNavigationQueryService>();
        var valueEditor = new MediaPicker3PropertyEditor.MediaPicker3PropertyValueEditor(
            Mock.Of<IShortStringHelper>(),
            new SystemTextJsonSerializer(),
            Mock.Of<IIOHelper>(),
            new DataEditorAttribute("alias"),
            Mock.Of<IMediaImportService>(),
            Mock.Of<IMediaService>(),
            Mock.Of<ITemporaryFileService>(),
            Mock.Of<IScopeProvider>(),
            Mock.Of<IBackOfficeSecurityAccessor>(),
            Mock.Of<IDataTypeConfigurationCache>(),
            Mock.Of<ILocalizedTextService>(),
            mediaTypeServiceMock.Object,
            mediaNavigationQueryServiceMock.Object)
        {
            ConfigurationObject = new MediaPicker3Configuration()
        };

        return (valueEditor, mediaTypeServiceMock, mediaNavigationQueryServiceMock);
    }
}
