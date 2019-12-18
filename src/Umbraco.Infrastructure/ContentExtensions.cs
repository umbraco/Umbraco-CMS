using System;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;

namespace Umbraco.Core
{
    public static class ContentExtensions
    {
        /// <summary>
        /// Removes characters that are not valid XML characters from all entity properties
        /// of type string. See: http://stackoverflow.com/a/961504/5018
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// If this is not done then the xml cache can get corrupt and it will throw YSODs upon reading it.
        /// </remarks>
        /// <param name="entity"></param>
        public static void SanitizeEntityPropertiesForXmlStorage(this IContentBase entity)
        {
            entity.Name = entity.Name.ToValidXmlString();
            foreach (var property in entity.Properties)
            {
                foreach (var propertyValue in property.Values)
                {
                    if (propertyValue.EditedValue is string editString)
                        propertyValue.EditedValue = editString.ToValidXmlString();
                    if (propertyValue.PublishedValue is string publishedString)
                        propertyValue.PublishedValue = publishedString.ToValidXmlString();
                }
            }
        }


        #region IContent

        /// <summary>
        /// Gets the current status of the Content
        /// </summary>
        public static ContentStatus GetStatus(this IContent content, string culture = null)
        {
            if (content.Trashed)
                return ContentStatus.Trashed;

            if (!content.ContentType.VariesByCulture())
                culture = string.Empty;
            else if (culture.IsNullOrWhiteSpace())
                throw new ArgumentNullException($"{nameof(culture)} cannot be null or empty");

            var expires = content.ContentSchedule.GetSchedule(culture, ContentScheduleAction.Expire);
            if (expires != null && expires.Any(x => x.Date > DateTime.MinValue && DateTime.Now > x.Date))
                return ContentStatus.Expired;

            var release = content.ContentSchedule.GetSchedule(culture, ContentScheduleAction.Release);
            if (release != null && release.Any(x => x.Date > DateTime.MinValue && x.Date > DateTime.Now))
                return ContentStatus.AwaitingRelease;

            if (content.Published)
                return ContentStatus.Published;

            return ContentStatus.Unpublished;
        }



        #endregion

        /// <summary>
        /// Gets the <see cref="IProfile"/> for the Creator of this content item.
        /// </summary>
        public static IProfile GetCreatorProfile(this IContentBase content, IUserService userService)
        {
            return userService.GetProfileById(content.CreatorId);
        }
        /// <summary>
        /// Gets the <see cref="IProfile"/> for the Writer of this content.
        /// </summary>
        public static IProfile GetWriterProfile(this IContent content, IUserService userService)
        {
            return userService.GetProfileById(content.WriterId);
        }

        /// <summary>
        /// Gets the <see cref="IProfile"/> for the Writer of this content.
        /// </summary>
        public static IProfile GetWriterProfile(this IMedia content, IUserService userService)
        {
            return userService.GetProfileById(content.WriterId);
        }
    }
}
