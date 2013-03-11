using Umbraco.Core.Models;

namespace Umbraco.Core.Macros.PropertyTypes
{
    internal class ContentType : IMacroPropertyType
    {
        public string Alias
        {
            get { return "contentType"; }
        }

        public string RenderingAssembly
        {
            get { return "umbraco.macroRenderings"; }
        }

        public string RenderingType
        {
            get { return "contentTypeSingle"; }
        }

        public MacroPropertyTypeBaseTypes BaseType
        {
            get { return MacroPropertyTypeBaseTypes.Int32; }
        }
    }
}