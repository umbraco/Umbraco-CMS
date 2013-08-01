using System;
using Umbraco.Core;

namespace Umbraco.Web.Trees.Menu
{
    /// <summary>
    /// A menu item that represents some JS that needs to execute when the menu item is clicked.
    /// </summary>
    /// <remarks>
    /// These types of menu items are rare but they do exist. Things like refresh node simply execute
    /// JS and don't launch a dialog.
    /// 
    /// Each action menu item describes what angular service that it's method exists in and what the method name is
    /// </remarks>
    public abstract class ActionMenuItem : MenuItem
    {
        protected ActionMenuItem()
            : base()
        {
            var attribute = GetType().GetCustomAttribute<ActionMenuItemAttribute>(false);
            if (attribute == null)
            {
                throw new InvalidOperationException("All " + typeof (ActionMenuItem).FullName + " instances must be attributed with " + typeof (ActionMenuItemAttribute).FullName);
            }

            //add the current type to the metadata
            AdditionalData.Add("jsAction", string.Format("{0}.{1}", attribute.ServiceName, attribute.MethodName));
        }
    }
}