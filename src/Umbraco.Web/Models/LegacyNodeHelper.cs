using Umbraco.Core.Models;
using umbraco.interfaces;

namespace Umbraco.Web.Models
{
    /// <summary>
    /// Used to convert to/from the legacy INode from IPublishedContent
    /// </summary>
    internal static class LegacyNodeHelper
    {
        // NOTE - moved from umbraco.MacroEngines to avoid circ. references

        public static INode ConvertToNode(IPublishedContent doc)
        {
            var node = new LegacyConvertedNode(doc);
            return node;
        }

        public static IProperty ConvertToNodeProperty(IPublishedProperty prop)
        {
            return new LegacyConvertedNodeProperty(prop);
        }
        
    }
}
