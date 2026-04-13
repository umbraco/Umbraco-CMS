// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

internal sealed class DropDownFlexibleConfigurationEditor : ConfigurationEditor<DropDownFlexibleConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DropDownFlexibleConfigurationEditor"/> class.
    /// </summary>
    /// <param name="ioHelper">An <see cref="IIOHelper"/> used for file and path operations.</param>
    /// <param name="configurationEditorJsonSerializer">An <see cref="IConfigurationEditorJsonSerializer"/> used to serialize and deserialize configuration editor data as JSON.</param>
    public DropDownFlexibleConfigurationEditor(IIOHelper ioHelper, IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        : base(ioHelper)
    {
        ConfigurationField items = Fields.First(x => x.Key == "items");
        items.Validators.Add(new ValueListUniqueValueValidator(configurationEditorJsonSerializer));
    }
}
