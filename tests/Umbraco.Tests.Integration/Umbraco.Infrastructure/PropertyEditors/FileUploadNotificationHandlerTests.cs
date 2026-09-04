// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.PropertyEditors.NotificationHandlers;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.PropertyEditors;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class FileUploadNotificationHandlerTests : UmbracoIntegrationTest
{
    private const string Prefix = "copied-prefix";

    private MediaFileManager MediaFileManager => GetRequiredService<MediaFileManager>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    // Prefix every resolved path so we can assert that the copy handler routes its destination path through
    // IFileUploadPathResolver (rather than computing it directly). The file upload notification handlers are
    // registered by AddCoreNotifications() (in the web layer), which the integration harness does not call, so
    // they are registered here explicitly.
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.Services.AddUnique<IFileUploadPathResolver>(sp =>
            new PrefixAllResolver(sp.GetRequiredService<MediaFileManager>(), Prefix));

        builder.AddNotificationHandler<ContentCopiedNotification, FileUploadContentCopiedOrScaffoldedNotificationHandler>();
        builder.AddNotificationHandler<ContentScaffoldedNotification, FileUploadContentCopiedOrScaffoldedNotificationHandler>();
        builder.AddNotificationHandler<ContentDeletedNotification, FileUploadContentDeletedNotificationHandler>();
    }

    [Test]
    public async Task Can_Copy_File_Upload_To_Resolved_Path()
    {
        (_, IContent content, var sourceRelativePath) = await CreateContentWithUploadedFileAsync();

        IContent? copy = ContentService.Copy(content, Constants.System.Root, relateToOriginal: false);
        Assert.IsNotNull(copy);

        var copiedValue = ContentService.GetById(copy!.Key)!.GetValue<string>("file");
        Assert.IsNotNull(copiedValue);

        var copiedRelativePath = MediaFileManager.FileSystem.GetRelativePath(copiedValue!);
        Assert.Multiple(() =>
        {
            Assert.That(copiedRelativePath, Does.StartWith(Prefix + "/"), "Expected the copied file to be under the resolver's prefix.");
            Assert.IsTrue(MediaFileManager.FileSystem.FileExists(copiedRelativePath), "Expected the copied file to physically exist.");
            Assert.AreNotEqual(sourceRelativePath, copiedRelativePath, "Expected the copy to be a distinct file from the source.");
        });
    }

    [Test]
    public async Task Can_Delete_File_On_Content_Delete()
    {
        (_, IContent content, var sourceRelativePath) = await CreateContentWithUploadedFileAsync();
        Assert.IsTrue(MediaFileManager.FileSystem.FileExists(sourceRelativePath));

        ContentService.Delete(content);

        Assert.IsFalse(
            MediaFileManager.FileSystem.FileExists(sourceRelativePath),
            "Expected the associated file to be removed when the content was deleted.");
    }

    [Test]
    public async Task Can_Scaffold_File_Upload_To_Resolved_Path_Without_Resave()
    {
        (IContentType contentType, IContent original, _) = await CreateContentWithUploadedFileAsync();

        // A scaffold is an in-memory target that the handler updates but does not re-save (postUpdateAction is null).
        Content scaffold = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("Scaffold")
            .Build();

        GetRequiredService<IEventAggregator>().Publish(
            new ContentScaffoldedNotification(original, scaffold, Constants.System.Root, new EventMessages()));

        var scaffoldedValue = scaffold.GetValue<string>("file");
        Assert.IsNotNull(scaffoldedValue, "Expected the scaffolded (in-memory) property value to be populated.");

        var relativePath = MediaFileManager.FileSystem.GetRelativePath(scaffoldedValue!);
        Assert.Multiple(() =>
        {
            Assert.That(relativePath, Does.StartWith(Prefix + "/"), "Expected the scaffolded file to be under the resolver's prefix.");
            Assert.IsTrue(MediaFileManager.FileSystem.FileExists(relativePath), "Expected the scaffolded file to physically exist.");
        });
    }

    private async Task<(IContentType ContentType, IContent Content, string SourceRelativePath)> CreateContentWithUploadedFileAsync()
    {
        var contentType = new ContentTypeBuilder()
            .WithAlias("uploadPage")
            .AddPropertyType()
                .WithAlias("file")
                .WithName("File")
                .WithDataTypeId(Constants.DataTypes.Upload)
                .WithDataTypeKey(Constants.DataTypes.Guids.UploadGuid)
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.UploadField)
                .WithValueStorageType(ValueStorageType.Nvarchar)
                .Done()
            .WithAllowAsRoot(true)
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var sourceRelativePath = MediaFileManager.GetMediaPath("test.txt", Guid.NewGuid(), Guid.NewGuid());
        MediaFileManager.FileSystem.AddFile(sourceRelativePath, new MemoryStream("test"u8.ToArray()));

        var content = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("Original")
            .Build();
        content.SetValue("file", MediaFileManager.FileSystem.GetUrl(sourceRelativePath));
        ContentService.Save(content);

        return (contentType, content, sourceRelativePath);
    }

    private sealed class PrefixAllResolver : IFileUploadPathResolver
    {
        private readonly MediaFileManager _mediaFileManager;
        private readonly string _prefix;

        public PrefixAllResolver(MediaFileManager mediaFileManager, string prefix)
        {
            _mediaFileManager = mediaFileManager;
            _prefix = prefix;
        }

        public string ResolvePath(string fileName, object? dataTypeConfiguration, Guid contentKey, Guid propertyTypeKey)
            => $"{_prefix}/{_mediaFileManager.GetMediaPath(fileName, contentKey, propertyTypeKey)}";
    }
}
