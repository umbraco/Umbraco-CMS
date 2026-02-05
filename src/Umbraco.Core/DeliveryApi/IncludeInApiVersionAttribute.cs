namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Specifies that the property should be included in the Delivery API response based on API version constraints.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class IncludeInApiVersionAttribute : Attribute
{
    /// <summary>
    ///     Gets the minimum API version (inclusive) for which the property should be included, or <c>null</c> if no minimum is specified.
    /// </summary>
    public int? MinVersion { get; }

    /// <summary>
    ///     Gets the maximum API version (inclusive) for which the property should be included, or <c>null</c> if no maximum is specified.
    /// </summary>
    public int? MaxVersion { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="IncludeInApiVersionAttribute"/> class.
    ///     Specifies that the property should be included in the API response if the API version falls within the specified bounds.
    /// </summary>
    /// <param name="minVersion">The minimum API version (inclusive) for which the property should be included.</param>
    /// <param name="maxVersion">The maximum API version (inclusive) for which the property should be included.</param>
    public IncludeInApiVersionAttribute(int minVersion = -1, int maxVersion = -1)
    {
        MinVersion = minVersion >= 0 ? minVersion : null;
        MaxVersion = maxVersion >= 0 ? maxVersion : null;
    }
}
