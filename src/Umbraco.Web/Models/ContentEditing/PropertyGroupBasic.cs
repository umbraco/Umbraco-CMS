using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "propertyGroup", Namespace = "")]
    public abstract class PropertyGroupBasic
    {
        /// <summary>
        /// Gets the special generic properties tab identifier.
        /// </summary>
        public const int GenericPropertiesGroupId = -666;

        /// <summary>
        /// Gets a value indicating whether this tab is the generic properties tab.
        /// </summary>
        [IgnoreDataMember]
        public bool IsGenericProperties { get { return Id == GenericPropertiesGroupId; } }

        /// <summary>
        /// Gets a value indicating whether the property group is inherited through
        /// content types composition.
        /// </summary>
        /// <remarks>A property group can be inherited and defined on the content type
        /// currently being edited, at the same time. Inherited is true when there exists at least
        /// one property group higher in the composition, with the same alias.</remarks>
        [DataMember(Name = "inherited")]
        public bool Inherited { get; set; }

        // needed - so we can handle alias renames
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "sortOrder")]
        public int SortOrder { get; set; }

        [Required]
        [DataMember(Name = "name")]
        public string Name { get; set; }
    }

    [DataContract(Name = "propertyGroup", Namespace = "")]
    public class PropertyGroupBasic<TPropertyType> : PropertyGroupBasic
        where TPropertyType: PropertyTypeBasic
    {
        public PropertyGroupBasic()
        {
            Properties = new List<TPropertyType>();
        }

        

        [DataMember(Name = "properties")]
        public IEnumerable<TPropertyType> Properties { get; set; }

       
    }
}