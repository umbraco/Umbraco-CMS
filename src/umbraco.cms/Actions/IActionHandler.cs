using System;
using umbraco.cms.businesslogic.web;
using umbraco.interfaces;

namespace umbraco.BusinessLogic.Actions
{
    /// <summary>
    /// Implement the IActionHandler interface in order to automatically get code
    /// run whenever a document, member or media changed, deleted, created etc.
    /// The Clases implementing IActionHandler are loaded at runtime which means
    /// that there are no other setup when creating a custom actionhandler.
    /// </summary>
    /// <example>
    /// 
    /// </example>
    [Obsolete("Legacy! Use events instead")]
    public interface IActionHandler
    {
        bool Execute(Document documentObject, IAction action);
        IAction[] ReturnActions();
        string HandlerName();
    }
}
