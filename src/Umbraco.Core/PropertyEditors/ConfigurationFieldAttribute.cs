namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Marks a ConfigurationEditor property as a configuration field, and a class as a configuration field type.
/// </summary>
/// <remarks>Properties marked with this attribute are discovered as fields.</remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
public class ConfigurationFieldAttribute : Attribute
{
    private Type? _type;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigurationField" /> class.
    /// </summary>
    public ConfigurationFieldAttribute(Type type)
    {
        Type = type;
        Key = string.Empty;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigurationField" /> class.
    /// </summary>
    /// <param name="key">The unique identifier of the field.</param>
    public ConfigurationFieldAttribute(string key)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(key));
        }

        Key = key;
    }

    /// <summary>
    ///     Gets the key of the field.
    /// </summary>
    /// <remarks>
    ///     When null or empty, the <see cref="ConfigurationEditor" /> should derive a key
    ///     from the name of the property marked with this attribute.
    /// </remarks>
    public string Key { get; }

    /// <summary>
    ///     Gets or sets the type of the field.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         By default, fields are created as <see cref="ConfigurationField" /> instances,
    ///         unless specified otherwise through this property.
    ///     </para>
    ///     <para>The specified type must inherit from <see cref="ConfigurationField" />.</para>
    /// </remarks>
    public Type? Type
    {
        get => _type;
        set
        {
            if (!typeof(ConfigurationField).IsAssignableFrom(value))
            {
                throw new ArgumentException("Type must inherit from ConfigurationField.", nameof(value));
            }

            _type = value;
        }
    }
}
