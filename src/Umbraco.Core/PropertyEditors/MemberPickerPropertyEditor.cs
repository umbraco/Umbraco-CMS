﻿using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors
{
    [DataEditor(
        Constants.PropertyEditors.Aliases.MemberPicker,
        "Member Picker",
        "memberpicker",
        ValueType = ValueTypes.String,
        Group = Constants.PropertyEditors.Groups.People,
        Icon = Constants.Icons.Member)]
    public class MemberPickerPropertyEditor : DataEditor
    {
        public MemberPickerPropertyEditor(
            IDataValueEditorFactory dataValueEditorFactory)
            : base(dataValueEditorFactory)
        { }

        protected override IConfigurationEditor CreateConfigurationEditor() => new MemberPickerConfiguration();
    }
}
