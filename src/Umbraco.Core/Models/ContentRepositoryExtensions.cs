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
        /// <summary>
        /// Gets the cultures that have been flagged for unpublishing.
        /// </summary>
        /// <remarks>Gets cultures for which content.UnpublishCulture() has been invoked.</remarks>
        public static IReadOnlyList<string> GetCulturesUnpublishing(this IContent content)
        {
            if (!content.Published || !content.ContentType.VariesByCulture() || !content.IsPropertyDirty("PublishCultureInfos"))
                return Array.Empty<string>();

            var culturesUnpublishing = content.CultureInfos.Values
                .Where(x => content.IsPropertyDirty("_unpublishedCulture_" + x.Culture))
                .Select(x => x.Culture);

            return culturesUnpublishing.ToList();
        }

        /// <summary>
        /// Copies values from another document.
        /// </summary>
        public static void CopyFrom(this IContent content, IContent other, string culture = "*")
        {
            if (other.ContentTypeId != content.ContentTypeId)
                throw new InvalidOperationException("Cannot copy values from a different content type.");

            culture = culture?.ToLowerInvariant().NullOrWhiteSpaceAsNull();

            // the variation should be supported by the content type properties
            //  if the content type is invariant, only '*' and 'null' is ok
            //  if the content type varies, everything is ok because some properties may be invariant
            if (!content.ContentType.SupportsPropertyVariation(culture, "*", true))
                throw new NotSupportedException($"Culture \"{culture}\" is not supported by content type \"{content.ContentType.Alias}\" with variation \"{content.ContentType.Variations}\".");

            // copying from the same Id and VersionPk
            var copyingFromSelf = content.Id == other.Id && content.VersionId == other.VersionId;
            var published = copyingFromSelf;

            // note: use property.SetValue(), don't assign pvalue.EditValue, else change tracking fails

            // clear all existing properties for the specified culture
            foreach (var property in content.Properties)
            {
                // each property type may or may not support the variation
                if (!property.PropertyType.SupportsVariation(culture, "*", wildcards: true))
                    continue;

                foreach (var pvalue in property.Values)
                    if (property.PropertyType.SupportsVariation(pvalue.Culture, pvalue.Segment, wildcards: true) &&
                        (culture == "*" || pvalue.Culture.InvariantEquals(culture)))
                    {
                        property.SetValue(null, pvalue.Culture, pvalue.Segment);
                    }
            }

            // copy properties from 'other'
            var otherProperties = other.Properties;
            foreach (var otherProperty in otherProperties)
            {
                if (!otherProperty.PropertyType.SupportsVariation(culture, "*", wildcards: true))
                    continue;

                var alias = otherProperty.PropertyType.Alias;
                foreach (var pvalue in otherProperty.Values)
                {
                    if (otherProperty.PropertyType.SupportsVariation(pvalue.Culture, pvalue.Segment, wildcards: true) &&
                        (culture == "*" || pvalue.Culture.InvariantEquals(culture)))
                    {
                        var value = published ? pvalue.PublishedValue : pvalue.EditedValue;
                        content.SetValue(alias, value, pvalue.Culture, pvalue.Segment);
                    }
                }
            }

            // copy names, too

            if (culture == "*")
            {
                content.CultureInfos.Clear();
                content.CultureInfos = null;
            }
                

            if (culture == null || culture == "*")
                content.Name = other.Name;

            // ReSharper disable once UseDeconstruction
            foreach (var cultureInfo in other.CultureInfos)
            {
                if (culture == "*" || culture == cultureInfo.Culture)
                    content.SetCultureName(cultureInfo.Name, cultureInfo.Culture);
            }
        }

        /// <summary>
        /// Validates the content item's properties pass variant rules
        /// </summary>
        /// <para>If the content type is variant, then culture can be either '*' or an actual culture, but neither 'null' nor
        /// 'empty'. If the content type is invariant, then culture can be either '*' or null or empty.</para>
        public static Property[] ValidateProperties(this IContentBase content, string culture = "*")
        {
            // select invalid properties
            return content.Properties.Where(x =>
                {
                    // if culture is null, we validate invariant properties only
                    // if culture is '*' we validate both variant and invariant properties, automatically
                    // if culture is specific eg 'en-US' we both too, but explicitly

                    var varies = x.PropertyType.VariesByCulture();

                    if (culture == null)
                        return !(varies || x.IsValid(null)); // validate invariant property, invariant culture

                    if (culture == "*")
                        return !x.IsValid(culture); // validate property, all cultures

                    return varies
                        ? !x.IsValid(culture) // validate variant property, explicit culture
                        : !x.IsValid(null); // validate invariant property, explicit culture
                })
                .ToArray();
        }

        public static void SetPublishInfo(this IContent content, string culture, string name, DateTime date)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullOrEmptyException(nameof(name));

            if (culture.IsNullOrWhiteSpace())
                throw new ArgumentNullOrEmptyException(nameof(culture));

            content.PublishCultureInfos.AddOrUpdate(culture, name, date);
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

                if (!publishInfos.IsDirty())
                    continue; //if it's not dirty, it means it hasn't changed so there's nothing to adjust
                
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
            content.TouchCulture(culture);
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
    }
}
