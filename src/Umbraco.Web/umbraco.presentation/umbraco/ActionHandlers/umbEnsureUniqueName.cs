using System;
using System.Collections.Generic;
using System.Linq;

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
            interfaces.IAction[] _retVal = { ActionNew.Instance };
            return _retVal;
        }

        #endregion
    }

    /// <summary>
    /// Comparer that takes into account the duplicate index of a node name
    /// This is needed as a normal alphabetic sort would go Page (1), Page (10), Page (2) etc.
    /// </summary>
    public class SimilarNodeNameComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x.LastIndexOf(')') == x.Length - 1 && y.LastIndexOf(')') == y.Length - 1)
            {
                if (x.ToLower().Substring(0, x.LastIndexOf('(')) == y.ToLower().Substring(0, y.LastIndexOf('(')))
                {
                    int xDuplicateIndex = ExtractDuplicateIndex(x);
                    int yDuplicateIndex = ExtractDuplicateIndex(y);

                    if (xDuplicateIndex != 0 && yDuplicateIndex != 0)
                    {
                        return xDuplicateIndex.CompareTo(yDuplicateIndex);
                    }                    
                }
            }
            return x.ToLower().CompareTo(y.ToLower());                   
        }

        private int ExtractDuplicateIndex(string text)
        {
            int index = 0;

            if (text.LastIndexOf('(') != -1 && text.LastIndexOf('(') < text.Length - 2)
            {
                int startPos = text.LastIndexOf('(') + 1;
                int length = text.Length - 1 - startPos;

                int.TryParse(text.Substring(startPos, length), out index);
            }

            return index;
        }
    }
}
