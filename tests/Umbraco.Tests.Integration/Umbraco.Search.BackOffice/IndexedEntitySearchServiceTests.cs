using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.BackOffice;

public partial class IndexedEntitySearchServiceTests : BackOfficeTestBase
{
    private bool _fixtureIsInitialized;

    private IIndexedEntitySearchService IndexedEntitySearchService => GetRequiredService<IIndexedEntitySearchService>();

    public override async Task SetupTest()
    {
        await base.SetupTest();

        if (_fixtureIsInitialized)
        {
            return;
        }

        IContent[] contentAtRoot = (await ContentService.GetRootContentAsync(CancellationToken.None)).OrderBy(content => content.SortOrder).ToArray();
        await ContentService.MoveToRecycleBinAsync(contentAtRoot.Last(), Constants.Security.SuperUserKey, CancellationToken.None);

        IMedia[] mediaAtRoot = MediaService.GetRootMedia().OrderBy(media => media.SortOrder).ToArray();
        MediaService.MoveToRecycleBin(mediaAtRoot.Last());

        _fixtureIsInitialized = true;
    }
}
