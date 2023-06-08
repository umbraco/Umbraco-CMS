using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Examine;

internal sealed class DeliveryApiContentIndexHelper : IDeliveryApiContentIndexHelper
{
    private readonly IContentService _contentService;
    private readonly IUmbracoDatabaseFactory _umbracoDatabaseFactory;
    private DeliveryApiSettings _deliveryApiSettings;

    public DeliveryApiContentIndexHelper(
        IContentService contentService,
        IUmbracoDatabaseFactory umbracoDatabaseFactory,
        IOptionsMonitor<DeliveryApiSettings> deliveryApiSettings)
    {
        _contentService = contentService;
        _umbracoDatabaseFactory = umbracoDatabaseFactory;
        _deliveryApiSettings = deliveryApiSettings.CurrentValue;
        deliveryApiSettings.OnChange(settings => _deliveryApiSettings = settings);
    }

    public void EnumerateApplicableDescendantsForContentIndex(int rootContentId, Action<IContent[]> actionToPerform)
    {
        const int pageSize = 10000;
        var pageIndex = 0;

        IContent[] descendants;
        IQuery<IContent> query = _umbracoDatabaseFactory.SqlContext.Query<IContent>().Where(content => content.Trashed == false);
        do
        {
            descendants = _contentService
                .GetPagedDescendants(rootContentId, pageIndex, pageSize, out _, query, Ordering.By("Path"))
                .Where(descendant => _deliveryApiSettings.IsAllowedContentType(descendant.ContentType.Alias))
                .ToArray();

            actionToPerform(descendants.ToArray());

            pageIndex++;
        }
        while (descendants.Length == pageSize);
    }
}
