using Examine;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Examine;

public class DeliveryApiContentIndexPopulator : IndexPopulator
{
    private readonly IContentService _contentService;
    private readonly IDeliveryApiContentIndexValueSetBuilder _deliveryContentIndexValueSetBuilder;

    public DeliveryApiContentIndexPopulator(IContentService contentService, IDeliveryApiContentIndexValueSetBuilder deliveryContentIndexValueSetBuilder)
    {
        _contentService = contentService;
        _deliveryContentIndexValueSetBuilder = deliveryContentIndexValueSetBuilder;
        RegisterIndex(Constants.UmbracoIndexes.DeliveryApiContentIndexName);
    }

    protected override void PopulateIndexes(IReadOnlyList<IIndex> indexes)
    {
        if (indexes.Any() is false)
        {
            return;
        }

        const int pageSize = 10000;
        var pageIndex = 0;

        IContent[] content;
        do
        {
            content = _contentService.GetPagedDescendants(Constants.System.Root, pageIndex, pageSize, out _).ToArray();

            ValueSet[] valueSets = _deliveryContentIndexValueSetBuilder.GetValueSets(content).ToArray();

            // ReSharper disable once PossibleMultipleEnumeration
            foreach (IIndex index in indexes)
            {
                index.IndexItems(valueSets);
            }

            pageIndex++;
        }
        while (content.Length == pageSize);
    }
}
