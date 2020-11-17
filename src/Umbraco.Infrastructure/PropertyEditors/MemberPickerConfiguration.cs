﻿using System.Collections.Generic;
using Umbraco.Core;

namespace Umbraco.Web.PropertyEditors
{
    public class MemberPickerConfiguration : ConfigurationEditor
    {
        public override IDictionary<string, object> DefaultConfiguration => new Dictionary<string, object>
        {
            { "idType", "udi" }
        };
    }
}
