using System.Collections.Generic;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    public class UserPickerConfiguration : ConfigurationEditor
    {
        public override IDictionary<string, object> DefaultConfiguration => new Dictionary<string, object>
        {
            { "entityType", "User" },
            { "multiPicker", "0" }
        };
    }
}
