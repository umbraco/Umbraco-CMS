// Copyright (c) Umbraco.
// See LICENSE for more details.


using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Search.Examine.ExamineConstants;

namespace Umbraco.Search.Examine.Configuration;

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
}
