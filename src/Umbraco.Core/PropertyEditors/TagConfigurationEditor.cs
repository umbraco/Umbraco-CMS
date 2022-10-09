// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors.Validators;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration editor for the tag value editor.
/// </summary>
public class TagConfigurationEditor : ConfigurationEditor<TagConfiguration>
{
    // Scheduled for removal in v12
    [Obsolete("Please use constructor that takes an IEditorConfigurationParser instead")]
    public TagConfigurationEditor(ManifestValueValidatorCollection validators, IIOHelper ioHelper, ILocalizedTextService localizedTextService)
        : this(validators, ioHelper, localizedTextService, StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
    {
    }

    public TagConfigurationEditor(ManifestValueValidatorCollection validators, IIOHelper ioHelper, ILocalizedTextService localizedTextService, IEditorConfigurationParser editorConfigurationParser)
        : base(ioHelper, editorConfigurationParser)
    {
        Field(nameof(TagConfiguration.Group)).Validators.Add(new RequiredValidator(localizedTextService));
        Field(nameof(TagConfiguration.StorageType)).Validators.Add(new RequiredValidator(localizedTextService));
    }

    public override Dictionary<string, object> ToConfigurationEditor(TagConfiguration? configuration)
    {
        Dictionary<string, object> dictionary = base.ToConfigurationEditor(configuration);

        // the front-end editor expects the string value of the storage type
        if (!dictionary.TryGetValue("storageType", out var storageType))
        {
            storageType = TagsStorageType.Json; // default to Json
        }

        dictionary["storageType"] = storageType.ToString()!;

        return dictionary;
    }

    public override TagConfiguration? FromConfigurationEditor(
        IDictionary<string, object?>? editorValues,
        TagConfiguration? configuration)
    {
        // the front-end editor returns the string value of the storage type
        // pure Json could do with
        // [JsonConverter(typeof(StringEnumConverter))]
        // but here we're only deserializing to object and it's too late
        if (editorValues is not null)
        {
            editorValues["storageType"] = Enum.Parse(typeof(TagsStorageType), (string)editorValues["storageType"]!);
        }

        return base.FromConfigurationEditor(editorValues, configuration);
    }
}
