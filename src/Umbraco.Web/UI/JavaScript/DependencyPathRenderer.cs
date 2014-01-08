using System.Collections.Generic;
using System.Web;
using ClientDependency.Core;
using ClientDependency.Core.FileRegistration.Providers;

namespace Umbraco.Web.UI.JavaScript
{
    /// <summary>
    /// A custom renderer that only outputs a dependency path instead of script tags - for use with the js loader with yepnope
    /// </summary>
    public class DependencyPathRenderer : StandardRenderer
    {
        public override string Name
        {
            get { return "Umbraco.DependencyPathRenderer"; }
        }

        /// <summary>
        /// Used to delimit each dependency so we can split later
        /// </summary>
        public const string Delimiter = "||||";

        /// <summary>
        /// Override because the StandardRenderer replaces & with &amp; but we don't want that so we'll reverse it
        /// </summary>
        /// <param name="allDependencies"></param>
        /// <param name="paths"></param>
        /// <param name="jsOutput"></param>
        /// <param name="cssOutput"></param>
        /// <param name="http"></param>
        public override void RegisterDependencies(List<IClientDependencyFile> allDependencies, HashSet<IClientDependencyPath> paths, out string jsOutput, out string cssOutput, HttpContextBase http)
        {
            base.RegisterDependencies(allDependencies, paths, out jsOutput, out cssOutput, http);

            jsOutput = jsOutput.Replace("&amp;", "&");
            cssOutput = cssOutput.Replace("&amp;", "&");
        }
        
        protected override string RenderSingleJsFile(string js, IDictionary<string, string> htmlAttributes)
        {
            return js + Delimiter;
        }

        protected override string RenderSingleCssFile(string css, IDictionary<string, string> htmlAttributes)
        {
            return css + Delimiter;
        }

    }
}