using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration editor for the boolean value editor.
    /// </summary>
    public class TrueFalseConfigurationEditor : ConfigurationEditor<TrueFalseConfiguration>
    {
        public TrueFalseConfigurationEditor(IIOHelper ioHelper) : base(ioHelper)
        {
        }
    }
}
