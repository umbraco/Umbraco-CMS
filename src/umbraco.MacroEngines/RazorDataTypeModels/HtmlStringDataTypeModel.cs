using System.Text;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Macros;
using Umbraco.Web;

namespace umbraco.MacroEngines.RazorDataTypeModels
{
    /// <summary>
    /// This ensures that the RTE renders with HtmlString encoding and also ensures that the macro contents in it
    /// render properly too.
    /// </summary>
    [RazorDataTypeModel(Constants.PropertyEditors.TinyMCEv3, 90)]
    public class HtmlStringDataTypeModel : IRazorDataTypeModel
    {
        public bool Init(int CurrentNodeId, string PropertyData, out object instance)
        {
            //we're going to send the string through the macro parser and create the output string.
            if (UmbracoContext.Current != null)
            {
                var sb = new StringBuilder();
                var umbracoComponentRenderer = new UmbracoComponentRenderer(UmbracoContext.Current);
                MacroTagParser.ParseMacros(
                    PropertyData,
                    //callback for when text block is found
                    textBlock => sb.Append(textBlock),
                    //callback for when macro syntax is found
                    (macroAlias, macroAttributes) => sb.Append(umbracoComponentRenderer.RenderMacro(
                        macroAlias,
                        //needs to be explicitly casted to Dictionary<string, object>
                        macroAttributes.ConvertTo(x => (string)x, x => x)).ToString()));
                instance = new HtmlString(sb.ToString());
            }
            else
            {
                //we have no umbraco context, so best we can do is convert to html string
                instance = new HtmlString(PropertyData);
            }
            return true;
        }
    }
}
