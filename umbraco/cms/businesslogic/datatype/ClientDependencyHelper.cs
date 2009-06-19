using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web;

namespace umbraco.cms.businesslogic.datatype
{
    public class ClientDependencyHelper
    {
        /// <summary>Path to the dependency loader we need for adding control dependencies.</summary>
        protected const string DependencyLoaderScriptFile = "{0}/js/UmbracoDependencyLoader.js";

        /// <summary>
        /// Adds the client dependencies to the passed page's client script manager.
        /// </summary>
        public static void AddClientDependencies(Control control) {
            Type controlType = control.GetType();

            // find dependencies
            List<ClientDependencyAttribute> dependencies = new List<ClientDependencyAttribute>();
            foreach (Attribute attribute in Attribute.GetCustomAttributes(controlType))
            {
                if (attribute is ClientDependencyAttribute)
                {
                    dependencies.Add((ClientDependencyAttribute)attribute);
                }
            }
            // sort by priority
            dependencies.Sort((a, b) => a.Priority.CompareTo(b.Priority));

            if (dependencies.Count > 0)
            {
                // register loader script
                if (!HttpContext.Current.Items.Contains(DependencyLoaderScriptFile))
                {
                    ScriptManager.RegisterClientScriptInclude(control, controlType, "DependencyLoader",
                                                              String.Format(DependencyLoaderScriptFile, GlobalSettings.Path));
                    HttpContext.Current.Items[DependencyLoaderScriptFile] = true;
                }

                // create Javascript calls
                StringBuilder dependencyCalls = new StringBuilder("UmbDependencyLoader");
                foreach (ClientDependencyAttribute dependency in dependencies)
                {
                    switch (dependency.DependencyType)
                    {
                        case ClientDependencyType.Css:
                            dependencyCalls.AppendFormat(".AddCss('{0}')", dependency.FilePath);
                            break;
                        case ClientDependencyType.Javascript:
                            dependencyCalls.AppendFormat(".AddJs('{0}','{1}')",
                                                         dependency.FilePath, dependency.InvokeJavascriptMethodOnLoad);
                            break;
                    }
                }
                dependencyCalls.Append(';');

                // register Javascript calls
                ScriptManager.RegisterClientScriptBlock(control, controlType, new Guid().ToString(),
                                                        dependencyCalls.ToString(), true);
            }

            // add child dependencies
            foreach (Control child in control.Controls)
            {
                AddClientDependencies(child);
            }
        }
    }
}
