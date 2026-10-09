// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Search.Provider.Examine.Configuration;

// Deliberately in the global namespace: the JsonSchemaGenerate MSBuild task resolves this type by its bare
// name (TypeName="UmbracoSearchExamineSchema" in the .csproj), which only works unqualified.

/// <summary>
/// Describes this provider's own slice of the configuration tree, for generating its
/// <c>appsettings-schema.Umbraco.Cms.Search.Examine.json</c> fragment at build time.
/// </summary>
internal sealed class UmbracoSearchExamineSchema // NOSONAR: deliberately global-namespaced, see comment above
{
    /// <summary>
    /// Gets or sets the configuration for this provider.
    /// </summary>
    public required UmbracoDefinition Umbraco { get; set; }

    /// <summary>
    /// Configuration container for all Umbraco products.
    /// </summary>
    public class UmbracoDefinition
    {
        /// <summary>
        /// Gets or sets the configuration of Umbraco CMS.
        /// </summary>
        public required UmbracoCmsDefinition CMS { get; set; }
    }

    /// <summary>
    /// Configuration of Umbraco CMS.
    /// </summary>
    public class UmbracoCmsDefinition
    {
        /// <summary>
        /// Gets or sets the configuration of Umbraco Search providers.
        /// </summary>
        public required SearchDefinition Search { get; set; }
    }

    /// <summary>
    /// Configuration of Umbraco Search providers.
    /// </summary>
    public class SearchDefinition
    {
        /// <summary>
        /// Gets or sets the configuration of the Examine search provider.
        /// </summary>
        public required ExamineSearchProviderSettings Examine { get; set; }
    }
}
