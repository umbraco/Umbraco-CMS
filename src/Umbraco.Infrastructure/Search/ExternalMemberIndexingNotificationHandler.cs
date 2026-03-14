// Copyright (c) Umbraco.
// See LICENSE for more details.

using Examine;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Search;

/// <summary>
///     Handles indexing of external-only members in response to save and delete notifications.
/// </summary>
/// <remarks>
///     Unlike <see cref="MemberIndexingNotificationHandler"/>, this handler subscribes directly
///     to <see cref="ExternalMemberSavedNotification"/> and <see cref="ExternalMemberDeletedNotification"/>
///     rather than cache refresher notifications, because external members do not use the
///     distributed cache refresher pipeline.
/// </remarks>
public sealed class ExternalMemberIndexingNotificationHandler :
    INotificationHandler<ExternalMemberSavedNotification>,
    INotificationHandler<ExternalMemberDeletedNotification>
{
    private readonly IExamineManager _examineManager;
    private readonly IValueSetBuilder<ExternalMemberIdentity> _valueSetBuilder;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExternalMemberIndexingNotificationHandler"/> class.
    /// </summary>
    /// <param name="examineManager">Manages Examine indexes and searchers.</param>
    /// <param name="valueSetBuilder">Builder for creating value sets from external member entities.</param>
    public ExternalMemberIndexingNotificationHandler(
        IExamineManager examineManager,
        IValueSetBuilder<ExternalMemberIdentity> valueSetBuilder)
    {
        _examineManager = examineManager;
        _valueSetBuilder = valueSetBuilder;
    }

    /// <summary>
    ///     Handles the <see cref="ExternalMemberSavedNotification"/> by re-indexing
    ///     the saved external members into all registered member indexes.
    /// </summary>
    /// <param name="notification">The notification containing the saved external members.</param>
    public void Handle(ExternalMemberSavedNotification notification)
    {
        IEnumerable<ValueSet> valueSets = _valueSetBuilder.GetValueSets(notification.SavedEntities.ToArray());

        foreach (IUmbracoMemberIndex index in _examineManager.Indexes.OfType<IUmbracoMemberIndex>())
        {
            index.IndexItems(valueSets);
        }
    }

    /// <summary>
    ///     Handles the <see cref="ExternalMemberDeletedNotification"/> by removing
    ///     the deleted external members from all registered member indexes.
    /// </summary>
    /// <param name="notification">The notification containing the deleted external members.</param>
    public void Handle(ExternalMemberDeletedNotification notification)
    {
        foreach (ExternalMemberIdentity entity in notification.DeletedEntities)
        {
            foreach (IUmbracoMemberIndex index in _examineManager.Indexes.OfType<IUmbracoMemberIndex>())
            {
                index.DeleteFromIndex(entity.Id.ToInvariantString());
            }
        }
    }
}
