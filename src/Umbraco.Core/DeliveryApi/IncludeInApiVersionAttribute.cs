namespace Umbraco.Cms.Core.DeliveryApi;

[AttributeUsage(AttributeTargets.Property)]
public class IncludeInApiVersionAttribute : Attribute
{
    public int? MinVersion { get; }

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
