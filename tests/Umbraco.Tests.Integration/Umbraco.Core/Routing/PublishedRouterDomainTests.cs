using Castle.DynamicProxy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Scoping;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Routing;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Mock)]
internal sealed class PublishedRouterDomainTests : UmbracoIntegrationTest
{
    private const string DefaultCulture = "en-US";
    private const string DomainCulture = "da-DK";
    private const string DomainHost = "da.example.com";

    private readonly TestVariationContextAccessor _variationContextAccessor = new();

    private int _contentReads;

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IPublishedRouter PublishedRouter => GetRequiredService<IPublishedRouter>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.Services.AddUnique<IVariationContextAccessor>(_variationContextAccessor);
        builder.AddUmbracoHybridCache();
        builder.Services.AddUnique<IDatabaseCacheRepository>(sp =>
            (IDatabaseCacheRepository)new ProxyGenerator().CreateInterfaceProxyWithTarget(
                typeof(IDatabaseCacheRepository),
                ActivatorUtilities.CreateInstance<DatabaseCacheRepository>(sp),
                new ContentReadCounter(() => Interlocked.Increment(ref _contentReads))));

        builder.Services.AddUnique<IServerMessenger, ScopedRepositoryTests.LocalServerMessenger>();
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.AddNotificationHandler<DomainSavedNotification, DomainSavedDistributedCacheNotificationHandler>();
    }

    [SetUp]
    public async Task SetUpTest()
    {
        await GetRequiredService<ILanguageService>().CreateAsync(new Language(DomainCulture, "Danish"), Constants.Security.SuperUserKey);

        GetRequiredService<IHttpContextAccessor>().HttpContext = new DefaultHttpContext
        {
            Request =
            {
                Scheme = "https",
                Host = new HostString(DomainHost),
                Path = "/",
                QueryString = new QueryString(string.Empty),
            },
            RequestServices = Services,
        };
    }

    [Test]
    public async Task Can_Resolve_Domain_Culture_For_Published_Invariant_Content_In_Non_Default_Language()
    {
        IContent root = await CreateRootAsync(ContentVariation.Nothing);
        ContentService.Publish(root, ["*"]);
        await AssignDomainAsync(root);

        Assert.IsTrue(UpdateVariationContext());
        Assert.AreEqual(DomainCulture, _variationContextAccessor.VariationContext?.Culture);
    }

    [Test]
    public async Task Can_Resolve_Domain_Culture_For_Variant_Content_Published_In_The_Domain_Culture()
    {
        IContent root = await CreateRootAsync(ContentVariation.Culture);
        ContentService.Publish(root, ["*"]);
        await AssignDomainAsync(root);

        Assert.IsTrue(UpdateVariationContext());
        Assert.AreEqual(DomainCulture, _variationContextAccessor.VariationContext?.Culture);
    }

    [Test]
    public async Task Cannot_Resolve_Domain_For_Variant_Content_Not_Published_In_The_Domain_Culture()
    {
        IContent root = await CreateRootAsync(ContentVariation.Culture);
        ContentService.Publish(root, [DefaultCulture]);
        await AssignDomainAsync(root);

        Assert.IsFalse(UpdateVariationContext());
    }

    [Test]
    public async Task Cannot_Resolve_Domain_For_Unpublished_Content()
    {
        IContent root = await CreateRootAsync(ContentVariation.Nothing);
        await AssignDomainAsync(root);

        Assert.IsFalse(UpdateVariationContext());
    }

    [Test]
    public async Task Cannot_Resolve_Domain_For_Content_Below_An_Unpublished_Ancestor()
    {
        IContent root = await CreateRootAsync(ContentVariation.Nothing);
        ContentService.Publish(root, ["*"]);

        IContent child = CreatePublishedChild(root);
        await AssignDomainAsync(child);

        Assert.IsTrue(UpdateVariationContext(), "The domain should resolve while the whole path is published.");

        ContentService.Unpublish(root);

        Assert.IsFalse(UpdateVariationContext());
    }

    [Test]
    public async Task Does_Not_Read_Content_When_Resolving_Domains_For_Content_Below_An_Unpublished_Ancestor()
    {
        IContent root = await CreateRootAsync(ContentVariation.Nothing);
        ContentService.Publish(root, ["*"]);
        IContent child = CreatePublishedChild(root);
        await AssignDomainAsync(child);
        ContentService.Unpublish(root);

        Interlocked.Exchange(ref _contentReads, 0);
        for (var i = 0; i < 10; i++)
        {
            Assert.IsFalse(UpdateVariationContext());
        }

        Assert.AreEqual(0, _contentReads, "Resolving a domain must not read content from the database on every request.");
    }

    [Test]
    public async Task Cannot_Resolve_Domain_For_Unpublished_Content_That_Has_Published_Children()
    {
        IContent root = await CreateRootAsync(ContentVariation.Nothing);
        ContentService.Publish(root, ["*"]);
        IContent child = CreatePublishedChild(root);
        await AssignDomainAsync(root);
        ContentService.Unpublish(root);

        Assert.IsTrue(ContentService.GetById(child.Key)!.Published, "The child stays published when its parent is unpublished.");

        Interlocked.Exchange(ref _contentReads, 0);
        for (var i = 0; i < 10; i++)
        {
            Assert.IsFalse(UpdateVariationContext());
        }

        Assert.AreEqual(0, _contentReads, "Resolving a domain must not read content from the database.");
    }

    [Test]
    public async Task Can_Resolve_Domain_Again_After_Unpublished_Content_Is_Republished()
    {
        IContent root = await CreateRootAsync(ContentVariation.Nothing);
        ContentService.Publish(root, ["*"]);
        await AssignDomainAsync(root);

        Assert.IsTrue(UpdateVariationContext());

        ContentService.Unpublish(root);
        Assert.IsFalse(UpdateVariationContext());

        ContentService.Publish(root, ["*"]);
        Assert.IsTrue(UpdateVariationContext());
        Assert.AreEqual(DomainCulture, _variationContextAccessor.VariationContext?.Culture);
    }

    private bool UpdateVariationContext()
    {
        GetRequiredService<IUmbracoContextAccessor>().Clear();
        GetRequiredService<IUmbracoContextFactory>().EnsureUmbracoContext();

        _variationContextAccessor.VariationContext = new VariationContext(DefaultCulture);
        return PublishedRouter.UpdateVariationContext(new Uri($"https://{DomainHost}/"));
    }

    private async Task<IContent> CreateRootAsync(ContentVariation variation)
    {
        IContentType contentType = new ContentTypeBuilder()
            .WithAlias($"routerDomain{variation}")
            .WithContentVariation(variation)
            .Build();
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        contentType.AllowedContentTypes = [new() { Alias = contentType.Alias, Key = contentType.Key }];
        await ContentTypeService.UpdateAsync(contentType, Constants.Security.SuperUserKey);

        ContentBuilder builder = new ContentBuilder().WithContentType(contentType);
        builder = variation == ContentVariation.Culture
            ? builder.WithCultureName(DefaultCulture, "Root en-US").WithCultureName(DomainCulture, "Root da-DK")
            : builder.WithName("Root");

        IContent root = builder.Build();
        ContentService.Save(root);
        return root;
    }

    private IContent CreatePublishedChild(IContent root)
    {
        IContent child = new ContentBuilder()
            .WithContentType(ContentTypeService.Get(root.ContentType.Key)!)
            .WithParent(root)
            .WithName("Child")
            .Build();
        ContentService.Save(child);
        ContentService.Publish(child, ["*"]);
        return child;
    }

    private async Task AssignDomainAsync(IContent content)
    {
        var result = await GetRequiredService<IDomainService>().UpdateDomainsAsync(
            content.Key,
            new DomainsUpdateModel { Domains = [new DomainModel { DomainName = DomainHost, IsoCode = DomainCulture }] });
        Assert.IsTrue(result.Success);
    }

    private sealed class ContentReadCounter : IInterceptor
    {
        private readonly Action _onRead;

        public ContentReadCounter(Action onRead) => _onRead = onRead;

        public void Intercept(IInvocation invocation)
        {
            if (invocation.Method.Name.StartsWith("GetContentSource", StringComparison.Ordinal))
            {
                _onRead();
            }

            invocation.Proceed();
        }
    }
}
