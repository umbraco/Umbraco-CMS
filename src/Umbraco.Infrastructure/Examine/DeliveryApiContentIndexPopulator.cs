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
        foreach (IIndex index in indexes)
        {
            IEnumerable<IContent> rootNodes = _contentService.GetRootContent();

            index.IndexItems(_deliveryContentIndexValueSetBuilder.GetValueSets(rootNodes.ToArray()));

            foreach (IContent root in rootNodes)
            {
                IEnumerable<ValueSet> valueSets = _deliveryContentIndexValueSetBuilder.GetValueSets(
                    _contentService.GetPagedDescendants(root.Id, 0, int.MaxValue, out _).ToArray());

                index.IndexItems(valueSets);
            }
        }
    }
}
