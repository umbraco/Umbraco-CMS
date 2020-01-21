using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// Used to unpublish content and variants
    /// </summary>
    [DataContract(Name = "unpublish", Namespace = "")]
    public class UnpublishContent
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "cultures")]
        public string[] Cultures { get; set; }
    }
}
