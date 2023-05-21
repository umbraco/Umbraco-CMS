namespace Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

/// <summary>
///     Attribute that represents the length of a column
/// </summary>
/// <remarks>Used to define the length of fixed sized columns - typically used for nvarchar</remarks>
[AttributeUsage(AttributeTargets.Property)]
public class LengthAttribute : Attribute
{
    public LengthAttribute(int length) => Length = length;

    /// <summary>
    ///     Gets or sets the length of a column
    /// </summary>
    public int Length { get; }
}
