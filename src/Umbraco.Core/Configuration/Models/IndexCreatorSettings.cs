// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for index creator settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigExamine)]
public class IndexCreatorSettings
{
    private const bool StaticExplicitlyIndexEachNestedProperty = true;

    /// <summary>
    ///     Gets or sets a value for lucene directory factory type.
    /// </summary>
    public LuceneDirectoryFactory LuceneDirectoryFactory { get; set; }

    /// <summary>
    /// Gets or sets a value for whether each nested property should have it's own indexed value. Requires a rebuild of indexes when changed.
    /// </summary>
    [DefaultValue(StaticExplicitlyIndexEachNestedProperty)]
    public bool ExplicitlyIndexEachNestedProperty { get; set; } = StaticExplicitlyIndexEachNestedProperty;
}
