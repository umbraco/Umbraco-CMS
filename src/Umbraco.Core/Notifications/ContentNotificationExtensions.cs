// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public static class ContentNotificationExtensions
{
    /// <summary>
    ///     Determines whether a culture is being saved, during a Saving notification
    /// </summary>
    public static bool IsSavingCulture<T>(this SavingNotification<T> notification, T content, string culture)
        where T : IContentBase
        => (content.CultureInfos?.TryGetValue(culture, out ContentCultureInfos cultureInfo) ?? false) &&
           cultureInfo.IsDirty();

    /// <summary>
    ///     Determines whether a culture has been saved, during a Saved notification
    /// </summary>
    public static bool HasSavedCulture<T>(this SavedNotification<T> notification, T content, string culture)
        where T : IContentBase
        => content.WasPropertyDirty(ContentBase.ChangeTrackingPrefix.UpdatedCulture + culture);

    /// <summary>
    ///     Determines whether a culture is being published, during a Publishing notification
    /// </summary>
    public static bool IsPublishingCulture(this ContentPublishingNotification notification, IContent content, string culture)
        => IsPublishingCulture(content, culture);

    /// <summary>
    ///     Determines whether a culture is being unpublished, during an Publishing notification
    /// </summary>
    public static bool IsUnpublishingCulture(this ContentPublishingNotification notification, IContent content, string culture)
        => IsUnpublishingCulture(content, culture);

    /// <summary>
    ///     Determines whether a culture is being unpublished, during a Unpublishing notification
    /// </summary>
    public static bool IsUnpublishingCulture(this ContentUnpublishingNotification notification, IContent content, string culture) => IsUnpublishingCulture(content, culture);

    /// <summary>
    ///     Determines whether a culture has been published, during a Published notification
    /// </summary>
    public static bool HasPublishedCulture(this ContentPublishedNotification notification, IContent content, string culture)
        => content.WasPropertyDirty(ContentBase.ChangeTrackingPrefix.ChangedCulture + culture);

    /// <summary>
    ///     Determines whether a culture has been unpublished, during a Published notification
    /// </summary>
    public static bool HasUnpublishedCulture(this ContentPublishedNotification notification, IContent content, string culture)
        => HasUnpublishedCulture(content, culture);

    /// <summary>
    ///     Determines whether a culture has been unpublished, during an Unpublished notification
    /// </summary>
    public static bool HasUnpublishedCulture(this ContentUnpublishedNotification notification, IContent content, string culture)
        => HasUnpublishedCulture(content, culture);

    public static bool IsPublishingCulture(IContent content, string culture)
        => (content.PublishCultureInfos?.TryGetValue(culture, out ContentCultureInfos cultureInfo) ?? false) &&
           cultureInfo.IsDirty();

    private static bool IsUnpublishingCulture(IContent content, string culture)
        => content.IsPropertyDirty(ContentBase.ChangeTrackingPrefix.UnpublishedCulture + culture);

    public static bool HasUnpublishedCulture(IContent content, string culture)
        => content.WasPropertyDirty(ContentBase.ChangeTrackingPrefix.UnpublishedCulture + culture);
}
