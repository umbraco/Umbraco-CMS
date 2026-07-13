using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.BackOffice;

public partial class ContentSearchServiceTests : BackOfficeTestBase
{
    private IContentSearchService ContentSearchService => GetRequiredService<IContentSearchService>();

    private IMediaSearchService MediaSearchService => GetRequiredService<IMediaSearchService>();
}
