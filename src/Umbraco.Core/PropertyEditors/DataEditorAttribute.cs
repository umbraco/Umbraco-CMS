namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Marks a class that represents a data editor.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class DataEditorAttribute : Attribute
{
    private string _valueType = ValueTypes.String;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DataEditorAttribute" /> class for a property editor.
    /// </summary>
    /// <param name="alias">The unique identifier of the editor.</param>
    public DataEditorAttribute(string alias)
    {
        if (alias == null)
        {
            throw new ArgumentNullException(nameof(alias));
        }

        if (string.IsNullOrWhiteSpace(alias))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(alias));
        }

        Alias = alias;
    }

    /// <summary>
    ///     Gets the unique alias of the editor.
    /// </summary>
    public string Alias { get; }

    /// <summary>
    ///     Gets or sets the type of the edited value.
    /// </summary>
    /// <remarks>Must be a valid <see cref="ValueTypes" /> value.</remarks>
    public string ValueType
    {
        get => _valueType;
        set
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    "Value can't be empty or consist only of white-space characters.",
                    nameof(value));
            }

            if (!ValueTypes.IsValue(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, $"Not a valid {typeof(ValueTypes)} value.");
            }

            _valueType = value;
        }
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the value editor is deprecated.
    /// </summary>
    /// <remarks>A deprecated editor is still supported but not proposed in the UI.</remarks>
    public bool IsDeprecated { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the value editor can be reused (cached).
    /// </summary>
    /// <remarks>While most value editors can be reused, complex editors (e.g. block based editors) might not be applicable for reuse.</remarks>
    public bool ValueEditorIsReusable { get; set; }
}
