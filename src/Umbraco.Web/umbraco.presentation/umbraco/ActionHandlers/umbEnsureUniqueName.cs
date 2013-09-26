using System;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using umbraco.cms.businesslogic.web;
using umbraco.BusinessLogic.Actions;

namespace umbraco.ActionHandlers
{
    /// <summary>
    /// umbEnsureUniqueName is a standard Umbraco action handler.
    /// It ensures that new content nodes gets a unique name, and thereby avoiding conflictiong URLs.
    /// It can be disabled in the umbracoSettings.config file.
    /// </summary>
    [Obsolete("This handler is no longer used")]
    public class umbEnsureUniqueName : IActionHandler
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
            
            if (UmbracoConfig.For.UmbracoSettings().Content.EnsureUniqueNaming)
            {
                string currentName = documentObject.Text;
                int uniqueNumber = 1;

                // Check for all items underneath the parent to see if they match
                // as any new created documents are stored in the bottom, we can just
                // keep checking for other documents with a uniquenumber from 

                //store children array here because iterating over an Array property object is very inneficient.
                var c = Document.GetChildrenBySearch(documentObject.ParentId, currentName + "%");

                // must sort the list or else duplicate name will exist if pages are out out sequence
                //e.g. Page (1), Page (3), Page (2)
                var results = c.OrderBy(x => x.Text, new SimilarNodeNameComparer());
                foreach (Document d in results)
                {
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
					LogHelper.Debug<umbEnsureUniqueName>("Title changed from '" + documentObject.Text + "' to '" + currentName + "' for document  id" + documentObject.Id);
                    
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
            interfaces.IAction[] _retVal = {  };
            return _retVal;
        }

        #endregion
    }
}
