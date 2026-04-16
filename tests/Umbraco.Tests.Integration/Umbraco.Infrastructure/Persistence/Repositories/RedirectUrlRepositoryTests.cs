// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class RedirectUrlRepositoryTests : UmbracoIntegrationTest
{
    [SetUp]
    public async Task SetUp() => await CreateTestDataAsync();

    [Test]
    public async Task Can_Save_And_Get()
    {
        var provider = NewScopeProvider;

        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository();
            var rurl = new RedirectUrl { ContentKey = _textpage.Key, Url = "blah" };
            await repo.SaveAsync(rurl, CancellationToken.None);
            scope.Complete();

            Assert.AreNotEqual(0, rurl.Id);
        }

        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository();
            var rurl = await repo.GetMostRecentUrlAsync("blah");
            scope.Complete();

            Assert.IsNotNull(rurl);
            Assert.AreEqual(_textpage.Id, rurl.ContentId);
        }
    }

    [Test]
    public async Task Can_Save_And_Get_With_Culture()
    {
        var culture = "en";
        using (var scope = NewScopeProvider.CreateScope())
        {
            var repo = CreateRepository();
            var rurl = new RedirectUrl { ContentKey = _textpage.Key, Url = "blah", Culture = culture };
            await repo.SaveAsync(rurl, CancellationToken.None);
            scope.Complete();

            Assert.AreNotEqual(0, rurl.Id);
        }

        using (var scope = NewScopeProvider.CreateScope())
        {
            var repo = CreateRepository();
            var rurl = await repo.GetMostRecentUrlAsync("blah");
            scope.Complete();

            Assert.IsNotNull(rurl);
            Assert.AreEqual(_textpage.Id, rurl.ContentId);
            Assert.AreEqual(culture, rurl.Culture);
        }
    }

    [Test]
    public async Task Can_Save_And_Get_Most_Recent()
    {
        var provider = NewScopeProvider;

        Assert.AreNotEqual(_textpage.Id, _otherpage.Id);

        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository();
            var rurl = new RedirectUrl { ContentKey = _textpage.Key, Url = "blah" };
            await repo.SaveAsync(rurl, CancellationToken.None);
            scope.Complete();

            Assert.AreNotEqual(0, rurl.Id);

            // TODO: too fast = same date = key violation?
            // and... can that happen in real life?
            // we don't really *care* about the IX, only supposed to make things faster...
            // BUT in realife we AddOrUpdate in a trx so it should be safe, always
            rurl = new RedirectUrl
            {
                ContentKey = _otherpage.Key,
                Url = "blah",
                CreateDateUtc = rurl.CreateDateUtc.AddSeconds(1) // ensure time difference
            };
            await repo.SaveAsync(rurl, CancellationToken.None);
            scope.Complete();

            Assert.AreNotEqual(0, rurl.Id);
        }

        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository();
            var rurl = await repo.GetMostRecentUrlAsync("blah");
            scope.Complete();

            Assert.IsNotNull(rurl);
            Assert.AreEqual(_otherpage.Id, rurl.ContentId);
        }
    }

    [Test]
    public async Task Can_Save_And_Get_Most_Recent_For_Culture()
    {
        var cultureA = "en";
        var cultureB = "de";
        Assert.AreNotEqual(_textpage.Id, _otherpage.Id);

        using (var scope = NewScopeProvider.CreateScope())
        {
            var repo = CreateRepository();
            var rurl = new RedirectUrl { ContentKey = _textpage.Key, Url = "blah", Culture = cultureA };
            await repo.SaveAsync(rurl, CancellationToken.None);
            scope.Complete();

            Assert.AreNotEqual(0, rurl.Id);

            // TODO: too fast = same date = key violation?
            // and... can that happen in real life?
            // we don't really *care* about the IX, only supposed to make things faster...
            // BUT in realife we AddOrUpdate in a trx so it should be safe, always
            rurl = new RedirectUrl
            {
                ContentKey = _otherpage.Key,
                Url = "blah",
                CreateDateUtc = rurl.CreateDateUtc.AddSeconds(1), // ensure time difference
                Culture = cultureB
            };
            await repo.SaveAsync(rurl, CancellationToken.None);
            scope.Complete();

            Assert.AreNotEqual(0, rurl.Id);
        }

        using (var scope = NewScopeProvider.CreateScope())
        {
            var repo = CreateRepository();
            var rurl = await repo.GetMostRecentUrlAsync("blah", cultureA);
            scope.Complete();

            Assert.IsNotNull(rurl);
            Assert.AreEqual(_textpage.Id, rurl.ContentId);
            Assert.AreEqual(cultureA, rurl.Culture);
        }
    }

    [Test]
    public async Task Can_Save_And_Get_By_Content()
    {
        var provider = NewScopeProvider;

        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository();
            var rurl = new RedirectUrl { ContentKey = _textpage.Key, Url = "blah" };
            await repo.SaveAsync(rurl, CancellationToken.None);
            scope.Complete();

            Assert.AreNotEqual(0, rurl.Id);

            // TODO: goes too fast and bam, errors, first is blah
            rurl = new RedirectUrl
            {
                ContentKey = _textpage.Key,
                Url = "durg",
                CreateDateUtc = rurl.CreateDateUtc.AddSeconds(1) // ensure time difference
            };
            await repo.SaveAsync(rurl, CancellationToken.None);
            scope.Complete();

            Assert.AreNotEqual(0, rurl.Id);
        }

        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository();
            var rurls = (await repo.GetContentUrlsAsync(_textpage.Key)).ToArray();
            scope.Complete();

            Assert.AreEqual(2, rurls.Length);
            Assert.AreEqual("durg", rurls[0].Url);
            Assert.AreEqual("blah", rurls[1].Url);
        }
    }

    [Test]
    public async Task Can_Save_And_Delete()
    {
        var provider = NewScopeProvider;

        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository();
            var rurl = new RedirectUrl { ContentKey = _textpage.Key, Url = "blah" };
            await repo.SaveAsync(rurl, CancellationToken.None);
            scope.Complete();

            Assert.AreNotEqual(0, rurl.Id);

            rurl = new RedirectUrl { ContentKey = _otherpage.Key, Url = "durg" };
            await repo.SaveAsync(rurl, CancellationToken.None);
            scope.Complete();

            Assert.AreNotEqual(0, rurl.Id);
        }

        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository();
            await repo.DeleteContentUrlsAsync(_textpage.Key);
            scope.Complete();

            var rurls = await repo.GetContentUrlsAsync(_textpage.Key);

            Assert.AreEqual(0, rurls.Count());
        }
    }

    [Test]
    public async Task Can_Get_All_Urls_Filtered_By_Root_Content_Id()
    {
        var provider = NewScopeProvider;

        // Create redirects for content at different levels of the hierarchy (_textpage is root,
        // _subpage and _otherpage are children of _textpage).
        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository();

            // Redirect for root page.
            var rurlRoot = new RedirectUrl { ContentKey = _textpage.Key, Url = "root-redirect" };
            await repo.SaveAsync(rurlRoot, CancellationToken.None);

            // Redirect for subpage (child of textpage).
            var rurlSub = new RedirectUrl
            {
                ContentKey = _subpage.Key,
                Url = "subpage-redirect",
                CreateDateUtc = rurlRoot.CreateDateUtc.AddSeconds(1)
            };
            await repo.SaveAsync(rurlSub, CancellationToken.None);

            // Redirect for otherpage (child of textpage).
            var rurlOther = new RedirectUrl
            {
                ContentKey = _otherpage.Key,
                Url = "otherpage-redirect",
                CreateDateUtc = rurlRoot.CreateDateUtc.AddSeconds(2)
            };
            await repo.SaveAsync(rurlOther, CancellationToken.None);

            scope.Complete();
        }

        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository();

            // Get all URLs under _textpage (should include _subpage and _otherpage redirects).
            PagedModel<IRedirectUrl> result = await repo.GetAllUrlsAsync(_textpage.Id, 0, 100);
            var rurls = result.Items.ToArray();
            scope.Complete();

            // Should find redirects for descendants of _textpage.
            // Note: The query uses path LIKE '%,{rootContentId},%' so it finds descendants, not the root itself.
            Assert.That(rurls.Length, Is.GreaterThanOrEqualTo(2));
            Assert.That(rurls.Any(r => r.Url == "subpage-redirect"), Is.True);
            Assert.That(rurls.Any(r => r.Url == "otherpage-redirect"), Is.True);
        }
    }

    [Test]
    public async Task Can_Search_Urls()
    {
        var provider = NewScopeProvider;

        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository();

            var rurl1 = new RedirectUrl { ContentKey = _textpage.Key, Url = "/old-products/widget-123" };
            await repo.SaveAsync(rurl1, CancellationToken.None);

            var rurl2 = new RedirectUrl
            {
                ContentKey = _subpage.Key,
                Url = "/old-services/consulting",
                CreateDateUtc = rurl1.CreateDateUtc.AddSeconds(1)
            };
            await repo.SaveAsync(rurl2, CancellationToken.None);

            var rurl3 = new RedirectUrl
            {
                ContentKey = _otherpage.Key,
                Url = "/old-products/gadget-456",
                CreateDateUtc = rurl1.CreateDateUtc.AddSeconds(2)
            };
            await repo.SaveAsync(rurl3, CancellationToken.None);

            scope.Complete();
        }

        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository();

            // Search for URLs containing "products".
            PagedModel<IRedirectUrl> result = await repo.SearchUrlsAsync("products", 0, 100);
            var rurls = result.Items.ToArray();
            scope.Complete();

            Assert.AreEqual(2, result.Total);
            Assert.AreEqual(2, rurls.Length);
            Assert.That(rurls.All(r => r.Url.Contains("products")), Is.True);
            Assert.That(rurls.Any(r => r.Url == "/old-products/widget-123"), Is.True);
            Assert.That(rurls.Any(r => r.Url == "/old-products/gadget-456"), Is.True);
        }

        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository();

            // Search for URLs containing "consulting" - should only find 1.
            PagedModel<IRedirectUrl> result = await repo.SearchUrlsAsync("consulting", 0, 100);
            var rurls = result.Items.ToArray();
            scope.Complete();

            Assert.AreEqual(1, result.Total);
            Assert.AreEqual(1, rurls.Length);
            Assert.AreEqual("/old-services/consulting", rurls[0].Url);
        }
    }

    private IRedirectUrlRepository CreateRepository() =>
        new RedirectUrlRepository(GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>(), AppCaches, LoggerFactory.CreateLogger<RedirectUrlRepository>(), Mock.Of<IRepositoryCacheVersionService>(), Mock.Of<ICacheSyncService>());

    private IContent _textpage;
    private IContent _subpage;
    private IContent _otherpage;
    private IContent _trashed;

    public async Task CreateTestDataAsync()
    {
        var templateService = GetRequiredService<ITemplateService>();
        var template = TemplateBuilder.CreateTextPageTemplate();
        await templateService.CreateAsync(template, Constants.Security.SuperUserKey); // else, FK violation on contentType!

        var contentService = GetRequiredService<IContentService>();
        var contentTypeService = GetRequiredService<IContentTypeService>();

        // Create and Save ContentType "umbTextpage" -> (NodeDto.NodeIdSeed)
        var contentType =
            ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage", defaultTemplateId: template.Id);
        contentType.Key = Guid.NewGuid();
        contentTypeService.Save(contentType);

        // Create and Save Content "Homepage" based on "umbTextpage" -> (NodeDto.NodeIdSeed + 1)
        _textpage = ContentBuilder.CreateSimpleContent(contentType);
        _textpage.Key = Guid.NewGuid();
        contentService.Save(_textpage);

        // Create and Save Content "Text Page 1" based on "umbTextpage" -> (NodeDto.NodeIdSeed + 2)
        _subpage = ContentBuilder.CreateSimpleContent(contentType, "Text Page 1", _textpage.Id);
        _subpage.Key = Guid.NewGuid();
        contentService.Save(_subpage);

        // Create and Save Content "Text Page 1" based on "umbTextpage" -> (NodeDto.NodeIdSeed + 3)
        _otherpage = ContentBuilder.CreateSimpleContent(contentType, "Text Page 2", _textpage.Id);
        _otherpage.Key = Guid.NewGuid();
        contentService.Save(_otherpage);

        // Create and Save Content "Text Page Deleted" based on "umbTextpage" -> (NodeDto.NodeIdSeed + 4)
        _trashed = ContentBuilder.CreateSimpleContent(contentType, "Text Page Deleted", -20);
        _trashed.Key = Guid.NewGuid();
        ((Content)_trashed).Trashed = true;
        contentService.Save(_trashed);
    }
}
