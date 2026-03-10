// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class RedirectUrlRepositoryTests : UmbracoIntegrationTest
{
    [SetUp]
    public void SetUp() => CreateTestData();

    [Test]
    public void Can_Save_And_Get()
    {
        var provider = ScopeProvider;

        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository(provider);
            var rurl = new RedirectUrl { ContentKey = _textpage.Key, Url = "blah" };
            repo.Save(rurl);
            scope.Complete();

            Assert.AreNotEqual(0, rurl.Id);
        }

        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository(provider);
            var rurl = repo.GetMostRecentUrl("blah");
            scope.Complete();

            Assert.IsNotNull(rurl);
            Assert.AreEqual(_textpage.Id, rurl.ContentId);
        }
    }

    [Test]
    public void Can_Save_And_Get_With_Culture()
    {
        var culture = "en";
        using (var scope = ScopeProvider.CreateScope())
        {
            var repo = CreateRepository(ScopeProvider);
            var rurl = new RedirectUrl { ContentKey = _textpage.Key, Url = "blah", Culture = culture };
            repo.Save(rurl);
            scope.Complete();

            Assert.AreNotEqual(0, rurl.Id);
        }

        using (var scope = ScopeProvider.CreateScope())
        {
            var repo = CreateRepository(ScopeProvider);
            var rurl = repo.GetMostRecentUrl("blah");
            scope.Complete();

            Assert.IsNotNull(rurl);
            Assert.AreEqual(_textpage.Id, rurl.ContentId);
            Assert.AreEqual(culture, rurl.Culture);
        }
    }

    [Test]
    public void Can_Save_And_Get_Most_Recent()
    {
        var provider = ScopeProvider;

        Assert.AreNotEqual(_textpage.Id, _otherpage.Id);

        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository(provider);
            var rurl = new RedirectUrl { ContentKey = _textpage.Key, Url = "blah" };
            repo.Save(rurl);
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
            repo.Save(rurl);
            scope.Complete();

            Assert.AreNotEqual(0, rurl.Id);
        }

        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository(provider);
            var rurl = repo.GetMostRecentUrl("blah");
            scope.Complete();

            Assert.IsNotNull(rurl);
            Assert.AreEqual(_otherpage.Id, rurl.ContentId);
        }
    }

    [Test]
    public void Can_Save_And_Get_Most_Recent_For_Culture()
    {
        var cultureA = "en";
        var cultureB = "de";
        Assert.AreNotEqual(_textpage.Id, _otherpage.Id);

        using (var scope = ScopeProvider.CreateScope())
        {
            var repo = CreateRepository(ScopeProvider);
            var rurl = new RedirectUrl { ContentKey = _textpage.Key, Url = "blah", Culture = cultureA };
            repo.Save(rurl);
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
            repo.Save(rurl);
            scope.Complete();

            Assert.AreNotEqual(0, rurl.Id);
        }

        using (var scope = ScopeProvider.CreateScope())
        {
            var repo = CreateRepository(ScopeProvider);
            var rurl = repo.GetMostRecentUrl("blah", cultureA);
            scope.Complete();

            Assert.IsNotNull(rurl);
            Assert.AreEqual(_textpage.Id, rurl.ContentId);
            Assert.AreEqual(cultureA, rurl.Culture);
        }
    }

    [Test]
    public void Can_Save_And_Get_By_Content()
    {
        var provider = ScopeProvider;

        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository(provider);
            var rurl = new RedirectUrl { ContentKey = _textpage.Key, Url = "blah" };
            repo.Save(rurl);
            scope.Complete();

            Assert.AreNotEqual(0, rurl.Id);

            // TODO: goes too fast and bam, errors, first is blah
            rurl = new RedirectUrl
            {
                ContentKey = _textpage.Key,
                Url = "durg",
                CreateDateUtc = rurl.CreateDateUtc.AddSeconds(1) // ensure time difference
            };
            repo.Save(rurl);
            scope.Complete();

            Assert.AreNotEqual(0, rurl.Id);
        }

        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository(provider);
            var rurls = repo.GetContentUrls(_textpage.Key).ToArray();
            scope.Complete();

            Assert.AreEqual(2, rurls.Length);
            Assert.AreEqual("durg", rurls[0].Url);
            Assert.AreEqual("blah", rurls[1].Url);
        }
    }

    [Test]
    public void Can_Save_And_Delete()
    {
        var provider = ScopeProvider;

        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository(provider);
            var rurl = new RedirectUrl { ContentKey = _textpage.Key, Url = "blah" };
            repo.Save(rurl);
            scope.Complete();

            Assert.AreNotEqual(0, rurl.Id);

            rurl = new RedirectUrl { ContentKey = _otherpage.Key, Url = "durg" };
            repo.Save(rurl);
            scope.Complete();

            Assert.AreNotEqual(0, rurl.Id);
        }

        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository(provider);
            repo.DeleteContentUrls(_textpage.Key);
            scope.Complete();

            var rurls = repo.GetContentUrls(_textpage.Key);

            Assert.AreEqual(0, rurls.Count());
        }
    }

    [Test]
    public void Can_Get_All_Urls_Filtered_By_Root_Content_Id()
    {
        var provider = ScopeProvider;

        // Create redirects for content at different levels of the hierarchy (_textpage is root,
        // _subpage and _otherpage are children of _textpage).
        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository(provider);

            // Redirect for root page.
            var rurlRoot = new RedirectUrl { ContentKey = _textpage.Key, Url = "root-redirect" };
            repo.Save(rurlRoot);

            // Redirect for subpage (child of textpage).
            var rurlSub = new RedirectUrl
            {
                ContentKey = _subpage.Key,
                Url = "subpage-redirect",
                CreateDateUtc = rurlRoot.CreateDateUtc.AddSeconds(1)
            };
            repo.Save(rurlSub);

            // Redirect for otherpage (child of textpage).
            var rurlOther = new RedirectUrl
            {
                ContentKey = _otherpage.Key,
                Url = "otherpage-redirect",
                CreateDateUtc = rurlRoot.CreateDateUtc.AddSeconds(2)
            };
            repo.Save(rurlOther);

            scope.Complete();
        }

        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository(provider);

            // Get all URLs under _textpage (should include _subpage and _otherpage redirects).
            var rurls = repo.GetAllUrls(_textpage.Id, 0, 100, out var total).ToArray();
            scope.Complete();

            // Should find redirects for descendants of _textpage.
            // Note: The query uses path LIKE '%,{rootContentId},%' so it finds descendants, not the root itself.
            Assert.That(rurls.Length, Is.GreaterThanOrEqualTo(2));
            Assert.That(rurls.Any(r => r.Url == "subpage-redirect"), Is.True);
            Assert.That(rurls.Any(r => r.Url == "otherpage-redirect"), Is.True);
        }
    }

    [Test]
    public void Can_Search_Urls()
    {
        var provider = ScopeProvider;

        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository(provider);

            var rurl1 = new RedirectUrl { ContentKey = _textpage.Key, Url = "/old-products/widget-123" };
            repo.Save(rurl1);

            var rurl2 = new RedirectUrl
            {
                ContentKey = _subpage.Key,
                Url = "/old-services/consulting",
                CreateDateUtc = rurl1.CreateDateUtc.AddSeconds(1)
            };
            repo.Save(rurl2);

            var rurl3 = new RedirectUrl
            {
                ContentKey = _otherpage.Key,
                Url = "/old-products/gadget-456",
                CreateDateUtc = rurl1.CreateDateUtc.AddSeconds(2)
            };
            repo.Save(rurl3);

            scope.Complete();
        }

        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository(provider);

            // Search for URLs containing "products".
            var rurls = repo.SearchUrls("products", 0, 100, out var total).ToArray();
            scope.Complete();

            Assert.AreEqual(2, total);
            Assert.AreEqual(2, rurls.Length);
            Assert.That(rurls.All(r => r.Url.Contains("products")), Is.True);
            Assert.That(rurls.Any(r => r.Url == "/old-products/widget-123"), Is.True);
            Assert.That(rurls.Any(r => r.Url == "/old-products/gadget-456"), Is.True);
        }

        using (var scope = provider.CreateScope())
        {
            var repo = CreateRepository(provider);

            // Search for URLs containing "consulting" - should only find 1.
            var rurls = repo.SearchUrls("consulting", 0, 100, out var total).ToArray();
            scope.Complete();

            Assert.AreEqual(1, total);
            Assert.AreEqual(1, rurls.Length);
            Assert.AreEqual("/old-services/consulting", rurls[0].Url);
        }
    }

    private IRedirectUrlRepository CreateRepository(IScopeProvider provider) =>
        new RedirectUrlRepository((IScopeAccessor)provider, AppCaches, LoggerFactory.CreateLogger<RedirectUrlRepository>(), Mock.Of<IRepositoryCacheVersionService>(), Mock.Of<ICacheSyncService>());

    private IContent _textpage;
    private IContent _subpage;
    private IContent _otherpage;
    private IContent _trashed;

    public void CreateTestData()
    {
        var fileService = GetRequiredService<IFileService>();
        var template = TemplateBuilder.CreateTextPageTemplate();
        fileService.SaveTemplate(template); // else, FK violation on contentType!

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
