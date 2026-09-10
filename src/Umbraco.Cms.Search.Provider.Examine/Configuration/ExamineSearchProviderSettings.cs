// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Configuration.Models;
using CoreConstants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Search.Provider.Examine.Configuration;

/// <summary>
///     Typed configuration options for the Examine provider for Umbraco Search.
/// </summary>
[UmbracoOptions(CoreConstants.Configuration.ConfigSearchExamine)]
public class ExamineSearchProviderSettings
{
    /// <summary>
    ///     Gets or sets a value indicating whether index rebuilds should use zero-downtime indexing
    ///     (an active and a shadow index per index alias, swapped when a rebuild completes).
    /// </summary>
    public bool ZeroDowntimeIndexing { get; set; }

    /// <summary>
    ///     Gets or sets a value for the Lucene directory factory type.
    /// </summary>
    public LuceneDirectoryFactory LuceneDirectoryFactory { get; set; }
}
