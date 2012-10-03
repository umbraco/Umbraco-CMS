using Umbraco.Core.Models;

namespace Umbraco.Core.Macros.PropertyTypes
{
    public class YesNoBool : IMacroPropertyType
    {
        public string Alias
        {
            get { return "bool"; }
        }

        public string RenderingAssembly
        {
            get { return "umbraco.macroRenderings"; }
        }

        public string RenderingType
        {
            get { return "yesNo"; }
        }

        public MacroPropertyTypeBaseTypes BaseType
        {
            get { return MacroPropertyTypeBaseTypes.Boolean; }
        }
    }
}