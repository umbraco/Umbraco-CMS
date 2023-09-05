// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the label value editor.
/// </summary>
public class NoopStringConfigurationEditor : ConfigurationEditor<NoopStringValueTypeConfiguration>
{
    public NoopStringConfigurationEditor(IIOHelper ioHelper, IEditorConfigurationParser editorConfigurationParser)
        : base(ioHelper, editorConfigurationParser)
    {
    }

    public override IDictionary<string, object> FromConfigurationEditor(IDictionary<string, object> configuration)
    {
        return new Dictionary<string, object>();
    }

    public override object ToConfigurationObject(IDictionary<string, object> configuration,
        IConfigurationEditorJsonSerializer configurationEditorJsonSerializer) => new NoopStringValueTypeConfiguration();
}
