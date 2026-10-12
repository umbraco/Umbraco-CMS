using System.Collections.Concurrent;
using System.Data;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache.PropertyEditors;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.IO.MediaPathSchemes;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.PropertyEditors.NotificationHandlers;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PropertyEditors;

[TestFixture]
public class FileUploadContentDeletedNotificationHandlerTests
{
    private static readonly Guid _contentKey = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid _elementTypeKey = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Test]
    public void Handle_Deletes_A_Block_Nested_Upload_File_The_Item_Owns()
    {
        ConcurrentBag<string> deleted = Run(pickUploadValue: (owned, _) => owned, out var ownedPath, out _);

        // Also proves the block was actually parsed and its path collected - otherwise the guard case below
        // could pass vacuously.
        Assert.That(deleted, Does.Contain(ownedPath));
    }

    [Test]
    public void Handle_Does_Not_Delete_A_Block_Nested_Upload_File_Belonging_To_Another_Item()
    {
        ConcurrentBag<string> deleted = Run(pickUploadValue: (_, foreign) => foreign, out _, out var foreignPath);

        Assert.That(deleted, Does.Not.Contain(foreignPath));
        Assert.That(deleted, Is.Empty);
    }

    private static ConcurrentBag<string> Run(Func<string, string, string> pickUploadValue, out string ownedPath, out string foreignPath)
    {
        var deleted = new ConcurrentBag<string>();

        var fileSystem = new Mock<IFileSystem>();
        fileSystem.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);
        fileSystem.Setup(x => x.GetRelativePath(It.IsAny<string>())).Returns<string>(p => p.Replace('\\', '/').TrimStart('/'));
        fileSystem.Setup(x => x.DeleteFile(It.IsAny<string>())).Callback<string>(deleted.Add);

        var shortStringHelper = new Mock<IShortStringHelper>();
        shortStringHelper.Setup(x => x.CleanStringForSafeFileName(It.IsAny<string>())).Returns<string>(s => s);

        MediaFileManager manager = CreateMediaFileManager(fileSystem.Object, shortStringHelper.Object);

        // A block element type carrying an upload property; read the generated property type key back so we can
        // compute the paths that would (and would not) belong to this content and block property.
        IContentType elementType = new ContentTypeBuilder()
            .WithKey(_elementTypeKey)
            .AddPropertyType()
                .WithAlias(Constants.Conventions.Media.File)
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.UploadField)
                .Done()
            .Build();
        Guid uploadPropertyTypeKey = elementType.PropertyTypes.Single(x => x.Alias == Constants.Conventions.Media.File).Key;

        ownedPath = manager.GetMediaPath("file.txt", _contentKey, uploadPropertyTypeKey);
        foreignPath = manager.GetMediaPath("secret.txt", Guid.NewGuid(), uploadPropertyTypeKey);

        var elementTypeCache = new Mock<IBlockEditorElementTypeCache>();
        elementTypeCache.Setup(x => x.GetMany(It.IsAny<IEnumerable<Guid>>())).Returns(new[] { elementType });

        var blockJson = CreateBlockListJson(_elementTypeKey, Guid.NewGuid(), pickUploadValue(ownedPath, foreignPath));

        IContentType outerType = new ContentTypeBuilder()
            .WithKey(Guid.NewGuid())
            .AddPropertyType()
                .WithAlias("blocks")
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.BlockList)
                .Done()
            .Build();
        IContent content = new ContentBuilder()
            .WithContentType(outerType)
            .WithKey(_contentKey)
            .WithPropertyValues(new { blocks = blockJson })
            .Build();

        var contentSettings = new Mock<IOptionsMonitor<ContentSettings>>();
        contentSettings.Setup(x => x.CurrentValue).Returns(new ContentSettings());

        var handler = new FileUploadContentDeletedNotificationHandler(
            new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()),
            manager,
            elementTypeCache.Object,
            NullLogger<FileUploadContentDeletedNotificationHandler>.Instance,
            contentSettings.Object);

        handler.Handle(new ContentDeletedNotification(content, new EventMessages()));

        return deleted;
    }

    private static string CreateBlockListJson(Guid elementTypeKey, Guid blockKey, string uploadValue)
    {
        var root = new JsonObject
        {
            ["layout"] = new JsonObject
            {
                [Constants.PropertyEditors.Aliases.BlockList] = new JsonArray
                {
                    new JsonObject { ["$type"] = "BlockListLayoutItem", ["contentKey"] = blockKey.ToString() },
                },
            },
            ["contentData"] = new JsonArray
            {
                new JsonObject
                {
                    ["contentTypeKey"] = elementTypeKey.ToString(),
                    ["key"] = blockKey.ToString(),
                    ["values"] = new JsonArray
                    {
                        new JsonObject
                        {
                            ["editorAlias"] = Constants.PropertyEditors.Aliases.UploadField,
                            ["alias"] = Constants.Conventions.Media.File,
                            ["value"] = uploadValue,
                        },
                    },
                },
            },
        };

        return root.ToJsonString();
    }

    private static MediaFileManager CreateMediaFileManager(IFileSystem fileSystem, IShortStringHelper shortStringHelper)
    {
        var scopeProvider = new Mock<ICoreScopeProvider>();
        scopeProvider
            .Setup(x => x.CreateCoreScope(
                It.IsAny<IsolationLevel>(),
                It.IsAny<RepositoryCacheMode>(),
                It.IsAny<IEventDispatcher?>(),
                It.IsAny<IScopedNotificationPublisher?>(),
                It.IsAny<bool?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(Mock.Of<ICoreScope>());

        return new MediaFileManager(
            fileSystem,
            new CombinedGuidsMediaPathScheme(),
            NullLogger<MediaFileManager>.Instance,
            shortStringHelper,
            Mock.Of<IServiceProvider>(),
            new Lazy<ICoreScopeProvider>(() => scopeProvider.Object));
    }
}
