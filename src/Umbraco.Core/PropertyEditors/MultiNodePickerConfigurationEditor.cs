// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the multinode picker value editor.
/// </summary>
public class MultiNodePickerConfigurationEditor : ConfigurationEditor<MultiNodePickerConfiguration>
{
    // Scheduled for removal in v12
    [Obsolete("Please use constructor that takes an IEditorConfigurationParser instead")]
    public MultiNodePickerConfigurationEditor(IIOHelper ioHelper)
        : this(ioHelper, StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
    {
    }

    public MultiNodePickerConfigurationEditor(IIOHelper ioHelper, IEditorConfigurationParser editorConfigurationParser)
        : base(ioHelper, editorConfigurationParser) =>
        Field(nameof(MultiNodePickerConfiguration.TreeSource))
            .Config = new Dictionary<string, object> { { "idType", "udi" } };

    public override IDictionary<string, object> ToConfigurationEditor(IDictionary<string, object> configuration)
    {
        IDictionary<string, object> config = base.ToConfigurationEditor(configuration);
        // TODO: this belongs on the client side!
        if (config.TryGetValue("maxNumber", out var maxNumberValue)
            && int.TryParse(maxNumberValue.ToString(), out var maxNumber)
            && maxNumber > 1)
        {
            config["multiPicker"] = true;
        }

        return config;
    }

    public override IDictionary<string, object> ToValueEditor(IDictionary<string, object> configuration)
    {
        IDictionary<string, object> config = base.ToValueEditor(configuration);
        // TODO: this belongs on the client side!
        config["multiPicker"] = true;
        config["showEditButton"] = false;
        config["showPathOnHover"] = false;
        config["idType"] = "udi";
        return config;
    }
}
