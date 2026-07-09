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

    /// <summary>
    /// Initializes a new instance of the <see cref="DeliveryApiContentIndexHelper"/> class.
    /// </summary>
    /// <param name="contentService">Service used to manage and retrieve Umbraco content items.</param>
    /// <param name="umbracoDatabaseFactory">Factory for creating Umbraco database connections.</param>
    /// <param name="deliveryApiSettings">Monitors configuration settings for the Delivery API.</param>
    [Obsolete("Please use the non-obsolete constructor. Scheduled for removal in Umbraco 19.")]
    public DeliveryApiContentIndexHelper(
        IContentService contentService,
        IUmbracoDatabaseFactory umbracoDatabaseFactory,
        IOptionsMonitor<DeliveryApiSettings> deliveryApiSettings)
        : this(contentService,  umbracoDatabaseFactory, deliveryApiSettings, StaticServiceProvider.Instance.GetRequiredService<IOptionsMonitor<IndexingSettings>>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DeliveryApiContentIndexHelper"/> class.
    /// </summary>
    /// <param name="contentService">An <see cref="IContentService"/> used to access and manage content data.</param>
    /// <param name="umbracoDatabaseFactory">An <see cref="IUmbracoDatabaseFactory"/> for creating database connections.</param>
    /// <param name="deliveryApiSettings">An <see cref="IOptionsMonitor{DeliveryApiSettings}"/> providing access to Delivery API configuration settings.</param>
    /// <param name="indexingSettings">An <see cref="IOptionsMonitor{IndexingSettings}"/> providing access to indexing configuration settings.</param>
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

    /// <summary>
    /// Enumerates applicable descendant content items for content indexing starting from the specified root content ID.
    /// </summary>
    /// <param name="rootContentId">The ID of the root content item to start enumeration from.</param>
    /// <param name="actionToPerform">The action to perform on each batch of descendant content items.</param>
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
