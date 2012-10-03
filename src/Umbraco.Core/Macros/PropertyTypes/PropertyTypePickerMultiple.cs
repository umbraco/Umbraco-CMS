using Umbraco.Core.Models;

namespace Umbraco.Core.Macros.PropertyTypes
{
    public class PropertyTypePickerMultiple : IMacroPropertyType
    {
        public string Alias
        {
            get { return "propertyTypePickerMultiple"; }
        }

        public string RenderingAssembly
        {
            get { return "umbraco.macroRenderings"; }
        }

        public string RenderingType
        {
            get { return "propertyTypePickerMultiple"; }
        }

        public MacroPropertyTypeBaseTypes BaseType
        {
            get { return MacroPropertyTypeBaseTypes.String; }
        }
    }
}