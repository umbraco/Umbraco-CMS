using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "propertyType")]
    public class PropertyTypeDisplay : PropertyTypeBasic
    {  
        [DataMember(Name = "editor")]
        [ReadOnly(true)]
        public string Editor { get; set; }
        
        [DataMember(Name = "view")]
        [ReadOnly(true)]
        public string View { get; set; }

        [DataMember(Name = "config")]
        [ReadOnly(true)]
        public IDictionary<string, object> Config { get; set; }
        
        //SD: Seems strange that this is needed
        [DataMember(Name = "contentTypeId")]
        [ReadOnly(true)]
        public int ContentTypeId { get; set; }

        //SD: Seems strange that this is needed
        [DataMember(Name = "contentTypeName")]
        [ReadOnly(true)]
        public string ContentTypeName { get; set; }
    }
}
