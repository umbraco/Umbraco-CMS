using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for index creator settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigIndexing)]
public class IndexingSettings
{
    private const bool StaticExplicitlyIndexEachNestedProperty = false;
    private const bool StaticIndexExternalElements = false;
    private const int StaticBatchSize = 10000;

    /// <summary>
    /// Gets or sets a value for whether each nested property should have it's own indexed value. Requires a rebuild of indexes when changed.
    /// </summary>
    [DefaultValue(StaticExplicitlyIndexEachNestedProperty)]
    public bool ExplicitlyIndexEachNestedProperty { get; set; } = StaticExplicitlyIndexEachNestedProperty;

    /// <summary>
    /// Gets or sets a value indicating whether the content of external elements referenced by block editors is flattened into the index entry of referencing documents. Requires a rebuild of indexes when changed.
    /// </summary>
    [DefaultValue(StaticIndexExternalElements)]
    public bool IndexExternalElements { get; set; } = StaticIndexExternalElements;

    /// <summary>
    /// Gets or sets a value for how many items to index at a time.
    /// </summary>
    public int BatchSize { get; set; } = StaticBatchSize;
}
