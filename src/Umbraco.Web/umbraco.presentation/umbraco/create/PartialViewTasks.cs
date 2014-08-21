using Umbraco.Core.CodeAnnotations;
using umbraco.BusinessLogic;

namespace umbraco
{
    /// <summary>
    /// The UI 'tasks' for the create dialog and delete processes
    /// </summary>
    [UmbracoWillObsolete("http://issues.umbraco.org/issue/U4-1373", "This will one day be removed when we overhaul the create process")]
    public class PartialViewTasks : PartialViewTasksBase
    {
        protected override string CodeHeader
        {
            get { return "@inherits Umbraco.Web.Mvc.UmbracoTemplatePage"; }
        }

        protected override string ParentFolderName
        {
            get { return "Partials"; }
        }
        
        public override string AssignedApp
        {
            get { return DefaultApps.settings.ToString(); }
        }
    }
}
