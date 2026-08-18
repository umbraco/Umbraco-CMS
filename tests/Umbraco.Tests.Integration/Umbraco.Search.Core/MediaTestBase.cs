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

    protected IMedia RootFolder() => MediaService.GetById(RootFolderKey) ?? throw new InvalidOperationException("Root folder was not found");

    protected IMedia ChildFolder() => MediaService.GetById(ChildFolderKey) ?? throw new InvalidOperationException("Child folder was not found");

    protected IMedia RootMedia() => MediaService.GetById(RootMediaKey) ?? throw new InvalidOperationException("Root media was not found");

    protected IMedia ChildMedia() => MediaService.GetById(ChildMediaKey) ?? throw new InvalidOperationException("Child media was not found");

    protected IMedia GrandchildMedia() => MediaService.GetById(GrandchildMediaKey) ?? throw new InvalidOperationException("Child media was not found");

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
        await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);

        IMediaType folderType = MediaTypeService.Get(Constants.Conventions.MediaTypes.Folder) ?? throw new InvalidOperationException("Media type \"Folder\" was not found");
        folderType.AllowedContentTypes = [new ContentTypeSort(folderType.Key, 0, folderType.Alias), new ContentTypeSort(mediaType.Key, 1, mediaType.Alias)];
        await MediaTypeService.UpdateAsync(folderType, Constants.Security.SuperUserKey);

        Media rootFolder = new MediaBuilder()
            .WithKey(RootFolderKey)
            .WithMediaType(folderType)
            .WithName("Root folder")
            .Build();
        MediaService.Save(rootFolder);

        Media childFolder = new MediaBuilder()
            .WithKey(ChildFolderKey)
            .WithMediaType(folderType)
            .WithName("Child folder")
            .WithParentId(rootFolder.Id)
            .Build();
        MediaService.Save(childFolder);

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
        MediaService.Save(rootMedia);

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
        MediaService.Save(childMedia);

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
        MediaService.Save(grandchildMedia);

        IndexerAndSearcher.Reset();
    }
}
