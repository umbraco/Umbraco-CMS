// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;

namespace Umbraco.Cms.Core.Configuration.Models
{
    /// <summary>
    /// Typed configuration options for index creator settings.
    /// </summary>
    public class IndexCreatorSettings
    {
        /// <summary>
        /// Gets or sets a value for lucene directory factory type.
        /// </summary>
        public LuceneDirectoryFactory LuceneDirectoryFactory { get; set; }

    }
}
