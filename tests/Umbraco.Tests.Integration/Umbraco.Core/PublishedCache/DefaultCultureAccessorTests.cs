using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.PublishedCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class DefaultCultureAccessorTests : UmbracoIntegrationTest
{
    private IDefaultCultureAccessor DefaultCultureAccessor => GetRequiredService<IDefaultCultureAccessor>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<LanguageSavedNotification, LanguageSavedDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
    }

    [Test]
    public async Task Can_Get_Changed_Default_Culture_After_The_Default_Language_Is_Changed()
    {
        ILanguage danish = new LanguageBuilder()
            .WithCultureInfo("da-DK")
            .Build();
        await LanguageService.CreateAsync(danish, Constants.Security.SuperUserKey);

        Assert.AreEqual("en-US", DefaultCultureAccessor.DefaultCulture);

        danish.IsDefault = true;
        await LanguageService.UpdateAsync(danish, Constants.Security.SuperUserKey);

        Assert.AreEqual("da-DK", DefaultCultureAccessor.DefaultCulture);
    }
}
