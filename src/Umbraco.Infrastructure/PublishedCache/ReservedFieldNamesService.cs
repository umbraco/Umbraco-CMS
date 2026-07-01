using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.PublishedCache;

internal sealed class ReservedFieldNamesService : IReservedFieldNamesService
{
    private readonly ContentPropertySettings _contentPropertySettings;
    private readonly MemberPropertySettings _memberPropertySettings;
    private readonly MediaPropertySettings _mediaPropertySettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReservedFieldNamesService"/> class.
    /// </summary>
    /// <param name="contentPropertySettings">The options for content property settings.</param>
    /// <param name="memberPropertySettings">The options for member property settings.</param>
    /// <param name="mediaPropertySettings">The options for media property settings.</param>
    public ReservedFieldNamesService(
        IOptions<ContentPropertySettings> contentPropertySettings,
        IOptions<MemberPropertySettings> memberPropertySettings,
        IOptions<MediaPropertySettings> mediaPropertySettings)
    {
        _contentPropertySettings = contentPropertySettings.Value;
        _memberPropertySettings = memberPropertySettings.Value;
        _mediaPropertySettings = mediaPropertySettings.Value;
    }

    /// <summary>
    /// Retrieves the set of reserved field names that cannot be used for document properties.
    /// </summary>
    /// <returns>An <see cref="ISet{String}"/> containing the reserved field names for documents.</returns>
    public ISet<string> GetDocumentReservedFieldNames() => _contentPropertySettings.ReservedFieldNames;

    /// <summary>
    /// Retrieves the collection of reserved field names that are used for media items.
    /// </summary>
    /// <returns>An <see cref="ISet{String}"/> containing the reserved field names for media.</returns>
    public ISet<string> GetMediaReservedFieldNames() => _mediaPropertySettings.ReservedFieldNames;

    /// <summary>
    /// Returns the reserved field names used for member properties.
    /// </summary>
    /// <returns>A set containing the reserved field names for members.</returns>
    public ISet<string> GetMemberReservedFieldNames() => _memberPropertySettings.ReservedFieldNames;
}
