using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using RazorEngine;
using RazorEngine.Templating;
using umbraco.cms.businesslogic.macro;
using umbraco.interfaces;
using umbraco.IO;
using RE = RazorEngine;

namespace umbraco.MacroEngines
{
    public class RazorEngine : IMacroEngine
    {
        public string Name
        {
            get { return "Razor Engine"; }
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
                Razor.SetTemplateBaseType(typeof(UmbracoTemplateBase<>));
                result = Razor.Parse(template, new DynamicNode(currentPage), macro.CacheIdenitifier);

            }
            catch (TemplateException ee)
            {
                string error = ee.ToString();
                if (ee.Errors.Count > 0)
                {
                    error += "</p><strong>Detailed errors:</strong><br/><ul style=\"list-style-type: disc; margin: 1em 0;\">";
                    foreach (var err in ee.Errors)
                        error += string.Format("<li style=\"display: list-item;\">{0}</li>", err.ToString());
                    error += "</ul><p>";
                }
                result = string.Format(
                    "<div class=\"error\"><h3>Razor Macro Engine</h3><p><em>An TemplateException occured while parsing the following code:</em></p><p>{0}</p><h4 style=\"font-weight: bold; margin: 0.5em 0 0.3em 0;\">Your Razor template:</h4><code>{1}</code><h4 style=\"font-weight: bold; margin: 0.5em 0 0.3em 0;\">Cache key:</h4><p>{2}</p></div>",
                    error,
                    HttpContext.Current.Server.HtmlEncode(template),
                    friendlyCacheKey(macro.CacheIdenitifier));
            }
            catch (Exception ee)
            {
                result = string.Format(
                    "<div class=\"error\"><h3>Razor Macro Engine</h3><em>An unknown error occured while rendering the following code:</em><br /><p>{0}</p><h4 style=\"font-weight: bold; margin: 0.5em 0 0.3em 0;\">Your Razor template:</h4><code>{1}</code><h4 style=\"font-weight: bold; margin: 0.5em 0 0.3em 0;\">Cache key:</h4><p>{2}</p></div>",
                    ee.ToString(),
                    HttpContext.Current.Server.HtmlEncode(template),
                                        friendlyCacheKey(macro.CacheIdenitifier));
            }

            return result;
        }

        private string friendlyCacheKey(string cacheKey)
        {
            if (!String.IsNullOrEmpty(cacheKey))
                return cacheKey;
            else
                return "<em>No caching defined</em>";
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

    public abstract class UmbracoTemplateBase<T> : TemplateBase<T>
    {
        private object m_model;

        public override T Model
        {
            get
            {
                return (T)m_model;
            }
            set
            {
                m_model = value;
            }
        }

        public string ToUpperCase(string name)
        {
            return name.ToUpper();
        }
    }
}
