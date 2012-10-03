using Umbraco.Core.Models;

namespace Umbraco.Core.Macros.PropertyTypes
{
    public class Text : IMacroPropertyType
    {
        public string Alias
        {
            get { return "text"; }
        }

        public string RenderingAssembly
        {
            get { return "umbraco.macroRenderings"; }
        }

        public string RenderingType
        {
            get { return "text"; }
        }

        public MacroPropertyTypeBaseTypes BaseType
        {
            get { return MacroPropertyTypeBaseTypes.String; }
        }
    }
}