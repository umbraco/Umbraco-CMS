using Umbraco.Core.Models;

namespace Umbraco.Core.Macros.PropertyTypes
{
    public class ContentTypeMultiple : IMacroPropertyType
    {
        public string Alias
        {
            get { return "contentTypeMultiple"; }
        }

        public string RenderingAssembly
        {
            get { return "umbraco.macroRenderings"; }
        }

        public string RenderingType
        {
            get { return "contentTypeMultiple"; }
        }

        public MacroPropertyTypeBaseTypes BaseType
        {
            get { return MacroPropertyTypeBaseTypes.Int32; }
        }
    }
}