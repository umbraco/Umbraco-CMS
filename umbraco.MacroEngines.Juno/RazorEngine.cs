using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
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
        #region IMacroEngine Members

        public string Name
        {
            get { return "Razor Engine"; }
        }

        public List<string> SupportedExtensions
        {
            get
            {
                var exts = new List<string> {"razor"};
                return exts;
            }
        }

        public Dictionary<string, IMacroGuiRendering> SupportedProperties
        {
            get { throw new NotImplementedException(); }
        }

        public bool Validate(string code, INode currentPage, out string errorMessage)
        {
            try
            {
                string parsedResult;
                if (!GetResult("RazorValidation", code, currentPage, out parsedResult)) {
                    errorMessage = parsedResult;
                    return false;
                }

            }
            catch (Exception ee)
            {
                errorMessage = ee.ToString();
                return false;
            }

            // clear razor compilation cache (a hack - by setting the template base type back/forward as there isn't a clear cache method)
            Razor.SetTemplateBaseType(typeof (HtmlTemplateBase<>));
            Razor.SetTemplateBaseType(typeof (UmbracoTemplateBase<>));


            errorMessage = String.Empty;
            return true;
        }

        public string Execute(MacroModel macro, INode currentPage)
        {
            string template = !String.IsNullOrEmpty(macro.ScriptCode)
                                  ? macro.ScriptCode
                                  : loadScript(IOHelper.MapPath(SystemDirectories.Python + "/" + macro.ScriptName));
            string parsedResult;
            GetResult(macro.CacheIdentifier, template, currentPage, out parsedResult);
            return parsedResult;
        }

        #endregion

        private bool GetResult(string cacheIdentifier, string template, INode currentPage, out string result)
        {
            try
            {
                Razor.SetTemplateBaseType(typeof (UmbracoTemplateBase<>));
                result = Razor.Parse(template, new DynamicNode(currentPage), cacheIdentifier);
                return true;
            }
            catch (TemplateException ee)
            {
                string error = ee.ToString();
                if (ee.Errors.Count > 0)
                {
                    error +=
                        "</p><strong>Detailed errors:</strong><br/><ul style=\"list-style-type: disc; margin: 1em 0;\">";
                    foreach (CompilerError err in ee.Errors)
                        error += string.Format("<li style=\"display: list-item;\">{0}</li>", err);
                    error += "</ul><p>";
                }
                result = string.Format(
                    "<div class=\"error\"><h3>Razor Macro Engine</h3><p><em>An TemplateException occured while parsing the following code:</em></p><p>{0}</p><h4 style=\"font-weight: bold; margin: 0.5em 0 0.3em 0;\">Your Razor template:</h4><code>{1}</code><h4 style=\"font-weight: bold; margin: 0.5em 0 0.3em 0;\">Cache key:</h4><p>{2}</p></div>",
                    error,
                    HttpContext.Current.Server.HtmlEncode(template),
                    friendlyCacheKey(cacheIdentifier));
            }
            catch (Exception ee)
            {
                result = string.Format(
                    "<div class=\"error\"><h3>Razor Macro Engine</h3><em>An unknown error occured while rendering the following code:</em><br /><p>{0}</p><h4 style=\"font-weight: bold; margin: 0.5em 0 0.3em 0;\">Your Razor template:</h4><code>{1}</code><h4 style=\"font-weight: bold; margin: 0.5em 0 0.3em 0;\">Cache key:</h4><p>{2}</p></div>",
                    ee,
                    HttpContext.Current.Server.HtmlEncode(template),
                    friendlyCacheKey(cacheIdentifier));
            }
            return false;
        }

        private string friendlyCacheKey(string cacheKey)
        {
            if (!String.IsNullOrEmpty(cacheKey))
                return cacheKey;

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
            get { return (T) m_model; }
            set { m_model = value; }
        }

        public string ToUpperCase(string name)
        {
            return name.ToUpper();
        }
    }
}