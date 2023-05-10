using Examine;
using Examine.Lucene;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Examine;

public class DeliveryApiContentIndex : UmbracoContentIndexBase
{
    public DeliveryApiContentIndex(
        ILoggerFactory loggerFactory,
        string name,
        IOptionsMonitor<LuceneDirectoryIndexOptions> indexOptions,
        IHostingEnvironment hostingEnvironment,
        IRuntimeState runtimeState)
        : base(loggerFactory, name, indexOptions, hostingEnvironment, runtimeState)
    {
        PublishedValuesOnly = true;
        EnableDefaultEventHandler = true;
    }

    protected override void OnTransformingIndexValues(IndexingItemEventArgs e)
    {
        // UmbracoExamineIndex (base class down the hierarchy) performs some magic transformations here for paths and icons;
        // we don't want that for the Delivery API, so we'll have to override this method and simply do nothing.
    }
}
