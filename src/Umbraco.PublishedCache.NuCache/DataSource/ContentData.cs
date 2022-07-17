namespace Umbraco.Cms.Infrastructure.PublishedCache.DataSource;

/// <summary>
///     Represents everything that is specific to an edited or published content version
/// </summary>
public class ContentData
{
    // Scheduled for removal in V11
    [Obsolete("Use ctor with all params, as the pros should be immutable")]
    public ContentData()
    {
        Name = string.Empty;
        UrlSegment = string.Empty;
        Properties = null!;
        CultureInfos = null!;
    }

    public ContentData(
        string? name,
        string? urlSegment,
        int versionId,
        DateTime versionDate,
        int writerId,
        int? templateId,
        bool published,
        IDictionary<string, PropertyData[]>? properties,
        IReadOnlyDictionary<string, CultureVariation>? cultureInfos)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        UrlSegment = urlSegment;
        VersionId = versionId;
        VersionDate = versionDate;
        WriterId = writerId;
        TemplateId = templateId;
        Published = published;
        Properties = properties ?? throw new ArgumentNullException(nameof(properties));
        CultureInfos = cultureInfos;
    }

    public string Name
    {
        get;
        [Obsolete("Do not change this, use ctor with params and have this object immutable.")]
        set;
    }

    public string? UrlSegment
    {
        get;
        [Obsolete("Do not change this, use ctor with params and have this object immutable.")]
        set;
    }

    public int VersionId
    {
        get;
        [Obsolete("Do not change this, use ctor with params and have this object immutable.")]
        set;
    }

    public DateTime VersionDate
    {
        get;
        [Obsolete("Do not change this, use ctor with params and have this object immutable.")]
        set;
    }

    public int WriterId
    {
        get;
        [Obsolete("Do not change this, use ctor with params and have this object immutable.")]
        set;
    }

    public int? TemplateId
    {
        get;
        [Obsolete("Do not change this, use ctor with params and have this object immutable.")]
        set;
    }

    public bool Published
    {
        get;
        [Obsolete("Do not change this, use ctor with params and have this object immutable.")]
        set;
    }

    public IDictionary<string, PropertyData[]> Properties
    {
        get;
        [Obsolete("Do not change this, use ctor with params and have this object immutable.")]
        set;
    }

    /// <summary>
    ///     The collection of language Id to name for the content item
    /// </summary>
    public IReadOnlyDictionary<string, CultureVariation>? CultureInfos
    {
        get;
        [Obsolete("Do not change this, use ctor with params and have this object immutable.")]
        set;
    }
}
