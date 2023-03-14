using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Implements <see cref="IRedirectUrl" />.
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
public class RedirectUrl : EntityBase, IRedirectUrl
{
    private int _contentId;
    private Guid _contentKey;
    private DateTime _createDateUtc;
    private string? _culture;
    private string _url;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RedirectUrl" /> class.
    /// </summary>
    public RedirectUrl()
    {
        CreateDateUtc = DateTime.UtcNow;
        _url = string.Empty;
    }

    /// <inheritdoc />
    public int ContentId
    {
        get => _contentId;
        set => SetPropertyValueAndDetectChanges(value, ref _contentId, nameof(ContentId));
    }

    /// <inheritdoc />
    public Guid ContentKey
    {
        get => _contentKey;
        set => SetPropertyValueAndDetectChanges(value, ref _contentKey, nameof(ContentKey));
    }

    /// <inheritdoc />
    public DateTime CreateDateUtc
    {
        get => _createDateUtc;
        set => SetPropertyValueAndDetectChanges(value, ref _createDateUtc, nameof(CreateDateUtc));
    }

    /// <inheritdoc />
    public string? Culture
    {
        get => _culture;
        set => SetPropertyValueAndDetectChanges(value, ref _culture, nameof(Culture));
    }

    /// <inheritdoc />
    public string Url
    {
        get => _url;
        set => SetPropertyValueAndDetectChanges(value, ref _url!, nameof(Url));
    }
}
