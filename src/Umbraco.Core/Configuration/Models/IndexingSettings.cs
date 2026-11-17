using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for index creator settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigIndexing)]
public class IndexingSettings
{
    private const bool StaticExplicitlyIndexEachNestedProperty = false;
    private const int StaticBatchSize = 10000;
    private const bool StaticLoadOnlyPropertiesForIncludedFields = false;

    /// <summary>
    /// Gets or sets a value for whether each nested property should have it's own indexed value. Requires a rebuild of indexes when changed.
    /// </summary>
    [DefaultValue(StaticExplicitlyIndexEachNestedProperty)]
    public bool ExplicitlyIndexEachNestedProperty { get; set; } = StaticExplicitlyIndexEachNestedProperty;

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

    /// <summary>
    /// Gets or sets a value for whether a content index rebuild loads only the properties that can produce the fields the indexes include.
    /// </summary>
    /// <remarks>
    /// This applies when every index being rebuilt validates with a validator that includes specific fields. It reduces
    /// the time and memory a rebuild takes on sites with many cultures or large property values. The property each
    /// included field comes from is worked out from the field name, which must be the property alias, optionally prefixed
    /// with the raw field prefix and followed by a nested value path or a culture suffix. That's how the built-in property
    /// index value factories name fields; leave this disabled if a custom factory or value set builder produces included
    /// fields named differently, as those fields would otherwise be left empty by a rebuild.
    /// </remarks>
    [DefaultValue(StaticLoadOnlyPropertiesForIncludedFields)]
    public bool LoadOnlyPropertiesForIncludedFields { get; set; } = StaticLoadOnlyPropertiesForIncludedFields;
}
