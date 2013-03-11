using Umbraco.Core.Models;

namespace Umbraco.Core.Macros.PropertyTypes
{
    internal class TextMultiLine : IMacroPropertyType
    {
        public string Alias
        {
            get { return "textMultiLine"; }
        }

        public string RenderingAssembly
        {
            get { return "umbraco.macroRenderings"; }
        }

        public string RenderingType
        {
            get { return "textMultiple"; }
        }

        public MacroPropertyTypeBaseTypes BaseType
        {
            get { return MacroPropertyTypeBaseTypes.String; }
        }
    }
}