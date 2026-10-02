using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.ServerEvents.AccessFilters;

/// <summary>
/// Gates document server events by the recipient's content start-node access.
/// </summary>
public sealed class DocumentServerEventAccessFilter : EntityStartNodeAccessFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentServerEventAccessFilter"/> class.
    /// </summary>
    /// <param name="entityService">The entity service used when calculating start nodes.</param>
    /// <param name="appCaches">The application caches backing the start-node calculation.</param>
    public DocumentServerEventAccessFilter(IEntityService entityService, AppCaches appCaches)
        : base(entityService, appCaches)
    {
    }

    /// <inheritdoc />
    public override IEnumerable<string> FilteredEventSources => [Constants.ServerEvents.EventSource.Document];

    /// <inheritdoc />
    protected override int RecycleBinId => Constants.System.RecycleBinContent;

    /// <inheritdoc />
    protected override int[]? CalculateStartNodeIds(IUser user, IEntityService entityService, AppCaches appCaches) =>
        user.CalculateContentStartNodeIds(entityService, appCaches);
}
