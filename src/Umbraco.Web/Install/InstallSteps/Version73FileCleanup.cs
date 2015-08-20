using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install.InstallSteps
{
    [InstallSetupStep(InstallationType.Upgrade,
        "Version73FileCleanup", 2, 
        "Performing some housecleaning...", 
        PerformsAppRestart = true)]
    internal class Version73FileCleanup : InstallSetupStep<object>
    {
        private readonly HttpContextBase _httpContext;
        private readonly ILogger _logger;

        public Version73FileCleanup(HttpContextBase httpContext, ILogger logger)
        {
            _httpContext = httpContext;
            _logger = logger;
        }

        /// <summary>
        /// The step execution method
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public override InstallSetupResult Execute(object model)
        {
            //first cleanup all web.configs

            var root = new DirectoryInfo(_httpContext.Server.MapPath("~/"));
            ProcessWebConfigs(root);
            
            //now remove the dll 

            var bin = root.GetDirectories("bin", SearchOption.TopDirectoryOnly);
            if (bin.Length == 1)
            {
                var dll = bin[0].GetFiles("Microsoft.Web.Mvc.FixedDisplayModes.dll", SearchOption.TopDirectoryOnly);
                if (dll.Length == 1)
                {
                    _logger.Info<Version73FileCleanup>("Deleting non-compatible and no longer used DLL: {0}", () => dll[0].FullName);
                    File.Delete(dll[0].FullName);
                }
            }

            return null;
        }

        /// <summary>
        /// Determines if this step needs to execute based on the current state of the application and/or install process
        /// </summary>
        /// <returns></returns>
        public override bool RequiresExecution(object model)
        {
            return UmbracoVersion.Current == Version.Parse("7.3.0");
        }

        private void ProcessWebConfigs(DirectoryInfo dir)
        {
            //Do the processing of files
            var found = dir.GetFiles("web.config", SearchOption.AllDirectories);

            foreach (var configFile in found)
            {
                var fileName = configFile.FullName;
                _logger.Info<Version73FileCleanup>("Cleaning up web.config file: {0}", () => fileName);

                var contents = File.ReadAllText(fileName);
                contents = _microsoftWebHelpers.Replace(contents, string.Empty);
                contents = _webPagesRazorVersion.Replace(contents, "$1=3.0.0.0");
                contents = _mvcVersion.Replace(contents, "$1=5.2.3.0");
                using (var writer = new StreamWriter(configFile.FullName, false))
                {
                    writer.Write(contents);
                }
            }
        }

        private readonly Regex _microsoftWebHelpers = new Regex(@"<add\s*?namespace=""Microsoft\.Web\.Helpers""\s*?/>", RegexOptions.Compiled);
        private readonly Regex _webPagesRazorVersion = new Regex(@"(System\.Web\.WebPages\.Razor,\s*?Version)(=2\.0\.0\.0)", RegexOptions.Compiled);
        private readonly Regex _mvcVersion = new Regex(@"(System\.Web\.Mvc,\s*?Version)(=4\.0\.0\.0)", RegexOptions.Compiled);
    }
}