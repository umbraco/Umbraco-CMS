using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class PublishedContentFactoryTests : UmbracoIntegrationTestWithContent
{
    private IPublishedContentFactory PublishedContentFactory => GetRequiredService<IPublishedContentFactory>();

    private IPublishedValueFallback PublishedValueFallback => GetRequiredService<IPublishedValueFallback>();

    private IMediaService MediaService => GetRequiredService<IMediaService>();

    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private IMemberService MemberService => GetRequiredService<IMemberService>();

    private IMemberTypeService MemberTypeService => GetRequiredService<IMemberTypeService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        var requestCache = new DictionaryAppCache();
        var appCaches = new AppCaches(
            NoAppCache.Instance,
            requestCache,
            new IsolatedCaches(type => NoAppCache.Instance));
        builder.Services.AddUnique(appCaches);
    }

    [Test]
    public void Can_Create_Published_Content_For_Document()
    {
        var contentCacheNode = new ContentCacheNode
        {
            Id = Textpage.Id,
            Key = Textpage.Key,
            ContentTypeId = Textpage.ContentType.Id,
            CreateDate = Textpage.CreateDate,
            CreatorId = Textpage.CreatorId,
            SortOrder = Textpage.SortOrder,
            Data = new ContentData(
                Textpage.Name,
                "text-page",
                Textpage.VersionId,
                Textpage.UpdateDate,
                Textpage.WriterId,
                Textpage.TemplateId,
                true,
                new Dictionary<string, PropertyData[]>
                {
                    {
                        "title", new[]
                        {
                            new PropertyData
                            {
                                Value = "Test title",
                                Culture = string.Empty,
                                Segment = string.Empty,
                            },
                        }
                    },
                },
                null),
        };
        var result = PublishedContentFactory.ToIPublishedContent(contentCacheNode, false);
        Assert.IsNotNull(result);
        Assert.AreEqual(Textpage.Id, result.Id);
        Assert.AreEqual(Textpage.Name, result.Name);
        Assert.AreEqual("Test title", result.Properties.Single(x => x.Alias == "title").Value<string>(PublishedValueFallback));

        // Verify that requesting the same content again returns the same instance (from request cache).
        var result2 = PublishedContentFactory.ToIPublishedContent(contentCacheNode, false);
        Assert.AreSame(result, result2);
    }

    [Test]
    public async Task Can_Create_Published_Content_For_Media()
    {
        var mediaType = new MediaTypeBuilder().Build();
        mediaType.AllowedAsRoot = true;
        await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);

        var media = new MediaBuilder()
            .WithMediaType(mediaType)
            .WithName("Media 1")
            .Build();
        MediaService.Save(media);

        var contentCacheNode = new ContentCacheNode
        {
            Id = media.Id,
            Key = media.Key,
            ContentTypeId = media.ContentType.Id,
            Data = new ContentData(
                media.Name,
                null,
                0,
                media.UpdateDate,
                media.WriterId,
                null,
                false,
                new Dictionary<string, PropertyData[]>(),
                null),
        };
        var result = PublishedContentFactory.ToIPublishedMedia(contentCacheNode);
        Assert.IsNotNull(result);
        Assert.AreEqual(media.Id, result.Id);
        Assert.AreEqual(media.Name, result.Name);

        // Verify that requesting the same content again returns the same instance (from request cache).
        var result2 = PublishedContentFactory.ToIPublishedMedia(contentCacheNode);
        Assert.AreSame(result, result2);
    }

    [Test]
    public async Task Can_Create_Published_Member_For_Member()
    {
        var memberType = new MemberTypeBuilder().Build();
        await MemberTypeService.CreateAsync(memberType, Constants.Security.SuperUserKey);

        var member = new MemberBuilder()
            .WithMemberType(memberType)
            .WithName("Member 1")
            .Build();
        MemberService.Save(member);

        var result = PublishedContentFactory.ToPublishedMember(member);
        Assert.IsNotNull(result);
        Assert.AreEqual(member.Id, result.Id);
        Assert.AreEqual(member.Name, result.Name);

        // Verify that requesting the same content again returns the same instance (from request cache).
        var result2 = PublishedContentFactory.ToPublishedMember(member);
        Assert.AreSame(result, result2);
    }
}
