namespace Umbraco.Cms.Core.Routing;

/// <summary>
/// Represents a published snapshot domain.
/// </summary>
public class Domain
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Domain" /> class.
    /// </summary>
    /// <param name="id">The unique identifier of the domain.</param>
    /// <param name="name">The name of the domain.</param>
    /// <param name="contentId">The identifier of the content which supports the domain.</param>
    /// <param name="culture">The culture of the domain.</param>
    /// <param name="isWildcard">A value indicating whether the domain is a wildcard domain.</param>
    /// <param name="sortOrder">The sort order.</param>
    public Domain(int id, string name, int contentId, string? culture, bool isWildcard, int sortOrder)
    {
        Id = id;
        Name = name;
        ContentId = contentId;
        Culture = culture;
        IsWildcard = isWildcard;
        SortOrder = sortOrder;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Domain" /> class.
    /// </summary>
    /// <param name="domain">An origin domain.</param>
    protected Domain(Domain domain)
    {
        Id = domain.Id;
        Name = domain.Name;
        ContentId = domain.ContentId;
        Culture = domain.Culture;
        IsWildcard = domain.IsWildcard;
        SortOrder = domain.SortOrder;
    }

    /// <summary>
    /// Gets the unique identifier of the domain.
    /// </summary>
    /// <value>
    /// The unique identifier of the domain.
    /// </value>
    public int Id { get; }

    /// <summary>
    /// Gets the name of the domain.
    /// </summary>
    /// <value>
    /// The name of the domain.
    /// </value>
    public string Name { get; }

    /// <summary>
    /// Gets the identifier of the content which supports the domain.
    /// </summary>
    /// <value>
    /// The identifier of the content which supports the domain.
    /// </value>
    public int ContentId { get; }

    /// <summary>
    /// Gets the culture of the domain.
    /// </summary>
    /// <value>
    /// The culture of the domain.
    /// </value>
    public string? Culture { get; }

    /// <summary>
    /// Gets a value indicating whether the domain is a wildcard domain.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this is a wildcard domain; otherwise, <c>false</c>.
    /// </value>
    public bool IsWildcard { get; }

    /// <summary>
    /// Gets the sort order.
    /// </summary>
    /// <value>
    /// The sort order.
    /// </value>
    public int SortOrder { get; }
}
