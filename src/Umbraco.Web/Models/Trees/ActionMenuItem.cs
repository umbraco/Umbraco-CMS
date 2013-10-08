using System;
using Umbraco.Core;

namespace Umbraco.Web.Models.Trees
{
    /// <summary>
    /// A menu item that represents some JS that needs to execute when the menu item is clicked.
    /// </summary>
    /// <remarks>
    /// These types of menu items are rare but they do exist. Things like refresh node simply execute
    /// JS and don't launch a dialog.
    /// 
    /// Each action menu item describes what angular service that it's method exists in and what the method name is.
    /// 
    /// An action menu item must describe the angular service name for which it's method exists. It may also define what the 
    /// method name is that will be called in this service but if one is not specified then we will assume the method name is the
    /// same as the Type name of the current action menu class.
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
            if (attribute.MethodName.IsNullOrWhiteSpace())
            {
                //if no method name is supplied we will assume that the menu action is the type name of the current menu class
                AdditionalData.Add("jsAction", string.Format("{0}.{1}", attribute.ServiceName, this.GetType().Name));
            }
            else
            {
                AdditionalData.Add("jsAction", string.Format("{0}.{1}", attribute.ServiceName, attribute.MethodName));    
            }
        }
    }
}