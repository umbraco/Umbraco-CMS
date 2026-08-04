using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

/// <summary>
/// Tests the correctness of the user group service's caching of allowed languages.
/// </summary>
/// <remarks>
/// Lives in its own fixture because the cache/messenger overrides in CustomTestSetup change the
/// environment for every test in a fixture, and the rest of UserGroupServiceTests should keep running
/// against the default (no-cache) harness.
/// </remarks>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class UserGroupLanguageCacheTests : UmbracoIntegrationTest
{
    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    // The integration harness registers AppCaches.NoCache (so user groups are never actually cached)
    // and a no-op server messenger (so cache refreshers never run). Use a real cache and a local,
    // synchronous messenger - as the web pipeline effectively does - so the stale-cache scenario and
    // its invalidation can be exercised.
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.Services.AddUnique(_ => new AppCaches(
            new DeepCloneAppCache(new ObjectCacheAppCache()),
            NoAppCache.Instance,
            new IsolatedCaches(_ => new DeepCloneAppCache(new ObjectCacheAppCache()))));
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();

        // The harness does not wire up the *DistributedCacheNotificationHandler set (those are only
        // registered for the real app in UmbracoBuilder.CoreServices), so register the handler under
        // test explicitly.
        builder.AddNotificationHandler<LanguageDeletedNotification, LanguageDeletedDistributedCacheNotificationHandler>();
    }

    [Test]
    public async Task Can_Evict_Deleted_Language_From_Cached_User_Group()
    {
        var language = new LanguageBuilder().WithCultureInfo("nb-NO").Build();
        var languageCreated = await LanguageService.CreateAsync(language, Constants.Security.SuperUserKey);
        Assert.IsTrue(languageCreated.Success);

        var userGroup = new UserGroup(ShortStringHelper)
        {
            Name = "Language Group",
            Alias = "languageGroup",
            HasAccessToAllLanguages = false,
        };
        userGroup.AddAllowedLanguage(language.Id);

        var created = await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);
        Assert.IsTrue(created.Success);
        var userGroupId = created.Result!.Id;

        // Populate both the per-id cache and the "get all" cache (the list/filter endpoint that
        // actually surfaced the bug uses the latter) while the language still exists.
        var cachedById = await UserGroupService.GetAsync(userGroupId);
        Assert.IsNotNull(cachedById);
        Assert.IsTrue(cachedById!.AllowedLanguages.Contains(language.Id));

        var cachedFromAll = (await UserGroupService.GetAllAsync(0, int.MaxValue)).Items.First(x => x.Id == userGroupId);
        Assert.IsTrue(cachedFromAll.AllowedLanguages.Contains(language.Id));

        var deleted = await LanguageService.DeleteAsync(language.IsoCode, Constants.Security.SuperUserKey);
        Assert.IsTrue(deleted.Success);

        // Both cached read paths must drop the now-deleted language.
        var reloadedById = await UserGroupService.GetAsync(userGroupId);
        Assert.IsNotNull(reloadedById);
        Assert.IsFalse(
            reloadedById!.AllowedLanguages.Contains(language.Id),
            "A deleted language should not remain on the cached user group (get-by-id path).");

        var reloadedFromAll = (await UserGroupService.GetAllAsync(0, int.MaxValue)).Items.First(x => x.Id == userGroupId);
        Assert.IsFalse(
            reloadedFromAll.AllowedLanguages.Contains(language.Id),
            "A deleted language should not remain on the cached user group (get-all path).");
    }
}
