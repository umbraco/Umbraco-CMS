using Examine;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Examine;

internal sealed class DeliveryApiContentIndexPopulator : IndexPopulator
{
    private readonly IDeliveryApiContentIndexValueSetBuilder _deliveryContentIndexValueSetBuilder;
    private readonly IDeliveryApiContentIndexHelper _deliveryApiContentIndexHelper;

    public DeliveryApiContentIndexPopulator(
        IDeliveryApiContentIndexValueSetBuilder deliveryContentIndexValueSetBuilder,
        IDeliveryApiContentIndexHelper deliveryApiContentIndexHelper)
    {
        _deliveryContentIndexValueSetBuilder = deliveryContentIndexValueSetBuilder;
        _deliveryApiContentIndexHelper = deliveryApiContentIndexHelper;
        RegisterIndex(Constants.UmbracoIndexes.DeliveryApiContentIndexName);
    }

    protected override void PopulateIndexes(IReadOnlyList<IIndex> indexes)
    {
        if (indexes.Any() is false)
        {
            return;
        }

        _deliveryApiContentIndexHelper.EnumerateApplicableDescendantsForContentIndex(
            Constants.System.Root,
            descendants =>
            {
                ValueSet[] valueSets = _deliveryContentIndexValueSetBuilder.GetValueSets(descendants).ToArray();
                foreach (IIndex index in indexes)
                {
                    index.IndexItems(valueSets);
                }
            });
    }
}
