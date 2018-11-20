using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "relationType", Namespace = "")]
    public class RelationTypeSave : EntityBasic
    {
        /// <summary>
        /// Gets or sets a boolean indicating whether the RelationType is Bidirectional (true) or Parent to Child (false)
        /// </summary>
        [DataMember(Name = "isBidirectional", IsRequired = true)]
        public bool IsBidirectional { get; set; }
    }
}
