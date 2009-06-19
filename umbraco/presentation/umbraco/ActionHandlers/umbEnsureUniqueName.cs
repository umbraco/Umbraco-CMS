using System;

using umbraco.cms.businesslogic.web;
using umbraco.BusinessLogic.Actions;

namespace umbraco.ActionHandlers
{
	/// <summary>
	/// umbEnsureUniqueName is a standard Umbraco action handler.
    /// It ensures that new content nodes gets a unique name, and thereby avoiding conflictiong URLs.
    /// It can be disabled in the umbracoSettings.config file.
	/// </summary>
	public class umbEnsureUniqueName : umbraco.BusinessLogic.Actions.IActionHandler
	{
		public umbEnsureUniqueName()
		{
		}
		#region IActionHandler Members

        /// <summary>
        /// Actionhandler name
        /// </summary>
        /// <returns></returns>
		public string HandlerName()
		{
			return "umbEnsureUniqueName";
		}

        /// <summary>
        /// Executes on the current document object when the specified actions occur
        /// </summary>
        /// <param name="documentObject">The document object.</param>
        /// <param name="action">The action.</param>
        /// <returns>Returns true if successfull, otherwise false</returns>
        public bool Execute(umbraco.cms.businesslogic.web.Document documentObject, interfaces.IAction action)
        {
	    if (UmbracoSettings.EnsureUniqueNaming) 
	    {
		string currentName = documentObject.Text;
		int uniqueNumber = 1;

		// Check for all items underneath the parent to see if they match
		// as any new created documents are stored in the bottom, we can just
		// keep checking for other documents with a uniquenumber from 
		foreach(umbraco.BusinessLogic.console.IconI d in documentObject.Parent.Children) 
		{
            /*
			if (d.Id != documentObject.Id && d.getProperty("umbracoUrlName") != null) {
                currentName = documentObject.Text + " (" + uniqueNumber.ToString() + ")";
                uniqueNumber++;
            }
             */

            if (d.Id != documentObject.Id && d.Text.ToLower() == currentName.ToLower()) 
			{
                currentName = documentObject.Text + " (" + uniqueNumber.ToString() + ")";
				uniqueNumber++;
			}
            
		}

		// if name has been changed, update the documentobject
		if (currentName != documentObject.Text) 
		{
			// add name change to the log
			umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, umbraco.BusinessLogic.User.GetUser(0), documentObject.Id, "Title changed from '" + documentObject.Text + "' to '" + currentName + "'");

			documentObject.Text = currentName;
		
			return true;
		}
	}

	return false;
}

        /// <summary>
        /// Returns a collection of Iactions this handler reacts on.
        /// The umbEnsureUniqueName handler reacts on ActionNew() actions by default.
        /// </summary>
        /// <returns></returns>
		public interfaces.IAction[] ReturnActions()
		{
			interfaces.IAction[] _retVal = {ActionNew.Instance};
			return _retVal;
		}

		#endregion
	}
}
