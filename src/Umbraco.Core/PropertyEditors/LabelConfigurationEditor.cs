// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;

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

    /// <inheritdoc />
    public override LabelConfiguration FromConfigurationEditor(
        IDictionary<string, object?>? editorValues,
        LabelConfiguration? configuration)
    {
        var newConfiguration = new LabelConfiguration();

        // get the value type
        // not simply deserializing Json because we want to validate the valueType
        if (editorValues is not null && editorValues.TryGetValue(
                                         Constants.PropertyEditors.ConfigurationKeys.DataValueType,
                                         out var valueTypeObj)
                                     && valueTypeObj is string stringValue)
        {
            // validate
            if (!string.IsNullOrWhiteSpace(stringValue) && ValueTypes.IsValue(stringValue))
            {
                newConfiguration.ValueType = stringValue;
            }
        }

        return newConfiguration;
    }
}
