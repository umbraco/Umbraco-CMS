using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Compilation;
using System.Web.WebPages;
using Umbraco.Core;
using umbraco.MacroEngines.Resources;
using umbraco.cms.businesslogic.macro;
using umbraco.interfaces;
using Umbraco.Core.IO;

namespace umbraco.MacroEngines
{
    public class RazorMacroEngine : IMacroEngine, IMacroEngineResultStatus {

        public const string RazorTempDirectory = "~/App_Data/TEMP/Razor/";
        private const string RazorWebConfig = "~/App_Data/TEMP/Razor/web.config";

        public string GetVirtualPathFromPhysicalPath(string physicalPath) {
            string rootpath = HttpContext.Current.Server.MapPath("~/");
            physicalPath = physicalPath.Replace(rootpath, "");
            physicalPath = physicalPath.Replace("\\", "/");
            return "~/" + physicalPath;
        }

        public string GetMd5(string text) {
			return text.ToMd5();
        }

        /// <summary>
        /// Creates A Temporary Razor File
        /// </summary>
        public string CreateTemporaryRazorFile(string razorSyntax, string fileName, bool skipIfFileExists) {
            if (razorSyntax == null)
                throw new ArgumentNullException("razorSyntax");
            if (fileName == null)
                throw new AbandonedMutexException("fileName");

            var relativePath = RazorTempDirectory + fileName;
            var physicalPath = IOHelper.MapPath(relativePath);
            var physicalDirectoryPath = IOHelper.MapPath(RazorTempDirectory);
            var webconfig = IOHelper.MapPath(RazorWebConfig);

            if (skipIfFileExists && File.Exists(physicalPath))
                return relativePath;
            if (File.Exists(physicalPath))
                File.Delete(physicalPath);
            if (!Directory.Exists(physicalDirectoryPath))
                Directory.CreateDirectory(physicalDirectoryPath);

            //Ensure the correct razor web.config is there
            if (File.Exists(webconfig) == false)
            {
                using (var writer = File.CreateText(webconfig))
                {
                    writer.Write(Strings.WebConfig);
                }
            }

            using (var file = new StreamWriter(physicalPath, false, Encoding.UTF8))
            {
                file.Write(razorSyntax);
            }
            return relativePath;
        }

        public static WebPage CompileAndInstantiate(string virtualPath) {
            //Compile Razor - We Will Leave This To ASP.NET Compilation Engine & ASP.NET WebPages
            //Security in medium trust is strict around here, so we can only pass a virtual file path
            //ASP.NET Compilation Engine caches returned types
            //Changed From BuildManager As Other Properties Are Attached Like Context Path/
            var webPageBase = WebPageBase.CreateInstanceFromVirtualPath(virtualPath);
            var webPage = webPageBase as WebPage;
            if (webPage == null)
                throw new InvalidCastException("Context Must Implement System.Web.WebPages.WebPage");
            return webPage;
        }

        public static void InjectContext(WebPage razorWebPage, MacroModel macro, INode currentPage) {
            var context = HttpContext.Current;
            var contextWrapper = new HttpContextWrapper(context);

            //inject http context - for request response
            HttpContext.Current.Trace.Write("umbracoMacro", string.Format("Loading Macro Script Context (file: {0})", macro.Name));
            razorWebPage.Context = contextWrapper;
            HttpContext.Current.Trace.Write("umbracoMacro", string.Format("Done Loading Macro Script Context (file: {0})", macro.Name));

            //Inject Macro Model And Parameters
            if (razorWebPage is IMacroContext) {
                HttpContext.Current.Trace.Write("umbracoMacro", string.Format("Boxing Macro Script MacroContext (file: {0})", macro.Name));
                var razorMacro = (IMacroContext)razorWebPage;
                HttpContext.Current.Trace.Write("umbracoMacro", string.Format("Done Boxing Macro Script MacroContext (file: {0})", macro.Name));

                HttpContext.Current.Trace.Write("umbracoMacro", string.Format("Loading Macro Script Model (file: {0})", macro.Name));
                razorMacro.SetMembers(macro, currentPage);
                HttpContext.Current.Trace.Write("umbracoMacro", string.Format("Done Loading Macro Script Model (file: {0})", macro.Name));
            }
        }

        public string ExecuteRazor(MacroModel macro, INode currentPage) {
            var context = HttpContext.Current;
            var contextWrapper = new HttpContextWrapper(context);

            string fileLocation = null;
            if (!string.IsNullOrEmpty(macro.ScriptName)) {
                //Razor Is Already Contained In A File
                if (macro.ScriptName.StartsWith("~"))
                    fileLocation = macro.ScriptName;
                else
                    fileLocation = SystemDirectories.MacroScripts + "/" + macro.ScriptName;
            } else if (!string.IsNullOrEmpty(macro.ScriptCode) && !string.IsNullOrEmpty(macro.ScriptLanguage)) {
                //Inline Razor Syntax
                fileLocation = CreateInlineRazorFile(macro.ScriptCode, macro.ScriptLanguage);
            }

            if (string.IsNullOrEmpty(fileLocation))
                return String.Empty; //No File Location

            var razorWebPage = CompileAndInstantiate(fileLocation);

            HttpContext.Current.Trace.Write("umbracoMacro", string.Format("Loading Macro Script Context (file: {0})", macro.Name));
            InjectContext(razorWebPage, macro, currentPage);
            HttpContext.Current.Trace.Write("umbracoMacro", string.Format("Done Loading Macro Script Context (file: {0})", macro.Name));

            //Output Razor To String
            var output = new StringWriter();
            HttpContext.Current.Trace.Write("umbracoMacro", string.Format("Executing Macro Script (file: {0})", macro.Name));
            razorWebPage.ExecutePageHierarchy(new WebPageContext(contextWrapper, razorWebPage, null), output);
            HttpContext.Current.Trace.Write("umbracoMacro", string.Format("Done Executing Macro Script (file: {0})", macro.Name));
            return output.ToString();
        }

        /// <summary>
        /// Creates Inline Razor File
        /// </summary>
        public string CreateInlineRazorFile(string razorSyntax, string scriptLanguage) {
            if (razorSyntax == null)
                throw new ArgumentNullException("razorSyntax");
            if (scriptLanguage == null)
                throw new ArgumentNullException("scriptLanguage");

            //Get Rid Of Whitespace From Start/End
            razorSyntax = razorSyntax.Trim();
            //Use MD5 as a cache key
            var syntaxMd5 = GetMd5(razorSyntax);
            var fileName = "inline-" + syntaxMd5 + "." + scriptLanguage;
            return CreateTemporaryRazorFile(razorSyntax, fileName, true);
        }

        #region IMacroEngine Members

        public string Name { get { return "Razor Macro Engine"; } }

        public IEnumerable<string> SupportedExtensions { get { return new List<string> {"cshtml", "vbhtml", "razor"}; } }

        public IEnumerable<string> SupportedUIExtensions { get { return new List<string> { "cshtml", "vbhtml" }; } }

        public Dictionary<string, IMacroGuiRendering> SupportedProperties {
            get { throw new NotSupportedException(); }
        }

        public bool Validate(string code, string tempFilePath, INode currentPage, out string errorMessage) {
            var temp = GetVirtualPathFromPhysicalPath(tempFilePath);
            try {
                CompileAndInstantiate(temp);
            } catch (Exception exception) {
                errorMessage = exception.Message;
                return false;
            }
            errorMessage = String.Empty;
            return true;
        }

        public string Execute(MacroModel macro, INode currentPage) {
            
            //if this is executing, we need to ensure the folder exists and that the correct web.config exists on that folder too
            var absolutePath = IOHelper.MapPath(SystemDirectories.MacroScripts);
            if (!Directory.Exists(absolutePath))
                Directory.CreateDirectory(absolutePath);
            if (!File.Exists(Path.Combine(absolutePath, "web.config")))
            {
                using (var writer = File.CreateText(Path.Combine(absolutePath, "web.config")))
                {
                    writer.Write(Strings.WebConfig);
                }
            }
            
            try
            {
                Success = true;
                return ExecuteRazor(macro, currentPage);
            } catch (Exception exception)
            {
                Success = false;
                ResultException = exception;
                HttpContext.Current.Trace.Warn("umbracoMacro", string.Format("Error Loading Razor Script (file: {0}) {1} {2}", macro.Name, exception.Message, exception.StackTrace));
                throw;
            }
        }

        #endregion


        #region IMacroEngineResultStatus Members

        public bool Success { get; set; }

        public Exception ResultException { get; set; }

        #endregion
    }
}