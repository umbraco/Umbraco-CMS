namespace Umbraco.Cms.Core.DeliveryApi;

[AttributeUsage(AttributeTargets.Property)]
public class IncludeInApiVersionAttribute : Attribute
{
    public int[] Versions { get; }

    public int? MinVersion { get; }

    public int? MaxVersion { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="IncludeInApiVersionAttribute"/> class.
    ///     Specifies that the property should be included in the API response for the specified versions.
    /// </summary>
    /// <param name="versions">The API versions for which the property should be included.</param>
    public IncludeInApiVersionAttribute(params int[] versions)
    {
        Versions = versions;
        MinVersion = null;
        MaxVersion = null;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="IncludeInApiVersionAttribute"/> class.
    ///     Specifies that the property should be included in the API response if the API version falls within the specified bounds.
    /// </summary>
    /// <param name="minVersion">The minimum API version (inclusive) for which the property should be included.</param>
    /// <param name="maxVersion">The maximum API version (inclusive) for which the property should be included.</param>
    public IncludeInApiVersionAttribute(int? minVersion = null, int? maxVersion = null)
    {
        Versions = [];
        MinVersion = minVersion;
        MaxVersion = maxVersion;
    }
}
