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
    internal const int StaticBackOfficeSearchMinimumMatchWithinWordLength = 3;

    /// <summary>
    ///     Gets or sets a value for lucene directory factory type.
    /// </summary>
    public LuceneDirectoryFactory LuceneDirectoryFactory { get; set; }

    /// <summary>
    ///     Gets or sets the minimum length of a search word for backoffice search to match it anywhere within a word
    ///     in a node name, rather than only at the start of a word. Set to 0 to only match at the start of a word.
    /// </summary>
    /// <remarks>
    ///     Matching within a word requires a leading-wildcard query per node name field (one per language), which is
    ///     slower than a prefix query on large indexes. Shorter search words only ever match at the start of a word.
    /// </remarks>
    [DefaultValue(StaticBackOfficeSearchMinimumMatchWithinWordLength)]
    public int BackOfficeSearchMinimumMatchWithinWordLength { get; set; } = StaticBackOfficeSearchMinimumMatchWithinWordLength;
}
