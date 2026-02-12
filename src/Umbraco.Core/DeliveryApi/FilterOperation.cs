namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Specifies the type of filter operation to apply in Delivery API queries.
/// </summary>
public enum FilterOperation
{
    /// <summary>
    ///     Exact equality match.
    /// </summary>
    Is,

    /// <summary>
    ///     Not equal to the specified value.
    /// </summary>
    IsNot,

    /// <summary>
    ///     Contains the specified value.
    /// </summary>
    Contains,

    /// <summary>
    ///     Does not contain the specified value.
    /// </summary>
    DoesNotContain,

    /// <summary>
    ///     Less than the specified value.
    /// </summary>
    LessThan,

    /// <summary>
    ///     Less than or equal to the specified value.
    /// </summary>
    LessThanOrEqual,

    /// <summary>
    ///     Greater than the specified value.
    /// </summary>
    GreaterThan,

    /// <summary>
    ///     Greater than or equal to the specified value.
    /// </summary>
    GreaterThanOrEqual
}
