﻿using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors.ParameterEditors
{
    [DataEditor(
        "tabPickerMultiple",
        EditorType.MacroParameter,
        "Multiple Tab Picker",
        "entitypicker")]
    public class MultiplePropertyGroupParameterEditor : DataEditor
    {
        public MultiplePropertyGroupParameterEditor(
            IDataValueEditorFactory dataValueEditorFactory)
            : base(dataValueEditorFactory)
        {
            // configure
            DefaultConfiguration.Add("multiple", true);
            DefaultConfiguration.Add("entityType", "PropertyGroup");
            //don't publish the id for a property group, publish its alias, which is actually just its lower cased name
            DefaultConfiguration.Add("publishBy", "alias");
        }
    }
}
