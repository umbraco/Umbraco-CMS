using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for index creator settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigIndexing)]
public class IndexingSettings
{
    private const bool StaticExplicitlyIndexEachNestedProperty = false;
    private const bool StaticIndexExternalBlockElements = false;
    private const int StaticBatchSize = 10000;

    /// <summary>
    /// Gets or sets a value for whether each nested property should have it's own indexed value. Requires a rebuild of indexes when changed.
    /// </summary>
    [DefaultValue(StaticExplicitlyIndexEachNestedProperty)]
    public bool ExplicitlyIndexEachNestedProperty { get; set; } = StaticExplicitlyIndexEachNestedProperty;

    /// <summary>
    /// Gets or sets a value indicating whether the content of external elements referenced by block editors is flattened into the index entry of referencing documents. Requires a rebuild of indexes when changed.
    /// </summary>
    [DefaultValue(StaticIndexExternalBlockElements)]
    public bool IndexExternalBlockElements { get; set; } = StaticIndexExternalBlockElements;

    /// <summary>
    /// Gets or sets a value for how many items to index at a time.
    /// </summary>
    /// <remarks>
    /// This is the primary lever for the peak memory used while (re)building an index: a full page of
    /// content and its property data is held in memory at once, so lowering this value reduces rebuild
    /// memory at the cost of more, smaller batches. Lower it on very large sites that hit memory pressure
    /// during a rebuild.
    /// </remarks>
    [DefaultValue(StaticBatchSize)]
    public int BatchSize { get; set; } = StaticBatchSize;
}
