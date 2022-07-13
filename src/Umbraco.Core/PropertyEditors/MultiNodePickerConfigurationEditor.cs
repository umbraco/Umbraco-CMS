// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;

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

    /// <inheritdoc />
    public override Dictionary<string, object> ToConfigurationEditor(MultiNodePickerConfiguration? configuration)
    {
        // sanitize configuration
        Dictionary<string, object> output = base.ToConfigurationEditor(configuration);

        output["multiPicker"] = configuration?.MaxNumber > 1;

        return output;
    }

    /// <inheritdoc />
    public override IDictionary<string, object> ToValueEditor(object? configuration)
    {
        IDictionary<string, object> d = base.ToValueEditor(configuration);
        d["multiPicker"] = true;
        d["showEditButton"] = false;
        d["showPathOnHover"] = false;
        d["idType"] = "udi";
        return d;
    }
}
