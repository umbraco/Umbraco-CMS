// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.PropertyEditors;

internal class ContentPickerConfigurationEditor : ConfigurationEditor<ContentPickerConfiguration>
{
    public ContentPickerConfigurationEditor(IIOHelper ioHelper, IEditorConfigurationParser editorConfigurationParser)
        : base(ioHelper, editorConfigurationParser) =>

        // configure fields
        // this is not part of ContentPickerConfiguration,
        // but is required to configure the UI editor (when editing the configuration)
        Field(nameof(ContentPickerConfiguration.StartNodeId))
            .Config = new Dictionary<string, object> { { "idType", "udi" } };

    public override IDictionary<string, object> ToValueEditor(object? configuration)
    {
        // get the configuration fields
        IDictionary<string, object> d = base.ToValueEditor(configuration);

        // add extra fields
        // not part of ContentPickerConfiguration but used to configure the UI editor
        d["showEditButton"] = false;
        d["showPathOnHover"] = false;
        d["idType"] = "udi";

        return d;
    }
}
