using Umbraco.Core.Models;

namespace Umbraco.Core.Macros.PropertyTypes
{
    public class MediaCurrent : IMacroPropertyType
    {
        public string Alias
        {
            get { return "mediaCurrent"; }
        }

        public string RenderingAssembly
        {
            get { return "umbraco.macroRenderings"; }
        }

        public string RenderingType
        {
            get { return "media"; }
        }

        public MacroPropertyTypeBaseTypes BaseType
        {
            get { return MacroPropertyTypeBaseTypes.Int32; }
        }
    }
}