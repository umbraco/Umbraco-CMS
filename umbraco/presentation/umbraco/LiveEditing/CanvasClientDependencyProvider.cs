using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClientDependency.Core;
using ClientDependency.Core.FileRegistration.Providers;
using umbraco.presentation;

namespace umbraco.presentation.LiveEditing
{
    public class CanvasClientDependencyProvider : LazyLoadProvider
    {
        public new const string DefaultName = "CanvasProvider";

        /// <summary>
        /// override to never render out the dependency handler address (no compression, combination, etc...)
        /// </summary>
        /// <param name="jsDependencies"></param>
        /// <param name="http"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        protected override string RenderJsDependencies(IEnumerable<IClientDependencyFile> jsDependencies, System.Web.HttpContextBase http, IDictionary<string, string> htmlAttributes)
        {
            if (!jsDependencies.Any())
                return string.Empty;

            var sb = new StringBuilder();

            foreach (var dependency in jsDependencies)
            {
                sb.Append(RenderSingleJsFile(string.Format("'{0}','{1}'", dependency.FilePath, string.Empty), new Dictionary<string, string>()));
            }

            return sb.ToString();
        }

        /// <summary>
        /// override to never render out the dependency handler address (no compression, combination, etc...)
        /// </summary>
        /// <param name="cssDependencies"></param>
        /// <param name="http"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        protected override string RenderCssDependencies(IEnumerable<IClientDependencyFile> cssDependencies, System.Web.HttpContextBase http, IDictionary<string, string> htmlAttributes)
        {
            if (!cssDependencies.Any())
                return string.Empty;

            var sb = new StringBuilder();

            foreach (var dependency in cssDependencies)
            {
                sb.Append(RenderSingleCssFile(dependency.FilePath, new Dictionary<string, string>()));
            }

            return sb.ToString();
        }

        protected override string RenderSingleCssFile(string css, IDictionary<string, string> htmlAttributes)
        {
            if (UmbracoContext.Current.LiveEditingContext.Enabled)
                return base.RenderSingleCssFile(css, new Dictionary<string, string>());
            return string.Empty;
        }

        protected override string RenderSingleJsFile(string js, IDictionary<string, string> htmlAttributes)
        {
            if (UmbracoContext.Current.LiveEditingContext.Enabled)
                return base.RenderSingleJsFile(js, new Dictionary<string, string>());
            return string.Empty;
        }

    }
}