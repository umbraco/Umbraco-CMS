﻿using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors.ParameterEditors
{
    [DataEditor(
        "tabPicker",
        EditorType.MacroParameter,
        "Tab Picker",
        "entitypicker")]
    public class PropertyGroupParameterEditor : DataEditor
    {
        public PropertyGroupParameterEditor(
            IDataValueEditorFactory dataValueEditorFactory)
            : base(dataValueEditorFactory)
        {
            // configure
            DefaultConfiguration.Add("multiple", "0");
            DefaultConfiguration.Add("entityType", "PropertyGroup");
            //don't publish the id for a property group, publish it's alias (which is actually just it's lower cased name)
            DefaultConfiguration.Add("publishBy", "alias");
        }
    }
}
