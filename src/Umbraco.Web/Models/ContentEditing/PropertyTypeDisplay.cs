using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "propertyType")]
    public class PropertyTypeDisplay
    {
        [DataMember(Name = "alias")]
        public string Alias { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "editor")]
        public string Editor { get; set; }

        [DataMember(Name = "validation")]
        public PropertyTypeValidation Validation { get; set; }

        [DataMember(Name = "label")]
        public string Label { get; set; }

        [DataMember(Name = "view")]
        public string View { get; set; }

        [DataMember(Name = "config")]
        public IDictionary<string, object> Config { get; set; }

        [DataMember(Name = "value")]
        public string Value { get; set; }
    }
}
