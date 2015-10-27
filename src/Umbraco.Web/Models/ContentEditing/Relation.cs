using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "relation", Namespace = "")]
    public class Relation
    {

        public Relation()
        {
            RelationType = new RelationType();
        }

        /// <summary>
        /// Gets or sets the Parent Id of the Relation (Source)
        /// </summary>
        [DataMember(Name = "parentId")]
        public int ParentId { get; set; }

        /// <summary>
        /// Gets or sets the Child Id of the Relation (Destination)
        /// </summary>
        [DataMember(Name = "childId")]
        public int ChildId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="RelationType"/> for the Relation
        /// </summary>
        [DataMember(Name = "relationType", IsRequired = true)]
        public RelationType RelationType { get; set; }

        /// <summary>
        /// Gets or sets a comment for the Relation
        /// </summary>
        [DataMember(Name = "comment")]
        public string Comment { get; set; }

    }
}
