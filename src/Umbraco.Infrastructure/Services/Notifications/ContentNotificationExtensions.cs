// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Infrastructure.Services.Notifications
{
    public static class ContentNotificationExtensions
    {
        /// <summary>
        /// Determines whether a culture is being saved, during a Saving notification
        /// </summary>
        public static bool IsSavingCulture<T>(this SavingNotification<T> notification, T content, string culture) where T : IContentBase
            => content.CultureInfos.TryGetValue(culture, out ContentCultureInfos cultureInfo) && cultureInfo.IsDirty();

        /// <summary>
        /// Determines whether a culture has been saved, during a Saved notification
        /// </summary>
        public static bool HasSavedCulture<T>(this SavedNotification<T> notification, T content, string culture) where T : IContentBase
            => content.WasPropertyDirty(ContentBase.ChangeTrackingPrefix.UpdatedCulture + culture);

        /// <summary>
        /// Determines whether a culture is being published, during a Publishing notification
        /// </summary>
        public static bool IsPublishingCulture(this PublishingNotification<IContent> notification, IContent content, string culture)
            => content.PublishCultureInfos.TryGetValue(culture, out ContentCultureInfos cultureInfo) && cultureInfo.IsDirty();

        /// <summary>
        /// Determines whether a culture is being unpublished, during a Publishing notification
        /// </summary>
        public static bool IsUnpublishingCulture(this UnpublishingNotification<IContent> notification, IContent content, string culture)
            => content.IsPropertyDirty(ContentBase.ChangeTrackingPrefix.UnpublishedCulture + culture); //bit of a hack since we know that the content implementation tracks changes this way

        /// <summary>
        /// Determines whether a culture has been published, during a Published notification
        /// </summary>
        public static bool HasPublishedCulture(this PublishedNotification<IContent> notification, IContent content, string culture)
            => content.WasPropertyDirty(ContentBase.ChangeTrackingPrefix.ChangedCulture + culture);

        /// <summary>
        /// Determines whether a culture has been unpublished, during a Published notification
        /// </summary>
        public static bool HasUnpublishedCulture(this UnpublishedNotification<IContent> notification, IContent content, string culture)
            => content.WasPropertyDirty(ContentBase.ChangeTrackingPrefix.UnpublishedCulture + culture);

    }
}
