using Umbraco.Core.Models;

namespace Umbraco.Core.Macros.PropertyTypes
{
    internal class TabPickerMultiple : IMacroPropertyType
    {
        public string Alias
        {
            get { return "tabPickerMultiple"; }
        }

        public string RenderingAssembly
        {
            get { return "umbraco.macroRenderings"; }
        }

        public string RenderingType
        {
            get { return "tabPickerMultiple"; }
        }

        public MacroPropertyTypeBaseTypes BaseType
        {
            get { return MacroPropertyTypeBaseTypes.String; }
        }
    }
}