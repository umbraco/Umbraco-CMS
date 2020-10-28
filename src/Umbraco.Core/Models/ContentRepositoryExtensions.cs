using System;
using System.Linq;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Core.Models
{
    [UmbracoVolatile]
    public static class ContentRepositoryExtensions
    {
        public static void SetCultureInfo(this IContentBase content, string culture, string name, DateTime date)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(name));

            if (culture == null) throw new ArgumentNullException(nameof(culture));
            if (string.IsNullOrWhiteSpace(culture)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(culture));

            content.CultureInfos.AddOrUpdate(culture, name, date);
        }

        /// <summary>
        /// Updates a culture date, if the culture exists.
        /// </summary>
        public static void TouchCulture(this IContentBase content, string culture)
        {
            if (culture.IsNullOrWhiteSpace()) return;
            if (!content.CultureInfos.TryGetValue(culture, out var infos)) return;
            content.CultureInfos.AddOrUpdate(culture, infos.Name, DateTime.Now);
        }

        /// <summary>
        /// Used to synchronize all culture dates to the same date if they've been modified
        /// </summary>
        /// <param name="content"></param>
        /// <param name="date"></param>
        /// <remarks>
        /// This is so that in an operation where (for example) 2 languages are updates like french and english, it is possible that
        /// these dates assigned to them differ by a couple of Ticks, but we need to ensure they are persisted at the exact same time.
        /// </remarks>
        public static void AdjustDates(this IContent content, DateTime date)
        {
            foreach (var culture in content.PublishedCultures.ToList())
            {
                if (!content.PublishCultureInfos.TryGetValue(culture, out var publishInfos))
                    continue;

                // if it's not dirty, it means it hasn't changed so there's nothing to adjust
                if (!publishInfos.IsDirty())
                    continue;

                content.PublishCultureInfos.AddOrUpdate(culture, publishInfos.Name, date);

                if (content.CultureInfos.TryGetValue(culture, out var infos))
                    SetCultureInfo(content, culture, infos.Name, date);
            }
        }
    }

}
