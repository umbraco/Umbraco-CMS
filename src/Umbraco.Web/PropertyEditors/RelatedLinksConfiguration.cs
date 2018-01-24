using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the related links value editor.
    /// </summary>
    public class RelatedLinksConfiguration
    {
        [ConfigurationField("max", "Maximum number of links", "number", Description = "Enter the maximum amount of links to be added, enter 0 for unlimited")]
        public int Maximum { get; set; }
    }
}