using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Marks property editors that support tags.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class TagsPropertyEditorAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TagsPropertyEditorAttribute" /> class.
    /// </summary>
    public TagsPropertyEditorAttribute(Type tagsConfigurationProvider)
        : this() =>
        TagsConfigurationProviderType = tagsConfigurationProvider ??
                                        throw new ArgumentNullException(nameof(tagsConfigurationProvider));

    /// <summary>
    ///     Initializes a new instance of the <see cref="TagsPropertyEditorAttribute" /> class.
    /// </summary>
    public TagsPropertyEditorAttribute()
    {
        Delimiter = ',';
        ReplaceTags = true;
        TagGroup = "default";
        StorageType = TagsStorageType.Json;
    }

    /// <summary>
    ///     Gets or sets a value indicating how tags are stored.
    /// </summary>
    public TagsStorageType StorageType { get; set; }

    /// <summary>
    ///     Gets or sets the delimited for delimited strings.
    /// </summary>
    /// <remarks>Default is a comma. Has no meaning when tags are stored as Json.</remarks>
    public char Delimiter { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether to replace the tags entirely.
    /// </summary>
    // TODO: what's the usage?
    public bool ReplaceTags { get; set; }

    /// <summary>
    ///     Gets or sets the tags group.
    /// </summary>
    /// <remarks>Default is "default".</remarks>
    public string TagGroup { get; set; }

    /// <summary>
    ///     Gets the type of the dynamic configuration provider.
    /// </summary>
    // TODO: This is not used and should be implemented in a nicer way, see https://github.com/umbraco/Umbraco-CMS/issues/6017#issuecomment-516253562
    public Type? TagsConfigurationProviderType { get; }
}
