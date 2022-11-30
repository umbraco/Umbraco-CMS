using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Represents ordering information.
/// </summary>
public class Ordering
{
    private static readonly Ordering DefaultOrdering = new(null);

    /// <summary>
    ///     Initializes a new instance of the <see cref="Ordering" /> class.
    /// </summary>
    /// <param name="orderBy">The name of the ordering field.</param>
    /// <param name="direction">The ordering direction.</param>
    /// <param name="culture">The (ISO) culture to consider when sorting multi-lingual fields.</param>
    /// <param name="isCustomField">A value indicating whether the ordering field is a custom user property.</param>
    /// <remarks>
    ///     <para>
    ///         The <paramref name="orderBy" /> can be null, meaning: not sorting. If it is the empty string, it becomes
    ///         null.
    ///     </para>
    ///     <para>
    ///         The <paramref name="culture" /> can be the empty string, meaning: invariant. If it is null, it becomes the
    ///         empty string.
    ///     </para>
    /// </remarks>
    public Ordering(string? orderBy, Direction direction = Direction.Ascending, string? culture = null, bool isCustomField = false)
    {
        OrderBy = orderBy.IfNullOrWhiteSpace(null); // empty is null and means, not sorting
        Direction = direction;
        Culture = culture.IfNullOrWhiteSpace(string.Empty); // empty is "" and means, invariant
        IsCustomField = isCustomField;
    }

    /// <summary>
    ///     Gets the name of the ordering field.
    /// </summary>
    public string? OrderBy { get; }

    /// <summary>
    ///     Gets the ordering direction.
    /// </summary>
    public Direction Direction { get; }

    /// <summary>
    ///     Gets (ISO) culture to consider when sorting multi-lingual fields.
    /// </summary>
    public string? Culture { get; }

    /// <summary>
    ///     Gets a value indicating whether the ordering field is a custom user property.
    /// </summary>
    public bool IsCustomField { get; }

    /// <summary>
    ///     Gets a value indicating whether this ordering is the default ordering.
    /// </summary>
    public bool IsEmpty => this == DefaultOrdering || OrderBy == null;

    /// <summary>
    ///     Gets a value indicating whether the culture of this ordering is invariant.
    /// </summary>
    public bool IsInvariant => this == DefaultOrdering || Culture == string.Empty;

    /// <summary>
    ///     Creates a new instance of the <see cref="Ordering" /> class.
    /// </summary>
    /// <param name="orderBy">The name of the ordering field.</param>
    /// <param name="direction">The ordering direction.</param>
    /// <param name="culture">The (ISO) culture to consider when sorting multi-lingual fields.</param>
    /// <param name="isCustomField">A value indicating whether the ordering field is a custom user property.</param>
    /// <remarks>
    ///     <para>
    ///         The <paramref name="orderBy" /> can be null, meaning: not sorting. If it is the empty string, it becomes
    ///         null.
    ///     </para>
    ///     <para>
    ///         The <paramref name="culture" /> can be the empty string, meaning: invariant. If it is null, it becomes the
    ///         empty string.
    ///     </para>
    /// </remarks>
    public static Ordering By(string orderBy, Direction direction = Direction.Ascending, string? culture = null, bool isCustomField = false)
        => new(orderBy, direction, culture, isCustomField);

    /// <summary>
    ///     Gets the default <see cref="Ordering" /> instance.
    /// </summary>
    public static Ordering ByDefault()
        => DefaultOrdering;
}
