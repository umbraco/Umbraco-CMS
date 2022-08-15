using System.Runtime.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a datatype configuration field for editing.
/// </summary>
[DataContract]
public class ConfigurationField
{
    private readonly string? _view;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigurationField" /> class.
    /// </summary>
    public ConfigurationField()
        : this(new List<IValueValidator>())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigurationField" /> class.
    /// </summary>
    public ConfigurationField(params IValueValidator[] validators)
        : this(validators.ToList())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigurationField" /> class.
    /// </summary>
    private ConfigurationField(List<IValueValidator> validators)
    {
        Validators = validators;
        Config = new Dictionary<string, object>();

        // fill details from attribute, if any
        ConfigurationFieldAttribute? attribute = GetType().GetCustomAttribute<ConfigurationFieldAttribute>(false);
        if (attribute is null)
        {
            return;
        }

        Name = attribute.Name;
        Description = attribute.Description;
        HideLabel = attribute.HideLabel;
        Key = attribute.Key;
        View = attribute.View;
    }

    /// <summary>
    ///     Gets or sets the key of the field.
    /// </summary>
    [DataMember(Name = "key", IsRequired = true)]
    public string Key { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the name of the field.
    /// </summary>
    [DataMember(Name = "label", IsRequired = true)]
    public string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the property name of the field.
    /// </summary>
    public string? PropertyName { get; set; }

    /// <summary>
    ///     Gets or sets the property CLR type of the field.
    /// </summary>
    public Type? PropertyType { get; set; }

    /// <summary>
    ///     Gets or sets the description of the field.
    /// </summary>
    [DataMember(Name = "description")]
    public string? Description { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether to hide the label of the field.
    /// </summary>
    [DataMember(Name = "hideLabel")]
    public bool HideLabel { get; set; }

    /// <summary>
    ///     Gets or sets the view to used in the editor.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Can be the full virtual path, or the relative path to the Umbraco folder,
    ///         or a simple view name which will map to ~/Views/PreValueEditors/{view}.html.
    ///     </para>
    /// </remarks>
    [DataMember(Name = "view", IsRequired = true)]
    public string? View { get; set; }

    /// <summary>
    ///     Gets the validators of the field.
    /// </summary>
    [DataMember(Name = "validation")]
    public List<IValueValidator> Validators { get; }

    /// <summary>
    ///     Gets or sets extra configuration properties for the editor.
    /// </summary>
    [DataMember(Name = "config")]
    public IDictionary<string, object> Config { get; set; }
}
