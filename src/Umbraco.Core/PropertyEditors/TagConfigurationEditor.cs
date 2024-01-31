// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors.Validators;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration editor for the tag value editor.
/// </summary>
public class TagConfigurationEditor : ConfigurationEditor<TagConfiguration>
{
    [Obsolete($"Use the constructor that does not accept {nameof(ILocalizedTextService)}. Will be removed in V15.")]
    public TagConfigurationEditor(ManifestValueValidatorCollection validators, IIOHelper ioHelper, ILocalizedTextService localizedTextService, IEditorConfigurationParser editorConfigurationParser)
        : this(ioHelper, editorConfigurationParser)
    {
    }

    public TagConfigurationEditor(IIOHelper ioHelper, IEditorConfigurationParser editorConfigurationParser)
        : base(ioHelper, editorConfigurationParser)
    {
        Field(nameof(TagConfiguration.Group)).Validators.Add(new RequiredValidator());
        Field(nameof(TagConfiguration.StorageType)).Validators.Add(new RequiredValidator());
    }

    public override IDictionary<string, object> ToConfigurationEditor(IDictionary<string, object> configuration)
    {
        IDictionary<string, object> config = base.ToConfigurationEditor(configuration);

        // the front-end editor expects the string value of the storage type
        // TODO: this (default value) belongs on the client side!
        if (!config.ContainsKey("storageType"))
        {
            config["storageType"] = TagsStorageType.Json.ToString();
        }

        return config;
    }
}
