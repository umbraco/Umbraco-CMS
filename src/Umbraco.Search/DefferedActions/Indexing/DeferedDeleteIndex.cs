using System.Globalization;
using Examine;
using Umbraco.Extensions;
using Umbraco.Search.Configuration;
using Umbraco.Search.Examine;
using Umbraco.Search.NotificationHandlers;

namespace Umbraco.Search.DefferedActions.Indexing;

internal class DeferedDeleteIndex : IDeferredAction
{
    private readonly ISearchProvider _provider;
    private readonly IUmbracoIndexesConfiguration _configuration;
    private readonly int _id;
    private readonly IReadOnlyCollection<int>? _ids;
    private readonly bool _keepIfUnpublished;

    public DeferedDeleteIndex(ISearchProvider provider,
        IUmbracoIndexesConfiguration configuration, int id,
        bool keepIfUnpublished)
    {
        _provider = provider;
        _configuration = configuration;
        _id = id;
        _keepIfUnpublished = keepIfUnpublished;
    }

    public DeferedDeleteIndex(ISearchProvider provider,
        IUmbracoIndexesConfiguration configuration,
        IReadOnlyCollection<int> ids, bool keepIfUnpublished)
    {
        _provider = provider;

        _configuration = configuration;
        _ids = ids;
        _keepIfUnpublished = keepIfUnpublished;
    }

    public void Execute()
    {
        if (_ids is null)
        {
            Execute(_provider, _configuration, _id, _keepIfUnpublished);
        }
        else
        {
            Execute(_provider, _configuration, _ids, _keepIfUnpublished);
        }
    }

    public static void Execute(ISearchProvider searchProvider,
        IUmbracoIndexesConfiguration umbracoIndexesConfiguration, int id,
        bool keepIfUnpublished)
    {
        foreach (string indexName in searchProvider.GetAllIndexes())
        {
            var configuration = umbracoIndexesConfiguration.Configuration(indexName);
            if (!configuration.EnableDefaultEventHandler || keepIfUnpublished || !configuration.PublishedValuesOnly)
            {
                continue;
            }

            var index = searchProvider.GetIndex(indexName);
            if (index == null)
            {
                continue;
            }

            index.RemoveFromIndex(id.ToString(CultureInfo.InvariantCulture).AsEnumerableOfOne());
        }
    }

    public static void Execute(ISearchProvider searchProvider,
        IUmbracoIndexesConfiguration umbracoIndexesConfiguration,
        IReadOnlyCollection<int> ids, bool keepIfUnpublished)
    {
        foreach (string indexName in searchProvider.GetAllIndexes())
        {
            IUmbracoIndexConfiguration configuration = umbracoIndexesConfiguration.Configuration(indexName);
            if (!configuration.EnableDefaultEventHandler || keepIfUnpublished || !configuration.PublishedValuesOnly)
            {
                continue;
            }

            IUmbracoIndex? index = searchProvider.GetIndex(indexName);
            if (index == null)
            {
                continue;
            }

            index.RemoveFromIndex(ids.Select(x => x.ToString(CultureInfo.InvariantCulture)));
        }
    }
}
