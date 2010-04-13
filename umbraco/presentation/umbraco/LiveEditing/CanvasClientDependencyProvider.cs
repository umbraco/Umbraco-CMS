using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClientDependency.Core.FileRegistration.Providers;
using umbraco.presentation;
using ClientDependency.Core;

namespace umbraco.presentation.LiveEditing
{
    public class CanvasClientDependencyProvider : LazyLoadProvider
    {
        public CanvasClientDependencyProvider()
            : base()
        {}

        public new const string DefaultName = "CanvasProvider";

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            base.Initialize(name, config);            
        }

        /// <summary>
        /// override to never render out the dependency handler address (no compression, combination, etc...)
        /// </summary>
        /// <param name="cssDependencies"></param>
        /// <returns></returns>
        protected override string RenderCssDependencies(List<ClientDependency.Core.IClientDependencyFile> cssDependencies)
        {
            if (cssDependencies.Count == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            foreach (IClientDependencyFile dependency in cssDependencies)
            {
                sb.Append(RenderSingleCssFile(dependency.FilePath));
            }

            return sb.ToString();
        }

        /// <summary>
        /// override to never render out the dependency handler address (no compression, combination, etc...)
        /// </summary>
        /// <param name="jsDependencies"></param>
        /// <returns></returns>
        protected override string RenderJsDependencies(List<ClientDependency.Core.IClientDependencyFile> jsDependencies)
        {
            if (jsDependencies.Count == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            foreach (IClientDependencyFile dependency in jsDependencies)
            {
                sb.Append(RenderSingleJsFile(string.Format("'{0}','{1}'", dependency.FilePath, string.Empty)));
            }

            return sb.ToString();
        }

        protected override string RenderSingleCssFile(string css)
        {
            if (UmbracoContext.Current.LiveEditingContext.Enabled)
                return base.RenderSingleCssFile(css);
            return string.Empty;
        }

        protected override string RenderSingleJsFile(string js)
        {
            if (UmbracoContext.Current.LiveEditingContext.Enabled)
                return base.RenderSingleJsFile(js);
            return string.Empty;
        }


    }
}
