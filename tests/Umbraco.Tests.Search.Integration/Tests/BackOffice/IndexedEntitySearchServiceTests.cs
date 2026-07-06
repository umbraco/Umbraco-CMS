using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Tests.Search.Integration.Tests.BackOffice;

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

        IContent[] contentAtRoot = ContentService.GetRootContent().OrderBy(content => content.SortOrder).ToArray();
        ContentService.MoveToRecycleBin(contentAtRoot.Last());

        IMedia[] mediaAtRoot = MediaService.GetRootMedia().OrderBy(media => media.SortOrder).ToArray();
        MediaService.MoveToRecycleBin(mediaAtRoot.Last());

        _fixtureIsInitialized = true;
    }
}
