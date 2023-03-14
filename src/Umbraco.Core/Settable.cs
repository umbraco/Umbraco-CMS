namespace Umbraco.Cms.Core;

/// <summary>
///     Represents a value that can be assigned a value.
/// </summary>
/// <typeparam name="T">The type of the value</typeparam>
public class Settable<T>
{
    private T? _value;

    /// <summary>
    ///     Gets a value indicating whether a value has been assigned to this <see cref="Settable{T}" /> instance.
    /// </summary>
    public bool HasValue { get; private set; }

    /// <summary>
    ///     Gets the value assigned to this <see cref="Settable{T}" /> instance.
    /// </summary>
    /// <remarks>An exception is thrown if the HasValue property is false.</remarks>
    /// <exception cref="InvalidOperationException">No value has been assigned to this instance.</exception>
    public T? Value
    {
        get
        {
            if (HasValue == false)
            {
                throw new InvalidOperationException("The HasValue property is false.");
            }

            return _value;
        }
    }

    /// <summary>
    ///     Assigns a value to this <see cref="Settable{T}" /> instance.
    /// </summary>
    /// <param name="value">The value.</param>
    public void Set(T? value)
    {
        if (value is not null)
        {
            HasValue = true;
        }

        _value = value;
    }

    /// <summary>
    ///     Assigns a value to this <see cref="Settable{T}" /> instance by copying the value
    ///     of another instance, if the other instance has a value.
    /// </summary>
    /// <param name="other">The other instance.</param>
    public void Set(Settable<T> other)
    {
        // set only if has value else don't change anything
        if (other.HasValue)
        {
            Set(other.Value);
        }
    }

    /// <summary>
    ///     Clears the value.
    /// </summary>
    public void Clear()
    {
        HasValue = false;
        _value = default;
    }

    /// <summary>
    ///     Gets the value assigned to this <see cref="Settable{T}" /> instance, if a value has been assigned,
    ///     otherwise the default value of <typeparamref name="T" />.
    /// </summary>
    /// <returns>
    ///     The value assigned to this <see cref="Settable{T}" /> instance, if a value has been assigned,
    ///     else the default value of <typeparamref name="T" />.
    /// </returns>
    public T? ValueOrDefault() => HasValue ? _value : default;

    /// <summary>
    ///     Gets the value assigned to this <see cref="Settable{T}" /> instance, if a value has been assigned,
    ///     otherwise a specified default value.
    /// </summary>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    ///     The value assigned to this <see cref="Settable{T}" /> instance, if a value has been assigned,
    ///     else <paramref name="defaultValue" />.
    /// </returns>
    public T? ValueOrDefault(T defaultValue) => HasValue ? _value : defaultValue;

    /// <inheritdoc />
    public override string? ToString() => HasValue ? _value?.ToString() : "void";
}
