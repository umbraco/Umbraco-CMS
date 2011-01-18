using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Web.Compilation;
using System.Web.WebPages;
using umbraco.cms.businesslogic.macro;
using umbraco.interfaces;
using umbraco.IO;

namespace umbraco.MacroEngines
{
    public class RazorMacroEngine : IMacroEngine {

        #region IMacroEngine Members

        public string Name { get { return "Razor Macro Engine"; } }

        public List<string> SupportedExtensions { get { return new List<string> {"cshtml", "vbhtml"}; } }

        public Dictionary<string, IMacroGuiRendering> SupportedProperties {
            get { throw new NotSupportedException(); }
        }

        public bool Validate(string code, INode currentPage, out string errorMessage) {
            errorMessage = String.Empty;
            return true;
        }

        public string ExecuteRazor(MacroModel macro, INode currentPage) {
            var context = HttpContext.Current;
            var isDebugMode = GlobalSettings.DebugMode && HttpContext.Current.Request.QueryString["umbDebug"] != null;
            var sw = new Stopwatch();
            if (isDebugMode)
                sw.Start();

            var fileLocation = SystemDirectories.Python + "/" + macro.ScriptName;

            //Returns The Compiled System.Type
            var razorType = BuildManager.GetCompiledType(fileLocation);

            //Instantiates The Razor Script
            var razorObj = Activator.CreateInstance(razorType);
            var razorWebPage = razorObj as WebPageBase;
            if (razorWebPage == null)
                throw new InvalidCastException("Razor Template Must Implement System.Web.WebPages.WebPageBase");

            //inject http context - for request response
            var httpContext = new HttpContextWrapper(context);
            razorWebPage.Context = httpContext;

            //inject macro and parameters
            if (razorObj is IMacroContext)  {
                var razorMacro = (IMacroContext)razorObj;
                razorMacro.SetMembers(macro, currentPage);
            }

            //output template
            var output = new StringWriter();
            if (isDebugMode)
                output.Write(string.Format(@"<div title=""Macro Tag: '{0}'"" style=""border: 1px solid #009;""><div style=""border: 1px solid #CCC;"">", macro.Alias));
            razorWebPage.ExecutePageHierarchy(new WebPageContext(httpContext, razorWebPage, null), output);
            if (isDebugMode) {
                sw.Stop();
                output.Write(string.Format("<strong>Taken {0}ms<strong>", sw.ElapsedMilliseconds));
                output.Write("</div>");
            }
            return output.ToString();
        }

        public string Execute(MacroModel macro, INode currentPage) {
            try {
                return ExecuteRazor(macro, currentPage);
            } catch (Exception exception) {
                HttpContext.Current.Trace.Write("Macro", string.Format("Error loading Razor Script (file: {0}) {1}", macro.Name, exception.Message));
                var loading = string.Format("<div>Error loading Razor Script (file: {0})</br/>", macro.ScriptName);
                if (GlobalSettings.DebugMode)
                    loading = loading + exception.Message;
                loading = loading + "</div>";
                return loading;
            }
        }
        #endregion
    }
}