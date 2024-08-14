using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.PublishedCache;

internal class ReservedFieldNamesService : IReservedFieldNamesService
{
    private readonly ContentPropertySettings _contentPropertySettings;
    private readonly MemberPropertySettings _memberPropertySettings;
    private readonly MediaPropertySettings _mediaPropertySettings;

    public ReservedFieldNamesService(
        IOptions<ContentPropertySettings> contentPropertySettings,
        IOptions<MemberPropertySettings> memberPropertySettings,
        IOptions<MediaPropertySettings> mediaPropertySettings)
    {
        _contentPropertySettings = contentPropertySettings.Value;
        _memberPropertySettings = memberPropertySettings.Value;
        _mediaPropertySettings = mediaPropertySettings.Value;
    }

    public ISet<string> GetDocumentReservedFieldNames() => _contentPropertySettings.ReservedFieldNames;

    public ISet<string> GetMediaReservedFieldNames() => _mediaPropertySettings.ReservedFieldNames;

    public ISet<string> GetMemberReservedFieldNames() => _memberPropertySettings.ReservedFieldNames;
}
