using Umbraco.Core.Models;

namespace Umbraco.Core.Macros.PropertyTypes
{
    public class PropertyTypePicker : IMacroPropertyType
    {
        public string Alias
        {
            get { return "propertyTypePicker"; }
        }

        public string RenderingAssembly
        {
            get { return "umbraco.macroRenderings"; }
        }

        public string RenderingType
        {
            get { return "propertyTypePicker"; }
        }

        public MacroPropertyTypeBaseTypes BaseType
        {
            get { return MacroPropertyTypeBaseTypes.String; }
        }
    }
}