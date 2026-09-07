using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing.Search;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class DistributedContentIndexRefresherMediaTests : TestBase
{
    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private IMediaService MediaService => GetRequiredService<IMediaService>();

    private IDistributedContentIndexRefresher DistributedContentIndexRefresher => GetRequiredService<IDistributedContentIndexRefresher>();

    private Guid _mediaOneKey;
    private Guid _mediaTwoKey;

    [SetUp]
    public async Task SetupTest()
    {
        IMediaType mediaType = new MediaTypeBuilder()
            .WithAlias("invariant")
            .WithAllowAsRoot(true)
            .Build();
        await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);

        _mediaOneKey = Guid.NewGuid();
        IMedia mediaOne = new MediaBuilder()
            .WithKey(_mediaOneKey)
            .WithMediaType(mediaType)
            .WithName("Media one")
            .Build();
        MediaService.Save(mediaOne);

        _mediaTwoKey = Guid.NewGuid();
        IMedia mediaTwo = new MediaBuilder()
            .WithKey(_mediaTwoKey)
            .WithMediaType(mediaType)
            .WithName("Media two")
            .Build();
        MediaService.Save(mediaTwo);

        IndexerAndSearcher.Reset();
    }

    [Test]
    public void RefreshMedia_Single()
    {
        Assert.That(IndexerAndSearcher.Dump(IndexAliases.Media), Is.Empty);

        DistributedContentIndexRefresher.RefreshMedia([MediaOne()]);

        IReadOnlyList<TestIndexDocument> dump = IndexerAndSearcher.Dump(IndexAliases.Media);

        Assert.That(dump, Has.Count.EqualTo(1));
        Assert.That(dump[0].Id, Is.EqualTo(_mediaOneKey));
    }

    [Test]
    public void RefreshMedia_Multiple()
    {
        Assert.That(IndexerAndSearcher.Dump(IndexAliases.Media), Is.Empty);

        DistributedContentIndexRefresher.RefreshMedia([MediaOne(), MediaTwo()]);

        IReadOnlyList<TestIndexDocument> dump = IndexerAndSearcher.Dump(IndexAliases.Media);

        Assert.That(dump, Has.Count.EqualTo(2));
        Assert.That(dump.Select(d => d.Id), Is.EquivalentTo(new[] { _mediaOneKey, _mediaTwoKey }));
    }

    private IMedia MediaOne() => MediaService.GetById(_mediaOneKey) ?? throw new InvalidOperationException("Media one was not found");

    private IMedia MediaTwo() => MediaService.GetById(_mediaTwoKey) ?? throw new InvalidOperationException("Media two was not found");
}
