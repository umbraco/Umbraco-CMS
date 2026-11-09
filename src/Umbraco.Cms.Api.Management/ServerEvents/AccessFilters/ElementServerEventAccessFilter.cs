using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.ServerEvents.AccessFilters;

/// <summary>
/// Gates element and element container server events by the recipient's element start-node access.
/// </summary>
public sealed class ElementServerEventAccessFilter : EntityStartNodeAccessFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ElementServerEventAccessFilter"/> class.
    /// </summary>
    /// <param name="entityService">The entity service used when calculating start nodes.</param>
    /// <param name="appCaches">The application caches backing the start-node calculation.</param>
    public ElementServerEventAccessFilter(IEntityService entityService, AppCaches appCaches)
        : base(entityService, appCaches)
    {
    }

    /// <inheritdoc />
    public override IEnumerable<string> FilteredEventSources => [Constants.ServerEvents.EventSource.Element, Constants.ServerEvents.EventSource.ElementContainer];

    /// <inheritdoc />
    protected override int RecycleBinId => Constants.System.RecycleBinElement;

    /// <inheritdoc />
    protected override int[]? CalculateStartNodeIds(IUser user, IEntityService entityService, AppCaches appCaches) =>
        user.CalculateElementStartNodeIds(entityService, appCaches);
}
