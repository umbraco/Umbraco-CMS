using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using umbraco.interfaces;
using System.Collections.Generic;

namespace Umbraco.Web.Trees.Menu
{
    /// <summary>
    /// A context menu item
    /// </summary>
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
            SeperatorBefore = false;
            Icon = legacyMenu.Icon;
            Action = legacyMenu;
        }

        internal IAction Action { get; set; }

        /// <summary>
        /// A dictionary to support any additional meta data that should be rendered for the node which is 
        /// useful for custom action commands such as 'create', 'copy', etc...
        /// </summary>
        /// <remarks>
        /// We will also use the meta data collection for dealing with legacy menu items (i.e. for loading custom URLs or
        /// executing custom JS).
        /// </remarks>
        [DataMember(Name = "metaData")]
        public Dictionary<string, object> AdditionalData { get; private set; }

        [DataMember(Name = "name", IsRequired = true)]
        [Required]
        public string Name { get; set; }

        [DataMember(Name = "alias", IsRequired = true)]
        [Required]
        public string Alias { get; set; }

        /// <summary>
        /// Ensures a menu separator will exist before this menu item
        /// </summary>
        [DataMember(Name = "seperator")]
        public bool SeperatorBefore { get; set; }

        [DataMember(Name = "cssclass")]
        public string Icon { get; set; }

    }
}