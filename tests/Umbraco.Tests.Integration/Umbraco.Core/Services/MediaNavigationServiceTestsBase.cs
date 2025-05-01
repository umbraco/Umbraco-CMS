using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Scoping;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal abstract class MediaNavigationServiceTestsBase : UmbracoIntegrationTest
{
    protected IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    // Testing with IMediaEditingService as it calls IMediaService underneath
    protected IMediaEditingService MediaEditingService => GetRequiredService<IMediaEditingService>();

    protected IMediaNavigationQueryService MediaNavigationQueryService => GetRequiredService<IMediaNavigationQueryService>();

    protected IMediaType FolderMediaType { get; set; }

    protected IMediaType ImageMediaType { get; set; }

    protected IMedia Album { get; set; }

    protected IMedia Image1 { get; set; }

    protected IMedia SubAlbum1 { get; set; }

    protected IMedia Image2 { get; set; }

    protected IMedia Image3 { get; set; }

    protected IMedia SubAlbum2 { get; set; }

    protected IMedia SubSubAlbum1 { get; set; }

    protected IMedia Image4 { get; set; }

    protected MediaCreateModel CreateMediaCreateModel(string name, Guid key, Guid mediaTypeKey, Guid? parentKey = null)
        => new()
        {
            ContentTypeKey = mediaTypeKey,
            ParentKey = parentKey ?? Constants.System.RootKey,
            Variants = [new () { Name = name }],
            Key = key,
        };

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.Services.AddUnique<IServerMessenger, ScopedRepositoryTests.LocalServerMessenger>();
        builder.AddNotificationHandler<MediaTreeChangeNotification, MediaTreeChangeDistributedCacheNotificationHandler>();
    }
}
