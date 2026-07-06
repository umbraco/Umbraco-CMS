using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;

namespace Umbraco.Tests.Search.Integration.Tests;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
public class DistributedContentIndexRebuilderTests : TestBase
{
    private bool _fixtureIsInitialized;

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private IMediaService MediaService => GetRequiredService<IMediaService>();

    private IMemberTypeService MemberTypeService => GetRequiredService<IMemberTypeService>();

    private IMemberService MemberService => GetRequiredService<IMemberService>();

    private IDistributedContentIndexRebuilder DistributedContentIndexRebuilder => GetRequiredService<IDistributedContentIndexRebuilder>();

    [SetUp]
    public async Task SetupTest()
    {
        // unfortunately, OneTimeSetUp won't work due to dependencies being setup in Umbraco test base classes,
        // so this is a workaround :)
        if (_fixtureIsInitialized)
        {
            return;
        }

        IContentType contentType = new ContentTypeBuilder()
            .WithAlias("theContent")
            .WithAllowAsRoot(true)
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        for (var i = 0; i < 5; i++)
        {
            IContent content = new ContentBuilder()
                .WithContentType(contentType)
                .WithName($"Content {i}")
                .Build();
            ContentService.Save(content);
            ContentService.Publish(content, ["*"]);
        }

        IMediaType mediaType = MediaTypeService.Get("Folder")
                               ?? throw new InvalidOperationException("Could not find the Folder media type");

        for (var i = 0; i < 5; i++)
        {
            IMedia media = new MediaBuilder()
                .WithMediaType(mediaType)
                .WithName($"Media {i}")
                .Build();
            MediaService.Save(media);
        }

        IMemberType memberType = new MemberTypeBuilder()
            .WithAlias("theMember")
            .Build();

        await MemberTypeService.CreateAsync(memberType, Constants.Security.SuperUserKey);

        for (var i = 0; i < 5; i++)
        {
            IMember member = new MemberBuilder()
                .WithMemberType(memberType)
                .WithName($"Member {i}")
                .Build();
            MemberService.Save(member);
        }

        IndexerAndSearcher.Reset();

        _fixtureIsInitialized = true;
    }

    [Test]
    public void RebuildDraftContent()
    {
        Assert.That(IndexerAndSearcher.Dump(IndexAliases.DraftContent), Is.Empty);

        DistributedContentIndexRebuilder.Rebuild(IndexAliases.DraftContent);

        Assert.That(IndexerAndSearcher.Dump(IndexAliases.DraftContent).Count, Is.EqualTo(5));
    }

    [Test]
    public void RebuildPublishedContent()
    {
        Assert.That(IndexerAndSearcher.Dump(IndexAliases.PublishedContent), Is.Empty);

        DistributedContentIndexRebuilder.Rebuild(IndexAliases.PublishedContent);

        Assert.That(IndexerAndSearcher.Dump(IndexAliases.PublishedContent).Count, Is.EqualTo(5));
    }

    [Test]
    public void RebuildMedia()
    {
        Assert.That(IndexerAndSearcher.Dump(IndexAliases.Media), Is.Empty);

        DistributedContentIndexRebuilder.Rebuild(IndexAliases.Media);

        Assert.That(IndexerAndSearcher.Dump(IndexAliases.Media).Count, Is.EqualTo(5));
    }

    [Test]
    public void RebuildMember()
    {
        Assert.That(IndexerAndSearcher.Dump(IndexAliases.Member), Is.Empty);

        DistributedContentIndexRebuilder.Rebuild(IndexAliases.Member);

        Assert.That(IndexerAndSearcher.Dump(IndexAliases.Member).Count, Is.EqualTo(5));
    }
}
