using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

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

        await ElementContainerService.MoveToRecycleBinAsync(ElementContainers.Last().Key, Constants.Security.SuperUserKey);

        _fixtureIsInitialized = true;
    }
}
