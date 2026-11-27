using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

public abstract class MediaTestBase : ContentBaseTestBase
{
    protected IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    protected IMediaService MediaService => GetRequiredService<IMediaService>();

    protected Guid RootFolderKey { get; } = Guid.NewGuid();

    protected Guid ChildFolderKey { get; } = Guid.NewGuid();

    protected Guid RootMediaKey { get; } = Guid.NewGuid();

    protected Guid ChildMediaKey { get; } = Guid.NewGuid();

    protected Guid GrandchildMediaKey { get; } = Guid.NewGuid();

    protected IMedia RootFolder() => MediaService.GetByIdAsync(RootFolderKey, CancellationToken.None).GetAwaiter().GetResult() ?? throw new InvalidOperationException("Root folder was not found");

    protected IMedia ChildFolder() => MediaService.GetByIdAsync(ChildFolderKey, CancellationToken.None).GetAwaiter().GetResult() ?? throw new InvalidOperationException("Child folder was not found");

    protected IMedia RootMedia() => MediaService.GetByIdAsync(RootMediaKey, CancellationToken.None).GetAwaiter().GetResult() ?? throw new InvalidOperationException("Root media was not found");

    protected IMedia ChildMedia() => MediaService.GetByIdAsync(ChildMediaKey, CancellationToken.None).GetAwaiter().GetResult() ?? throw new InvalidOperationException("Child media was not found");

    protected IMedia GrandchildMedia() => MediaService.GetByIdAsync(GrandchildMediaKey, CancellationToken.None).GetAwaiter().GetResult() ?? throw new InvalidOperationException("Child media was not found");

    [SetUp]
    public async Task SetupTest()
    {
        IMediaType mediaType = new MediaTypeBuilder()
            .WithAlias("myMediaType")
            .AddPropertyGroup()
            .WithName("Group")
            .AddPropertyType()
            .WithAlias("bytes")
            .WithDataTypeId(-51)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Integer)
            .Done()
            .AddPropertyType()
            .WithAlias("altText")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .AddPropertyType()
            .WithAlias("tags")
            .WithDataTypeId(Constants.DataTypes.Tags)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Tags)
            .Done()
            .Done()
            .Build();
        await MediaTypeService.CreateAsync(mediaType, Cms.Core.Constants.Security.SuperUserKey);

        IMediaType folderType = (await MediaTypeService.GetAsync(Constants.Conventions.MediaTypes.Folder)) ?? throw new InvalidOperationException("Media type \"Folder\" was not found");
        folderType.AllowedContentTypes = [new ContentTypeSort(folderType.Key, 0, folderType.Alias), new ContentTypeSort(mediaType.Key, 1, mediaType.Alias)];
        await MediaTypeService.UpdateAsync(folderType, Cms.Core.Constants.Security.SuperUserKey);

        Media rootFolder = new MediaBuilder()
            .WithKey(RootFolderKey)
            .WithMediaType(folderType)
            .WithName("Root folder")
            .Build();
        await MediaService.SaveAsync(rootFolder, Cms.Core.Constants.Security.SuperUserKey, CancellationToken.None);

        Media childFolder = new MediaBuilder()
            .WithKey(ChildFolderKey)
            .WithMediaType(folderType)
            .WithName("Child folder")
            .WithParentId(rootFolder.Id)
            .Build();
        await MediaService.SaveAsync(childFolder, Cms.Core.Constants.Security.SuperUserKey, CancellationToken.None);

        Media rootMedia = new MediaBuilder()
            .WithKey(RootMediaKey)
            .WithMediaType(mediaType)
            .WithName("Root media")
            .WithPropertyValues(
                new
                {
                    altText = "The root alt text",
                    bytes = 1234,
                    tags = "[\"tag1\",\"tag2\"]"
                })
            .Build();
        await MediaService.SaveAsync(rootMedia, Cms.Core.Constants.Security.SuperUserKey, CancellationToken.None);

        Media childMedia = new MediaBuilder()
            .WithKey(ChildMediaKey)
            .WithMediaType(mediaType)
            .WithName("Child media")
            .WithParentId(rootFolder.Id)
            .WithPropertyValues(
                new
                {
                    altText = "The child alt text",
                    bytes = 5678,
                    tags = "[\"tag3\",\"tag4\"]"
                })
            .Build();
        await MediaService.SaveAsync(childMedia, Cms.Core.Constants.Security.SuperUserKey, CancellationToken.None);

        Media grandchildMedia = new MediaBuilder()
            .WithKey(GrandchildMediaKey)
            .WithMediaType(mediaType)
            .WithName("Grandchild media")
            .WithParentId(childFolder.Id)
            .WithPropertyValues(
                new
                {
                    altText = "The grandchild alt text",
                    bytes = 9012,
                    tags = "[\"tag5\",\"tag6\"]"
                })
            .Build();
        await MediaService.SaveAsync(grandchildMedia, Cms.Core.Constants.Security.SuperUserKey, CancellationToken.None);

        IndexerAndSearcher.Reset();
    }
}
