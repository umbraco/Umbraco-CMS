// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for index creator settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigExamine)]
public class IndexCreatorSettings
{
    /// <summary>
    ///     Gets or sets a value for lucene directory factory type.
    /// </summary>
    public LuceneDirectoryFactory LuceneDirectoryFactory { get; set; }

    /// <summary>
    ///     Enable main index recovery when it gets corrupted
    /// </summary>
    public bool EnableRecovery { get; set; } = true;
}
