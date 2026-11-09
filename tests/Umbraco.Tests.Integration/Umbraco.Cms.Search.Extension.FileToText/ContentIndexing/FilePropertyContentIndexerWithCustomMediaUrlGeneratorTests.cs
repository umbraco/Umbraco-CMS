using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Search.Indexing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Extensions;
using CoreConstants = Umbraco.Cms.Core.Constants;
using FileToTextConstants = Umbraco.Cms.Search.Extension.FileToText.Constants;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Extension.FileToText.Tests.ContentIndexing;

internal class FilePropertyContentIndexerWithCustomMediaUrlGeneratorTests : FilePropertyContentIndexerTestBase
{
    private const string CustomFilePropertyAlias = "customFile";

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);
        builder.MediaUrlGenerators().Add<TextBoxMediaUrlGenerator>();
    }

    [Test]
    public async Task Can_Handle_Files_From_Custom_Media_Url_Generators()
    {
        IContentType contentType = new ContentTypeBuilder()
            .WithAlias("customFilePage")
            .WithName("Custom File Page")
            .AddPropertyType()
                .WithAlias(CustomFilePropertyAlias)
                .WithName("Custom File")
                .WithDataTypeId(CoreConstants.DataTypes.Textbox)
                .WithPropertyEditorAlias(CoreConstants.PropertyEditors.Aliases.TextBox)
                .Done()
            .Build();

        Attempt<ContentTypeOperationStatus> result = await ContentTypeService.CreateAsync(contentType, CoreConstants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True, $"Could not create the content type: {result.Result}");

        IContent content = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("Custom File Content")
            .Build();
        content.SetValue(CustomFilePropertyAlias, await AddLoremIpsumFileToMediaFileSystem(".md"));

        IndexField[] fields = (await FilePropertyContentIndexer.GetIndexFieldsAsync(content, [], false, CancellationToken.None)).ToArray();
        Assert.That(fields, Has.Length.EqualTo(1));

        IndexField field = fields.Single();
        Assert.That(field.FieldName, Is.EqualTo($"{CustomFilePropertyAlias}{FileToTextConstants.TextFieldPostfix}"));

        var texts = field.Value.Texts?.ToArray();
        Assert.That(texts, Is.Not.Null);
        Assert.That(texts, Has.Length.EqualTo(1));
        Assert.That(texts.Single(), Does.StartWith(TestFileHelper.LoremIpsumStart));
    }

    private class TextBoxMediaUrlGenerator : IMediaUrlGenerator
    {
        public bool TryGetMediaPath(string? propertyEditorAlias, object? value, out string? mediaPath)
        {
            mediaPath = propertyEditorAlias == CoreConstants.PropertyEditors.Aliases.TextBox ? value as string : null;
            return mediaPath.IsNullOrWhiteSpace() is false;
        }
    }
}
