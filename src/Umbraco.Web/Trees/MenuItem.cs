using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using umbraco.interfaces;

namespace Umbraco.Web.Trees
{
    [DataContract(Name = "menuItem", Namespace = "")]
    public class MenuItem
    {
        public MenuItem()
        {
            
        }

        public MenuItem(IAction legacyMenu)
        {
            Name = legacyMenu.Alias;
            Alias = legacyMenu.Alias;
            Seperator = false;
            Icon = legacyMenu.Icon;
        }

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
    }
}