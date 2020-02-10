using System.Collections.Generic;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    public class MemberPickerConfiguration : ConfigurationEditor
    {
        [ConfigurationField("multiPicker", "Pick multiple items", "boolean")]
        public bool Multiple { get; set; }

        public override IDictionary<string, object> DefaultConfiguration => new Dictionary<string, object>
        {
            { "idType", "udi" }
        };
    }
}
