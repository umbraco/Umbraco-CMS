using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

/// <inheritdoc />
[Serializable]
[DataContract(IsReference = true)]
public class UmbracoDomain : EntityBase, IDomain
{
    private string _domainName;
    private int? _languageId;
    private int? _rootContentId;
    private int _sortOrder;

    /// <summary>
    /// Initializes a new instance of the <see cref="UmbracoDomain" /> class.
    /// </summary>
    /// <param name="domainName">The name of the domain.</param>
    public UmbracoDomain(string domainName)
        => _domainName = domainName;

    /// <summary>
    /// Initializes a new instance of the <see cref="UmbracoDomain" /> class.
    /// </summary>
    /// <param name="domainName">The name of the domain.</param>
    /// <param name="languageIsoCode">The language ISO code.</param>
    public UmbracoDomain(string domainName, string languageIsoCode)
        : this(domainName)
        => LanguageIsoCode = languageIsoCode;

    /// <inheritdoc />
    [DataMember]
    public string DomainName
    {
        get => _domainName;
        set => SetPropertyValueAndDetectChanges(value, ref _domainName!, nameof(DomainName));
    }

    /// <inheritdoc />
    public bool IsWildcard => string.IsNullOrWhiteSpace(DomainName) || DomainName.StartsWith("*");

    /// <inheritdoc />
    [DataMember]
    public int? LanguageId
    {
        get => _languageId;
        set => SetPropertyValueAndDetectChanges(value, ref _languageId, nameof(LanguageId));
    }

    /// <inheritdoc />
    public string? LanguageIsoCode { get; set; }

    /// <inheritdoc />
    [DataMember]
    public int? RootContentId
    {
        get => _rootContentId;
        set => SetPropertyValueAndDetectChanges(value, ref _rootContentId, nameof(RootContentId));
    }

    /// <inheritdoc />
    [DataMember]
    public int SortOrder
    {
        get => _sortOrder;
        set => SetPropertyValueAndDetectChanges(value, ref _sortOrder, nameof(SortOrder));
    }
}
