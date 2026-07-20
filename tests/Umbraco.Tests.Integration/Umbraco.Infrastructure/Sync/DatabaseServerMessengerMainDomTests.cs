// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Sync;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Sync;

/// <summary>
/// Reproduces https://github.com/umbraco/Umbraco-CMS/issues/23219: an instance whose MainDom registration
/// failed (e.g. MainDom was released to an overlapping worker during an app pool recycle or deployment slot
/// swap before the messenger first initialized) must still write cache instructions when content is published,
/// otherwise subscriber servers silently never refresh.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class DatabaseServerMessengerMainDomTests : UmbracoIntegrationTest
{
    private IContentService ContentService => GetRequiredService<IContentService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private ICacheInstructionService CacheInstructionService => GetRequiredService<ICacheInstructionService>();

    private IFileService FileService => GetRequiredService<IFileService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.Services.AddUnique<IMainDom, NeverRegisteredMainDom>();
        builder.Services.AddUnique<IServerMessenger, BatchedDatabaseServerMessenger>();
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
    }

    [Test]
    public void Publish_WhenMainDomRegistrationFails_StillWritesCacheInstructions()
    {
        var maxInstructionIdBeforePublish = CacheInstructionService.GetMaxInstructionId();

        var template = TemplateBuilder.CreateTextPageTemplate("testPageTemplate");
        FileService.SaveTemplate(template);

        var contentType = ContentTypeBuilder.CreateSimpleContentType("testPage", "Test Page", defaultTemplateId: template.Id);
        ContentTypeService.Save(contentType);
        var content = ContentBuilder.CreateSimpleContent(contentType, "Test Content");
        ContentService.Save(content);

        var publishResult = ContentService.Publish(content, Array.Empty<string>());
        Assert.That(publishResult.Success, Is.True);

        var maxInstructionIdAfterPublish = CacheInstructionService.GetMaxInstructionId();
        Assert.That(
            maxInstructionIdAfterPublish,
            Is.GreaterThan(maxInstructionIdBeforePublish),
            "Publishing did not write any cache instructions, so subscriber servers would never refresh.");
    }

    /// <summary>
    /// Simulates an instance that lost the MainDom race: still running and serving traffic, but every
    /// <see cref="IMainDom.Register" /> call fails, as happens after MainDom has been released to another
    /// overlapping application instance.
    /// </summary>
    private sealed class NeverRegisteredMainDom : IMainDom
    {
        public bool IsMainDom => false;

        public bool Acquire(IApplicationShutdownRegistry hostingEnvironment) => true;

        public bool Register(Action? install = null, Action? release = null, int weight = 100) => false;
    }
}
