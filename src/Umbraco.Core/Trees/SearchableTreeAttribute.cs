namespace Umbraco.Cms.Core.Trees;

[AttributeUsage(AttributeTargets.Class)]
public sealed class SearchableTreeAttribute : Attribute
{
    public const int DefaultSortOrder = 1000;

    /// <summary>
    ///     This constructor will assume that the method name equals `format(searchResult, appAlias, treeAlias)`.
    /// </summary>
    /// <param name="serviceName">Name of the service.</param>
    public SearchableTreeAttribute(string serviceName)
        : this(serviceName, string.Empty)
    {
    }

    /// <summary>
    ///     This constructor defines both the Angular service and method name to use.
    /// </summary>
    /// <param name="serviceName">Name of the service.</param>
    /// <param name="methodName">Name of the method.</param>
    public SearchableTreeAttribute(string serviceName, string methodName)
        : this(serviceName, methodName, DefaultSortOrder)
    {
    }

    /// <summary>
    ///     This constructor defines both the Angular service and method name to use and explicitly defines a sort order for
    ///     the results
    /// </summary>
    /// <param name="serviceName">Name of the service.</param>
    /// <param name="methodName">Name of the method.</param>
    /// <param name="sortOrder">The sort order.</param>
    /// <exception cref="ArgumentNullException">
    ///     serviceName
    ///     or
    ///     methodName
    /// </exception>
    /// <exception cref="ArgumentException">Value can't be empty or consist only of white-space characters. - serviceName</exception>
    public SearchableTreeAttribute(string serviceName, string methodName, int sortOrder)
    {
        if (serviceName == null)
        {
            throw new ArgumentNullException(nameof(serviceName));
        }

        if (string.IsNullOrWhiteSpace(serviceName))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(serviceName));
        }

        ServiceName = serviceName;
        MethodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
        SortOrder = sortOrder;
    }

    public string ServiceName { get; }

    public string MethodName { get; }

    public int SortOrder { get; }
}
