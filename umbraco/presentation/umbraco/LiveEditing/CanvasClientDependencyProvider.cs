using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClientDependency.Core.FileRegistration.Providers;
using umbraco.presentation;

namespace umbraco.presentation.LiveEditing
{
    public class CanvasClientDependencyProvider : LazyLoadProvider
    {
        public CanvasClientDependencyProvider()
            : base()
        {
            //Force this to always be debug mode!
            this.IsDebugMode = true;
        }

        public new const string DefaultName = "CanvasProvider";

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            base.Initialize(name, config);

            //Force this to always be debug mode!
            this.IsDebugMode = true;
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
