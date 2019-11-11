using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "mediaReferences", Namespace = "")]
    public class MediaReferences
    {
        [DataMember(Name = "content")]
        public IEnumerable<EntityTypeReferences> Content { get; set; } = Enumerable.Empty<EntityTypeReferences>();

        [DataMember(Name = "members")]
        public IEnumerable<EntityTypeReferences> Members { get; set; } = Enumerable.Empty<EntityTypeReferences>();

        [DataMember(Name = "media")]
        public IEnumerable<EntityTypeReferences> Media { get; set; } = Enumerable.Empty<EntityTypeReferences>();

        [DataContract(Name = "entityType", Namespace = "")]
        public class EntityTypeReferences : EntityBasic
        {
        }
    }
}
