using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
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

        public string GetMd5(string text) {
            var x = new System.Security.Cryptography.MD5CryptoServiceProvider();
            var bs = System.Text.Encoding.UTF8.GetBytes(text);
            bs = x.ComputeHash(bs);
            var s = new System.Text.StringBuilder();
            foreach (var b in bs) {
                s.Append(b.ToString("x2").ToLower());
            }
            return s.ToString();
        }

        public string CreateInlineRazorFile(string razorSyntax, string scriptLanguage) {
            if (razorSyntax == null)
                throw new ArgumentNullException("razorSyntax");
            if (scriptLanguage == null)
                throw new AbandonedMutexException("scriptLanguage");
            var syntaxMd5 = GetMd5(razorSyntax);
            var relativePath = "~/App_Data/inlinerazor-" + syntaxMd5 + "." + scriptLanguage;
            var physicalPath = IOHelper.MapPath(relativePath);

            if (File.Exists(physicalPath)) {
                var created = File.GetCreationTime(physicalPath);
                if (created <= DateTime.Today.AddMinutes(-10))
                    File.Delete(physicalPath);
                else
                    return relativePath;
            }
            using (var file = new StreamWriter(physicalPath)) {
                file.Write(razorSyntax);
            }
            return relativePath;
        }

        public string ExecuteRazor(MacroModel macro, INode currentPage) {
            var context = HttpContext.Current;
            var contextWrapper = new HttpContextWrapper(context);

            string fileLocation = null;
            if (!string.IsNullOrEmpty(macro.ScriptName)) {
                //Razor Is Already Contained In A File
                fileLocation = SystemDirectories.Python + "/" + macro.ScriptName;
            } else if (!string.IsNullOrEmpty(macro.ScriptCode) && !string.IsNullOrEmpty(macro.ScriptLanguage)) {
                //Inline Razor Syntax
                fileLocation = CreateInlineRazorFile(macro.ScriptCode, macro.ScriptLanguage);
            }

            if (string.IsNullOrEmpty(fileLocation))
                return String.Empty; //No File Location

            if (fileLocation.StartsWith("~/") == false)
                throw new Exception("Only Relative Paths Are Supported");

            var physicalPath = IOHelper.MapPath(fileLocation);
            if (File.Exists(physicalPath) == false)
                throw new FileNotFoundException(string.Format("Razor Script Not Found At Location, {0}", fileLocation));

            //Compile Razor - We Will Leave This To ASP.NET Compilation Engine
            //Security in medium trust is strict around here, so we can only pass a relative file path
            //ASP.NET Compilation Engine caches returned types
            var razorType = BuildManager.GetCompiledType(fileLocation);
            if (razorType == null)
                throw new ArgumentException("Null Razor Compile Type Returned From The ASP.NET Compilation Engine");
            
            //Instantiates The Razor Script
            var razorObj = Activator.CreateInstance(razorType);
            var razorWebPage = razorObj as WebPageBase;
            if (razorWebPage == null)
                throw new InvalidCastException("Razor Context Must Implement System.Web.WebPages.WebPageBase, System.Web.WebPages");

            //inject http context - for request response
            razorWebPage.Context = contextWrapper;

            //Inject Macro Model And Parameters
            if (razorObj is IMacroContext)  {
                var razorMacro = (IMacroContext)razorObj;
                razorMacro.SetMembers(macro, currentPage);
            }

            //Output Razor To String
            var output = new StringWriter();
            razorWebPage.ExecutePageHierarchy(new WebPageContext(contextWrapper, razorWebPage, null), output);
            return output.ToString();
        }

        public string Execute(MacroModel macro, INode currentPage) {
            try {
                return ExecuteRazor(macro, currentPage);
            } catch (Exception exception) {
                HttpContext.Current.Trace.Warn("umbracoMacro", string.Format("Error Loading Razor Script (file: {0}) {1} {2}", macro.Name, exception.Message, exception.StackTrace));
                var loading = string.Format("<div style=\"border: 1px solid #990000\">Error loading Razor Script {0}</br/>", macro.ScriptName);
                if (GlobalSettings.DebugMode)
                    loading = loading + exception.Message;
                loading = loading + "</div>";
                return loading;
            }
        }

        #endregion
    }
}