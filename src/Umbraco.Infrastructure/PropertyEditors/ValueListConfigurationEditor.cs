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
    /// <summary>
    /// Initializes a new instance of the <see cref="ValueListConfigurationEditor"/> class.
    /// </summary>
    /// <param name="ioHelper">An <see cref="IIOHelper"/> instance used for file and path operations.</param>
    /// <param name="configurationEditorJsonSerializer">An <see cref="IConfigurationEditorJsonSerializer"/> used to serialize and deserialize configuration editor values as JSON.</param>
    public ValueListConfigurationEditor(IIOHelper ioHelper, IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        : base(ioHelper)
    {
        ConfigurationField items = Fields.First(x => x.Key == "items");
        items.Validators.Add(new ValueListUniqueValueValidator(configurationEditorJsonSerializer));
    }
}
