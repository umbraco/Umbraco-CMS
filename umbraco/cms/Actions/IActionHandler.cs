using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Reflection;
using umbraco.BasePages;
using umbraco.BusinessLogic.Utils;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.workflow;
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
	public interface IActionHandler
	{
		bool Execute(Document documentObject, IAction action);
		IAction[] ReturnActions();
		string HandlerName();
	}
}
