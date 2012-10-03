using Umbraco.Core.Models;

namespace Umbraco.Core.Macros.PropertyTypes
{
    public class ContentTree : IMacroPropertyType
    {
        public string Alias
        {
            get { return "contentTree"; }
        }

        public string RenderingAssembly
        {
            get { return "umbraco.macroRenderings"; }
        }

        public string RenderingType
        {
            get { return "content"; }
        }

        public MacroPropertyTypeBaseTypes BaseType
        {
            get { return MacroPropertyTypeBaseTypes.Int32; }
        }
    }
}