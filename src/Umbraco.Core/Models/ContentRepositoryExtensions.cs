using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Exceptions;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Extension methods used to manipulate content variations by the document repository
    /// </summary>
    internal static class ContentRepositoryExtensions
    {
        public static void SetPublishInfo(this IContent content, string culture, string name, DateTime date)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullOrEmptyException(nameof(name));

            if (culture.IsNullOrWhiteSpace())
                throw new ArgumentNullOrEmptyException(nameof(culture));

            content.PublishCultureInfos.AddOrUpdate(culture, name, date);
        }

        // adjust dates to sync between version, cultures etc used by the repo when persisting
        public static void AdjustDates(this IContent content, DateTime date)
        {
            foreach (var culture in content.PublishedCultures.ToList())
            {
                if (!content.PublishCultureInfos.TryGetValue(culture, out var publishInfos))
                    continue;

                //fixme: Removing the logic here for the old WasCulturePublished and the _publishInfosOrig has broken
                // the test Can_Rollback_Version_On_Multilingual, but we need to understand what it's doing since I don't

                content.PublishCultureInfos.AddOrUpdate(culture, publishInfos.Name, date);

                if (content.CultureInfos.TryGetValue(culture, out var infos))
                    SetCultureInfo(content, culture, infos.Name, date);
            }
        }

        // sets the edited cultures on the content
        public static void SetCultureEdited(this IContent content, IEnumerable<string> cultures)
        {
            if (cultures == null)
                content.EditedCultures = null;
            else
            {
                var editedCultures = new HashSet<string>(cultures.Where(x => !x.IsNullOrWhiteSpace()), StringComparer.OrdinalIgnoreCase);
                content.EditedCultures = editedCultures.Count > 0 ? editedCultures : null;
            }
        }

        public static void SetCultureInfo(this IContentBase content, string culture, string name, DateTime date)
        {
            if (name.IsNullOrWhiteSpace())
                throw new ArgumentNullOrEmptyException(nameof(name));

            if (culture.IsNullOrWhiteSpace())
                throw new ArgumentNullOrEmptyException(nameof(culture));

            content.CultureInfos.AddOrUpdate(culture, name, date);
        }

        public static bool PublishCulture(this IContent content, string culture = "*")
        {
            culture = culture.NullOrWhiteSpaceAsNull();

            // the variation should be supported by the content type properties
            //  if the content type is invariant, only '*' and 'null' is ok
            //  if the content type varies, everything is ok because some properties may be invariant
            if (!content.ContentType.SupportsPropertyVariation(culture, "*", true))
                throw new NotSupportedException($"Culture \"{culture}\" is not supported by content type \"{content.ContentType.Alias}\" with variation \"{content.ContentType.Variations}\".");

            // the values we want to publish should be valid
            if (content.ValidateProperties(culture).Any())
                return false;

            var alsoInvariant = false;
            if (culture == "*") // all cultures
            {
                foreach (var c in content.AvailableCultures)
                {
                    var name = content.GetCultureName(c);
                    if (string.IsNullOrWhiteSpace(name))
                        return false;
                    content.SetPublishInfo(c, name, DateTime.Now);
                }
            }
            else if (culture == null) // invariant culture
            {
                if (string.IsNullOrWhiteSpace(content.Name))
                    return false;
                // PublishName set by repository - nothing to do here
            }
            else // one single culture
            {
                var name = content.GetCultureName(culture);
                if (string.IsNullOrWhiteSpace(name))
                    return false;
                content.SetPublishInfo(culture, name, DateTime.Now);
                alsoInvariant = true; // we also want to publish invariant values
            }

            // property.PublishValues only publishes what is valid, variation-wise
            foreach (var property in content.Properties)
            {
                property.PublishValues(culture);
                if (alsoInvariant)
                    property.PublishValues(null);
            }

            content.PublishedState = PublishedState.Publishing;
            return true;
        }

        public static void UnpublishCulture(this IContent content, string culture = "*")
        {
            culture = culture.NullOrWhiteSpaceAsNull();

            // the variation should be supported by the content type properties
            if (!content.ContentType.SupportsPropertyVariation(culture, "*", true))
                throw new NotSupportedException($"Culture \"{culture}\" is not supported by content type \"{content.ContentType.Alias}\" with variation \"{content.ContentType.Variations}\".");

            if (culture == "*") // all cultures
                content.ClearPublishInfos();
            else // one single culture
                content.ClearPublishInfo(culture);

            // property.PublishValues only publishes what is valid, variation-wise
            foreach (var property in content.Properties)
                property.UnpublishValues(culture);

            content.PublishedState = PublishedState.Publishing;
        }

        public static void ClearPublishInfos(this IContent content)
        {
            content.PublishCultureInfos = null;
        }

        public static void ClearPublishInfo(this IContent content, string culture)
        {
            if (culture.IsNullOrWhiteSpace())
                throw new ArgumentNullOrEmptyException(nameof(culture));

            content.PublishCultureInfos.Remove(culture);

            // set the culture to be dirty - it's been modified
            content.TouchCultureInfo(culture);
        }

        public static void TouchCultureInfo(this IContent content, string culture)
        {
            if (!content.CultureInfos.TryGetValue(culture, out var infos)) return;
            content.CultureInfos.AddOrUpdate(culture, infos.Name, DateTime.Now);
        }
    }
}
