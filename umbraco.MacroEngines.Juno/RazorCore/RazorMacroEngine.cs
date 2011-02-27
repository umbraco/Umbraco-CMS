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

        public const string RazorTempDirectory = "~/App_Data/TEMP/Razor/";

        public string GetVirtualPathFromPhysicalPath(string physicalPath) {
            string rootpath = HttpContext.Current.Server.MapPath("~/");
            physicalPath = physicalPath.Replace(rootpath, "");
            physicalPath = physicalPath.Replace("\\", "/");
            return "~/" + physicalPath;
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

            if (skipIfFileExists && File.Exists(physicalPath))
                return relativePath;
            if (File.Exists(physicalPath))
                File.Delete(physicalPath);
            if (!Directory.Exists(physicalDirectoryPath))
                Directory.CreateDirectory(physicalDirectoryPath);
            using (var file = new StreamWriter(physicalPath))
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
            razorWebPage.Context = contextWrapper;

            //Inject Macro Model And Parameters
            if (razorWebPage is IMacroContext) {
                var razorMacro = (IMacroContext)razorWebPage;
                razorMacro.SetMembers(macro, currentPage);
            }
        }

        public string ExecuteRazor(MacroModel macro, INode currentPage) {
            var context = HttpContext.Current;
            var contextWrapper = new HttpContextWrapper(context);

            string fileLocation = null;
            if (!string.IsNullOrEmpty(macro.ScriptName)) {
                //Razor Is Already Contained In A File
                if (!macro.ScriptName.StartsWith("~"))
                    fileLocation = macro.ScriptName;
                else
                    fileLocation = SystemDirectories.Python + "/" + macro.ScriptName;
            } else if (!string.IsNullOrEmpty(macro.ScriptCode) && !string.IsNullOrEmpty(macro.ScriptLanguage)) {
                //Inline Razor Syntax
                fileLocation = CreateInlineRazorFile(macro.ScriptCode, macro.ScriptLanguage);
            }

            if (string.IsNullOrEmpty(fileLocation))
                return String.Empty; //No File Location

            var razorWebPage = CompileAndInstantiate(fileLocation);
            InjectContext(razorWebPage, macro, currentPage);

            //Output Razor To String
            var output = new StringWriter();
            razorWebPage.ExecutePageHierarchy(new WebPageContext(contextWrapper, razorWebPage, null), output);
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