using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using umbraco.cms.businesslogic.macro;
using umbraco.interfaces;
using umbraco.IO;

namespace umbraco.MacroEngines
{
    public class RazorEngine : IMacroEngine
    {
        public string Name
        {
            get { return "Razor Enggine"; }
        }

        public List<string> SupportedExtensions
        {
            get
            {
                var exts = new List<string> { "razor" };
                return exts;
            }
        }

        public Dictionary<string, IMacroGuiRendering> SupportedProperties
        {
            get { throw new NotImplementedException(); }
        }

        public bool Validate(string code, INode currentPage, out string errorMessage)
        {
            throw new NotImplementedException();
        }

        public string Execute(MacroModel macro, INode currentPage)
        {
            string result = String.Empty;
            string template = !String.IsNullOrEmpty(macro.ScriptCode) ? macro.ScriptCode : loadScript(IOHelper.MapPath(SystemDirectories.Python + "/" + macro.ScriptName));
            try
            {
                //TODO: Add caching support
                //                if (CacheTemplate)
                //                    result = Razor.Parse(template, new DynamicNode(currentPage), this.ID + "_razorTemplate");
                //                else
                //                {
                result = Razor.Razor.Parse(template, new DynamicNode(currentPage));
            }
            //            }
            catch (Exception ee)
            {
                result = string.Format(
                    "<div class=\"error\"><h1>Razor Macro Engine</h1><em>An error occured while rendering the following code:</em><br /><p>{0}</p><code>{1}</code></div>",
                    ee.ToString(),
                    HttpContext.Current.Server.HtmlEncode(template));
            }

            return result;
        }

        private string loadScript(string scriptName)
        {
            if (File.Exists(scriptName))
            {
                return File.ReadAllText(scriptName);
            }

            return String.Empty;
        }
    }
}
