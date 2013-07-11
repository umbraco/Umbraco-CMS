using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using umbraco.interfaces;
using System.Collections.Generic;

namespace Umbraco.Web.Trees
{
    [DataContract(Name = "menuItem", Namespace = "")]
    public class MenuItem
    {
        public MenuItem()
        {
            AdditionalData = new Dictionary<string, object>();
        }

        public MenuItem(IAction legacyMenu)
            : this()
        {
            Name = legacyMenu.Alias;
            Alias = legacyMenu.Alias;
            Seperator = false;
            Icon = legacyMenu.Icon;
        }

        /// <summary>
        /// A dictionary to support any additional meta data that should be rendered for the node which is 
        /// useful for custom action commands such as 'create', 'copy', etc...
        /// </summary>
        /// <remarks>
        /// We will also use the meta data collection for dealing with legacy menu items (i.e. for loading custom URLs or
        /// executing custom JS)
        /// </remarks>
        [DataMember(Name = "metaData")]
        public Dictionary<string, object> AdditionalData { get; private set; }

        [DataMember(Name = "name", IsRequired = true)]
        [Required]
        public string Name { get; set; }

        [DataMember(Name = "alias", IsRequired = true)]
        [Required]
        public string Alias { get; set; }

        [DataMember(Name = "seperator")]
        public bool Seperator { get; set; }

        [DataMember(Name = "cssclass")]
        public string Icon { get; set; }

        /// <summary>
        /// The action to execute when the menu item is clicked. This is generally a route path.
        /// </summary>
        [DataMember(Name = "action")]
        public string Action { get; set; }

        
    }

    public static class MenuItemExtensions
    {
        /// <summary>
        /// Used as a key for the AdditionalData to specify a specific dialog title instead of the menu title
        /// </summary>
        internal const string DialogTitleKey = "dialogTitle";
        internal const string ActionUrlKey = "actionUrl";
        internal const string ActionUrlMethodKey = "actionUrlMethod";

        /// <summary>
        /// Puts a dialog title into the meta data to be displayed on the dialog of the menu item (if there is one)
        /// instead of the menu name
        /// </summary>
        /// <param name="menuItem"></param>
        /// <param name="dialogTitle"></param>
        public static void SetDialogTitle(this MenuItem menuItem, string dialogTitle)
        {
            menuItem.AdditionalData[DialogTitleKey] = dialogTitle;
        }

        /// <summary>
        /// Configures the menu item to launch a URL with the specified action (dialog or new window)
        /// </summary>
        /// <param name="menuItem"></param>
        /// <param name="url"></param>
        /// <param name="method"></param>
        public static void SetActionUrl(this MenuItem menuItem, string url, ActionUrlMethod method = ActionUrlMethod.Dialog)
        {
            menuItem.AdditionalData[ActionUrlKey] = url;
            menuItem.AdditionalData[ActionUrlMethodKey] = url;
        }
    }

    /// <summary>
    /// Specifies the action to take for a menu item when a URL is specified
    /// </summary>
    public enum ActionUrlMethod
    {
        Dialog,
        BlankWindow
    }
}