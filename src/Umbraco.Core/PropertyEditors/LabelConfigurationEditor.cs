// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the label value editor.
/// </summary>
public class LabelConfigurationEditor : ConfigurationEditor<LabelConfiguration>
{
    public LabelConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
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
