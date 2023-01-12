// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the label value editor.
/// </summary>
public class LabelConfigurationEditor : ConfigurationEditor<LabelConfiguration>
{
    // Scheduled for removal in v12
    [Obsolete("Please use constructor that takes and IEditorConfigurationParser instead")]
    public LabelConfigurationEditor(IIOHelper ioHelper)
        : this(ioHelper, StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
    {
    }

    public LabelConfigurationEditor(IIOHelper ioHelper, IEditorConfigurationParser editorConfigurationParser)
        : base(ioHelper, editorConfigurationParser)
    {
    }

    public override IDictionary<string, object> FromConfigurationEditor(IDictionary<string, object> configuration)
    {
        // default value
        var valueType = new LabelConfiguration().ValueType;

        // get the value type
        // not simply deserializing Json because we want to validate the valueType
        if (configuration.TryGetValue(
                Constants.PropertyEditors.ConfigurationKeys.DataValueType,
                out var valueTypeObj)
            && valueTypeObj is string stringValue)
        {
            // validate
            if (!string.IsNullOrWhiteSpace(stringValue) && ValueTypes.IsValue(stringValue))
            {
                valueType = stringValue;
            }
        }

        configuration[Constants.PropertyEditors.ConfigurationKeys.DataValueType] = valueType;

        return base.FromConfigurationEditor(configuration);
    }
}
