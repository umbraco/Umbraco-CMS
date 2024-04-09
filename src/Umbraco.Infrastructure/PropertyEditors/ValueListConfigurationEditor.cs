// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Pre-value editor used to create a list of items
/// </summary>
/// <remarks>
///     This pre-value editor is shared with editors like drop down, checkbox list, etc....
/// </remarks>
public class ValueListConfigurationEditor : ConfigurationEditor<ValueListConfiguration>
{
    public ValueListConfigurationEditor(IIOHelper ioHelper, IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        : base(ioHelper)
    {
        ConfigurationField items = Fields.First(x => x.Key == "items");
        items.Validators.Add(new ValueListUniqueValueValidator(configurationEditorJsonSerializer));
    }
}
