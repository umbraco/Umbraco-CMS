using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for index creator settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigIndexing)]
public class IndexingSettings
{
    private const bool StaticExplicitlyIndexEachNestedProperty = false;

    /// <summary>
    /// Gets or sets a value for whether each nested property should have it's own indexed value. Requires a rebuild of indexes when changed.
    /// </summary>
    [DefaultValue(StaticExplicitlyIndexEachNestedProperty)]
    public bool ExplicitlyIndexEachNestedProperty { get; set; } = StaticExplicitlyIndexEachNestedProperty;
}
