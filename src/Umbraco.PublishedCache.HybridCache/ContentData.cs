using System.ComponentModel;

namespace Umbraco.Cms.Infrastructure.HybridCache;

/// <summary>
/// Represents everything that is specific to an edited or published content version
/// </summary>
// This is for cache performance reasons, see https://learn.microsoft.com/en-us/aspnet/core/performance/caching/hybrid?view=aspnetcore-9.0#reuse-objects
[ImmutableObject(true)]
public sealed class ContentData
{
    public ContentData(string? name, string? urlSegment, int versionId, DateTime versionDate, int writerId, int? templateId, bool published, Dictionary<string, PropertyData[]>? properties, IReadOnlyDictionary<string, CultureVariation>? cultureInfos)
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

    public string Name { get; }

    public string? UrlSegment { get; }

    public int VersionId { get; }

    public DateTime VersionDate { get; }

    public int WriterId { get; }

    public int? TemplateId { get; }

    public bool Published { get; }

    public Dictionary<string, PropertyData[]> Properties { get; }

    /// <summary>
    /// The collection of language Id to name for the content item
    /// </summary>
    public IReadOnlyDictionary<string, CultureVariation>? CultureInfos { get; }
}
