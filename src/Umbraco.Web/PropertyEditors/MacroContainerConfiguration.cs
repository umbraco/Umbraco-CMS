using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the macro container value editor.
    /// </summary>
    public class MacroContainerConfiguration
    {
        [ConfigurationField("max", "Max items", "number", Description = "The maximum number of macros that are allowed in the container")]
        public int MaxItems { get; set; }

        [ConfigurationField("allowed", "Allowed items", "views/propertyeditors/macrocontainer/macrolist.prevalues.html", Description = "The macro types allowed, if none are selected all macros will be allowed")]
        public object AllowedItems { get; set; }
    }
}