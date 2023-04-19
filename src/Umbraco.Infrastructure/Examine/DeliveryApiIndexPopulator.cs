using Examine;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Examine;

public class DeliveryApiIndexPopulator : IndexPopulator
{
    private readonly IContentService _contentService;
    private readonly IDeliveryApiValueSetBuilder _deliveryValueSetBuilder;

    public DeliveryApiIndexPopulator(IContentService contentService, IDeliveryApiValueSetBuilder deliveryValueSetBuilder)
    {
        _contentService = contentService;
        _deliveryValueSetBuilder = deliveryValueSetBuilder;
        RegisterIndex(Constants.UmbracoIndexes.DeliveryApiIndexName);
    }

    protected override void PopulateIndexes(IReadOnlyList<IIndex> indexes)
    {
        foreach (IIndex index in indexes)
        {
            IEnumerable<IContent> rootNodes = _contentService.GetRootContent();

            index.IndexItems(_deliveryValueSetBuilder.GetValueSets(rootNodes.ToArray()));

            foreach (IContent root in rootNodes)
            {
                IEnumerable<ValueSet> valueSets = _deliveryValueSetBuilder.GetValueSets(
                    _contentService.GetPagedDescendants(root.Id, 0, int.MaxValue, out _).ToArray());

                index.IndexItems(valueSets);
            }
        }
    }
}
