using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
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

    private IndexingSettings _indexingSettings;

    [Obsolete("Please use the non-obsolete constructor. Scheduled for removal in Umbraco 19.")]
    public DeliveryApiContentIndexHelper(
        IContentService contentService,
        IUmbracoDatabaseFactory umbracoDatabaseFactory,
        IOptionsMonitor<DeliveryApiSettings> deliveryApiSettings)
        : this(contentService,  umbracoDatabaseFactory, deliveryApiSettings, StaticServiceProvider.Instance.GetRequiredService<IOptionsMonitor<IndexingSettings>>())
    {
    }

    public DeliveryApiContentIndexHelper(
        IContentService contentService,
        IUmbracoDatabaseFactory umbracoDatabaseFactory,
        IOptionsMonitor<DeliveryApiSettings> deliveryApiSettings,
        IOptionsMonitor<IndexingSettings> indexingSettings)
    {
        _contentService = contentService;
        _umbracoDatabaseFactory = umbracoDatabaseFactory;
        _deliveryApiSettings = deliveryApiSettings.CurrentValue;
        _indexingSettings = indexingSettings.CurrentValue;
        deliveryApiSettings.OnChange(settings => _deliveryApiSettings = settings);
        indexingSettings.OnChange(settings => _indexingSettings = settings);
    }

    public void EnumerateApplicableDescendantsForContentIndex(int rootContentId, Action<IContent[]> actionToPerform)
        => EnumerateApplicableDescendantsForContentIndex(rootContentId, actionToPerform, _indexingSettings.BatchSize);

    internal void EnumerateApplicableDescendantsForContentIndex(int rootContentId, Action<IContent[]> actionToPerform, int pageSize)
    {
        var itemIndex = 0;
        long total;

        IQuery<IContent> query = _umbracoDatabaseFactory.SqlContext.Query<IContent>().Where(content => content.Trashed == false);

        IContent[] descendants;
        do
        {
            descendants = _contentService
                .GetPagedDescendants(rootContentId, itemIndex / pageSize, pageSize, out total, query, Ordering.By("Path"))
                .Where(descendant => _deliveryApiSettings.IsAllowedContentType(descendant.ContentType.Alias))
                .ToArray();

            actionToPerform(descendants);

            itemIndex += pageSize;
        }
        while (descendants.Length > 0 && itemIndex < total);
    }
}
