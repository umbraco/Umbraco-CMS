using Moq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Tests.Search.Integration.Services;

namespace Umbraco.Tests.Search.Integration.Tests;

public class NoopPropertyValueHandlerTests : ContentTestBase
{
    [Test]
    public void AllNoopEditors_YieldNoValues()
    {
        IJsonSerializer jsonSerializer = GetRequiredService<IJsonSerializer>();

        Media media = new MediaBuilder()
            .WithMediaType(GetMediaType())
            .WithName("The media")
            .Build();
        GetRequiredService<IMediaService>().Save(media);

        Content content = new ContentBuilder()
            .WithContentType(GetContentType())
            .WithName("All Supported Editors")
            .WithPropertyValues(
                new
                {
                    emailValue = "some@email.com",
                    colorPickerWithLabelsValue = jsonSerializer.Serialize(new ColorPickerValueConverter.PickedColor("123456", "test")),
                    colorPickerWithoutLabelsValue = jsonSerializer.Serialize(new ColorPickerValueConverter.PickedColor("123456", "test")),
                    colorPickerEyeDropperValue = "123456",
                    mediaPicker3Value = media.GetUdi().ToString(),
                    imageCropperValue = jsonSerializer.Serialize(new ImageCropperValue { Src = "/some/file.jpg" }),
                    uploadValue = "/some/file.jpg",
                })
            .Build();

        ContentService.Save(content);
        ContentService.Publish(content, ["*"]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(documents, Has.Count.EqualTo(1));

        TestIndexDocument document = documents.Single();
        Assert.That(document.Fields.Any(), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(document.Fields.Any(f => f.FieldName == "emailValue"), Is.False);
            Assert.That(document.Fields.Any(f => f.FieldName == "colorPickerWithLabelsValue"), Is.False);
            Assert.That(document.Fields.Any(f => f.FieldName == "colorPickerWithoutLabelsValue"), Is.False);
            Assert.That(document.Fields.Any(f => f.FieldName == "colorPickerEyeDropperValue"), Is.False);
            Assert.That(document.Fields.Any(f => f.FieldName == "mediaPicker3Value"), Is.False);
            Assert.That(document.Fields.Any(f => f.FieldName == "imageCropperValue"), Is.False);
            Assert.That(document.Fields.Any(f => f.FieldName == "uploadValue"), Is.False);
        });

        // cross-check that the input values actually yielded the expected published values
        IPublishedContent? publishedContent = GetRequiredService<IPublishedContentCache>().GetById(content.Key);
        Assert.That(publishedContent, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(publishedContent.Value<string>("emailValue"), Is.EqualTo("some@email.com"));
            Assert.That(publishedContent.Value<ColorPickerValueConverter.PickedColor>("colorPickerWithLabelsValue")?.Color, Is.EqualTo("123456"));
            Assert.That(publishedContent.Value<string>("colorPickerWithoutLabelsValue"), Is.EqualTo("123456"));
            Assert.That(publishedContent.Value<string>("colorPickerEyeDropperValue"), Is.EqualTo("123456"));
            Assert.That(publishedContent.Value<IPublishedContent>("mediaPicker3Value")?.Name, Is.EqualTo("The media"));
            Assert.That(publishedContent.Value<ImageCropperValue>("imageCropperValue")?.Src, Is.EqualTo("/some/file.jpg"));
            Assert.That(publishedContent.Value<string>("uploadValue"), Is.EqualTo("/some/file.jpg"));
        });
    }

    [SetUp]
    public async Task SetupTest()
    {
        IDataTypeService dataTypeService = GetRequiredService<IDataTypeService>();

        DataType emailDataType = new DataTypeBuilder()
            .WithId(0)
            .WithDatabaseType(ValueStorageType.Nvarchar)
            .WithName("Email")
            .AddEditor()
            .WithAlias(Constants.PropertyEditors.Aliases.EmailAddress)
            .Done()
            .Build();
        await dataTypeService.CreateAsync(emailDataType, Constants.Security.SuperUserKey);

        DataType colorPickerWithLabelsDataType = new DataTypeBuilder()
            .WithId(0)
            .WithDatabaseType(ValueStorageType.Nvarchar)
            .WithName("Color Picker (with labels)")
            .AddEditor()
            .WithAlias(Constants.PropertyEditors.Aliases.ColorPicker)
            .Done()
            .Build();
        colorPickerWithLabelsDataType.ConfigurationData = new Dictionary<string, object>
        {
            { "useLabel", true },
            { "items", new [] { new { value = "123456", label = "test" } } }
        };
        await dataTypeService.CreateAsync(colorPickerWithLabelsDataType, Constants.Security.SuperUserKey);

        DataType colorPickerWithoutLabelsDataType = new DataTypeBuilder()
            .WithId(0)
            .WithDatabaseType(ValueStorageType.Nvarchar)
            .WithName("Color Picker (without labels)")
            .AddEditor()
            .WithAlias(Constants.PropertyEditors.Aliases.ColorPicker)
            .Done()
            .Build();
        colorPickerWithoutLabelsDataType.ConfigurationData = new Dictionary<string, object>
        {
            { "useLabel", false },
            { "items", new [] { new { value = "123456", label = "test" } } }
        };
        await dataTypeService.CreateAsync(colorPickerWithoutLabelsDataType, Constants.Security.SuperUserKey);

        DataType colorPickerEyeDropperDataType = new DataTypeBuilder()
            .WithId(0)
            .WithDatabaseType(ValueStorageType.Nvarchar)
            .WithName("Color Picker Eye Dropper")
            .AddEditor()
            .WithAlias(Constants.PropertyEditors.Aliases.ColorPickerEyeDropper)
            .Done()
            .Build();
        await dataTypeService.CreateAsync(colorPickerEyeDropperDataType, Constants.Security.SuperUserKey);

        DataType mediaPicker3DataType = new DataTypeBuilder()
            .WithId(0)
            .WithDatabaseType(ValueStorageType.Nvarchar)
            .WithName("Media Picker 3")
            .AddEditor()
            .WithAlias(Constants.PropertyEditors.Aliases.MediaPicker3)
            .Done()
            .Build();
        await dataTypeService.CreateAsync(mediaPicker3DataType, Constants.Security.SuperUserKey);

        IContentType contentType = new ContentTypeBuilder()
            .WithAlias("allEditors")
            .AddPropertyType()
            .WithAlias("emailValue")
            .WithDataTypeId(emailDataType.Id)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.EmailAddress)
            .Done()
            .AddPropertyType()
            .WithAlias("colorPickerWithLabelsValue")
            .WithDataTypeId(colorPickerWithLabelsDataType.Id)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.ColorPicker)
            .Done()
            .AddPropertyType()
            .WithAlias("colorPickerWithoutLabelsValue")
            .WithDataTypeId(colorPickerWithoutLabelsDataType.Id)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.ColorPicker)
            .Done()
            .AddPropertyType()
            .WithAlias("colorPickerEyeDropperValue")
            .WithDataTypeId(colorPickerEyeDropperDataType.Id)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.ColorPickerEyeDropper)
            .Done()
            .AddPropertyType()
            .WithAlias("mediaPicker3Value")
            .WithDataTypeId(mediaPicker3DataType.Id)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.MediaPicker3)
            .Done()
            .AddPropertyType()
            .WithAlias("imageCropperValue")
            .WithDataTypeId(Constants.DataTypes.ImageCropper)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.ImageCropper)
            .Done()
            .AddPropertyType()
            .WithAlias("uploadValue")
            .WithDataTypeId(Constants.DataTypes.Upload)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.UploadField)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
    }

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        // short circuit URL generation (i.e. for picked media)
        builder.Services.AddUnique(Mock.Of<IPublishedUrlProvider>());
    }

    private IContentType GetContentType() => ContentTypeService.Get("allEditors")
                                             ?? throw new InvalidOperationException("Could not find the content type");

    private IMediaType GetMediaType() => GetRequiredService<IMediaTypeService>().Get(Constants.Conventions.MediaTypes.Image)
                                             ?? throw new InvalidOperationException("Could not find the media type");
}
