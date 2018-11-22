using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.Trees
{
    /// <summary>
    /// A menu item collection for a given tree node
    /// </summary>
    [DataContract(Name = "menuItems", Namespace = "")]
    public class MenuItemCollection 
    {
        private readonly MenuItemList _menuItems = new MenuItemList();

        public MenuItemCollection()
        {           
        }

        public MenuItemCollection(IEnumerable<MenuItem> items)
        {
            _menuItems = new MenuItemList(items);
        }
        
        /// <summary>
        /// Sets the default menu item alias to be shown when the menu is launched - this is optional and if not set then the menu will just be shown normally.
        /// </summary>
        [DataMember(Name = "defaultAlias")]
        public string DefaultMenuAlias { get; set; }

        /// <summary>
        /// The list of menu items
        /// </summary>
        /// <remarks>
        /// We require this so the json serialization works correctly
        /// </remarks>
        [DataMember(Name = "menuItems")]
        public MenuItemList Items
        {
            get { return _menuItems; }
        }

        

    }
}