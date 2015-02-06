using Umbraco.Core.CodeAnnotations;
using umbraco.BusinessLogic;

namespace umbraco
{
    /// <summary>
    /// The UI 'tasks' for the create dialog and delete processes
    /// </summary>
    [UmbracoWillObsolete("http://issues.umbraco.org/issue/U4-1373", "This will one day be removed when we overhaul the create process")]
    public class PartialViewMacroTasks : PartialViewTasksBase
    {        
        public override string AssignedApp
        {
            get { return DefaultApps.developer.ToString(); }
        }

        protected override bool IsPartialViewMacro
        {
            get { return true; }
        }
    }
}