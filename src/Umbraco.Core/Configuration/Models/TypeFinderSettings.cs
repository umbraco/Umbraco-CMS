// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Configuration.Models
{
    /// <summary>
    /// Typed configuration options for type finder settings.
    /// </summary>
    [UmbracoOptions(Constants.Configuration.ConfigTypeFinder)]
    public class TypeFinderSettings
    {
        /// <summary>
        /// Gets or sets a value for the assemblies that accept load exceptions during type finder operations.
        /// </summary>
        public string AssembliesAcceptingLoadExceptions { get; set; }
    }
}
