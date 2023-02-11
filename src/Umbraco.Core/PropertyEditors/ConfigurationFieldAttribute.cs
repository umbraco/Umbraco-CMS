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
    /// <param name="name">The friendly name of the field.</param>
    /// <param name="view">The view to use to render the field editor.</param>
    public ConfigurationFieldAttribute(string key, string name, string view)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(key));
        }

        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(name));
        }

        if (view == null)
        {
            throw new ArgumentNullException(nameof(view));
        }

        if (string.IsNullOrWhiteSpace(view))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(view));
        }

        Key = key;
        Name = name;
        View = view;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigurationField" /> class.
    /// </summary>
    /// <param name="name">The friendly name of the field.</param>
    /// <param name="view">The view to use to render the field editor.</param>
    /// <remarks>
    ///     When no key is specified, the <see cref="ConfigurationEditor" /> will derive a key
    ///     from the name of the property marked with this attribute.
    /// </remarks>
    public ConfigurationFieldAttribute(string name, string view)
    {
        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(name));
        }

        if (view == null)
        {
            throw new ArgumentNullException(nameof(view));
        }

        if (string.IsNullOrWhiteSpace(view))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(view));
        }

        Name = name;
        View = view;
        Key = string.Empty;
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
    ///     Gets the friendly name of the field.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    ///     Gets the view to use to render the field editor.
    /// </summary>
    public string? View { get; }

    /// <summary>
    ///     Gets or sets the description of the field.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the field editor should be displayed without its label.
    /// </summary>
    public bool HideLabel
    {
        get => HideLabelSettable.ValueOrDefault(false);
        set => HideLabelSettable.Set(value);
    }

    /// <summary>
    ///     Gets the settable underlying <see cref="HideLabel" />.
    /// </summary>
    public Settable<bool> HideLabelSettable { get; } = new();

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
