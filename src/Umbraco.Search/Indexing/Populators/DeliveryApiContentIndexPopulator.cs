using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Search.Services;

namespace Umbraco.Search.Indexing.Populators;

internal sealed class DeliveryApiContentIndexPopulator : IndexPopulator
{
   private readonly IDeliveryApiContentIndexHelper _deliveryApiContentIndexHelper;
    private readonly ILogger<DeliveryApiContentIndexPopulator> _logger;
    private DeliveryApiSettings _deliveryApiSettings;
    private readonly ISearchProvider _provider;
    public DeliveryApiContentIndexPopulator(
        IDeliveryApiContentIndexHelper deliveryApiContentIndexHelper,
        ILogger<DeliveryApiContentIndexPopulator> logger,
        IOptionsMonitor<DeliveryApiSettings> deliveryApiSettings, ISearchProvider provider)
    {
        _deliveryApiContentIndexHelper = deliveryApiContentIndexHelper;
        _logger = logger;
        _provider = provider;
        _deliveryApiSettings = deliveryApiSettings.CurrentValue;
        deliveryApiSettings.OnChange(settings => _deliveryApiSettings = settings);
        RegisterIndex(Constants.UmbracoIndexes.DeliveryApiContentIndexName);
    }

    protected override void PopulateIndexes(IReadOnlyList<string> indexes)
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
                // ReSharper disable once PossibleMultipleEnumeration
                foreach (string index in indexes)
                {
                    _provider.GetIndex<IContent>(index)?.IndexItems(descendants);
                }


            });
    }

    public override bool IsRegistered(string index)
    {
        if (_deliveryApiSettings.Enabled)
        {
            return base.IsRegistered(index);
        }

        // IsRegistered() is invoked for all indexes; only log a message when it's invoked for the Delivery API content index
        if (index is Constants.UmbracoIndexes.DeliveryApiContentIndexName)
        {
            // IsRegistered() is currently invoked only when Umbraco starts and when loading the Examine dashboard,
            // so we won't be flooding the logs with info messages here
            _logger.LogInformation("The Delivery API is not enabled, no indexing will performed for the Delivery API content index.");
        }

        return false;
    }
}
