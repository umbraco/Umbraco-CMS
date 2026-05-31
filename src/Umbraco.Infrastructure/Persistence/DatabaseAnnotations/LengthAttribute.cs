namespace Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

/// <summary>
///     Attribute that represents the length of a column
/// </summary>
/// <remarks>Used to define the length of fixed sized columns - typically used for nvarchar</remarks>
[AttributeUsage(AttributeTargets.Property)]
public class LengthAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LengthAttribute"/> class with the specified maximum length.
    /// </summary>
    /// <param name="length">The maximum length value for the attribute.</param>
    public LengthAttribute(int length) => Length = length;

    /// <summary>
    ///     Gets or sets the length of a column
    /// </summary>
    public int Length { get; }
}
