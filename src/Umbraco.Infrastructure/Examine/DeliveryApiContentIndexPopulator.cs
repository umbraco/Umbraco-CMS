using Examine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Infrastructure.Examine;

internal sealed class DeliveryApiContentIndexPopulator : IndexPopulator
{
    private readonly IDeliveryApiContentIndexValueSetBuilder _deliveryContentIndexValueSetBuilder;
    private readonly IDeliveryApiContentIndexHelper _deliveryApiContentIndexHelper;
    private readonly ILogger<DeliveryApiContentIndexPopulator> _logger;
    private DeliveryApiSettings _deliveryApiSettings;

    public DeliveryApiContentIndexPopulator(
        IDeliveryApiContentIndexValueSetBuilder deliveryContentIndexValueSetBuilder,
        IDeliveryApiContentIndexHelper deliveryApiContentIndexHelper,
        ILogger<DeliveryApiContentIndexPopulator> logger,
        IOptionsMonitor<DeliveryApiSettings> deliveryApiSettings)
    {
        _deliveryContentIndexValueSetBuilder = deliveryContentIndexValueSetBuilder;
        _deliveryApiContentIndexHelper = deliveryApiContentIndexHelper;
        _logger = logger;
        _deliveryApiSettings = deliveryApiSettings.CurrentValue;
        deliveryApiSettings.OnChange(settings => _deliveryApiSettings = settings);
        RegisterIndex(Constants.UmbracoIndexes.DeliveryApiContentIndexName);
    }

    protected override void PopulateIndexes(IReadOnlyList<IIndex> indexes)
    {
        if (indexes.Any() is false)
        {
            return;
        }

        if (_deliveryApiSettings.Enabled is false)
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

    public override bool IsRegistered(IIndex index)
    {
        if (_deliveryApiSettings.Enabled)
        {
            return base.IsRegistered(index);
        }

        // IsRegistered() is invoked for all indexes; only log a message when it's invoked for the Delivery API content index
        if (index.Name is Constants.UmbracoIndexes.DeliveryApiContentIndexName)
        {
            // IsRegistered() is currently invoked only when Umbraco starts and when loading the Examine dashboard,
            // so we won't be flooding the logs with info messages here
            _logger.LogInformation("The Delivery API is not enabled, no indexing will performed for the Delivery API content index.");
        }

        return false;
    }
}
