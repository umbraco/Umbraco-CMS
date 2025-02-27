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

        Key = attribute.Key;
    }

    /// <summary>
    ///     Gets or sets the key of the field.
    /// </summary>
    [DataMember(Name = "key", IsRequired = true)]
    public string Key { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the property name of the field.
    /// </summary>
    public string? PropertyName { get; set; }

    /// <summary>
    ///     Gets or sets the property CLR type of the field.
    /// </summary>
    public Type? PropertyType { get; set; }

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
