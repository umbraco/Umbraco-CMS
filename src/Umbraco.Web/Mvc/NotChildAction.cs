using System.Web.Mvc;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// Used to ensure that actions with duplicate names that are not child actions don't get executed when 
    /// we are Posting and not redirecting.
    /// </summary>
    /// <remarks>
    /// See issue: http://issues.umbraco.org/issue/U4-1819
    /// </remarks>
    public class NotChildAction : ActionMethodSelectorAttribute
    {
        public override bool IsValidForRequest(ControllerContext controllerContext, System.Reflection.MethodInfo methodInfo)
        {
            var isChildAction = controllerContext.IsChildAction;
            return !isChildAction;
        }
    }
}