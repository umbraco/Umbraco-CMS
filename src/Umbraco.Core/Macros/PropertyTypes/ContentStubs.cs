using Umbraco.Core.Models;

namespace Umbraco.Core.Macros.PropertyTypes
{
    internal class ContentStubs : IMacroPropertyType
    {
        public string Alias
        {
            get { return "contentStubs"; }
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