﻿using Examine;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;

namespace Umbraco.Examine
{
    /// <summary>
    /// Builds <see cref="ValueSet"/>s for <see cref="IContent"/> items
    /// </summary>
    public class ContentValueSetBuilder : BaseValueSetBuilder<IContent>, IContentValueSetBuilder, IPublishedContentValueSetBuilder
    {
        private readonly UrlSegmentProviderCollection _urlSegmentProviders;
        private readonly IUserService _userService;

        public ContentValueSetBuilder(PropertyEditorCollection propertyEditors,
            UrlSegmentProviderCollection urlSegmentProviders,
            IUserService userService,
            bool publishedValuesOnly)
            : base(propertyEditors, publishedValuesOnly)
        {
            _urlSegmentProviders = urlSegmentProviders;
            _userService = userService;
        }

        /// <inheritdoc />
        public override IEnumerable<ValueSet> GetValueSets(params IContent[] content)
        {
            // TODO: There is a lot of boxing going on here and ultimately all values will be boxed by Lucene anyways
            // but I wonder if there's a way to reduce the boxing that we have to do or if it will matter in the end since
            // Lucene will do it no matter what? One idea was to create a `FieldValue` struct which would contain `object`, `object[]`, `ValueType` and `ValueType[]`
            // references and then each array is an array of `FieldValue[]` and values are assigned accordingly. Not sure if it will make a difference or not.

            foreach (var c in content)
            {
                var isVariant = c.ContentType.VariesByCulture();

                var urlValue = c.GetUrlSegment(_urlSegmentProviders); //Always add invariant urlName
                var values = new Dictionary<string, IEnumerable<object>>
                {
                    {"icon", c.ContentType.Icon?.Yield() ?? Enumerable.Empty<string>()},
                    {UmbracoExamineIndex.PublishedFieldName, new object[] {c.Published ? "y" : "n"}},   //Always add invariant published value
                    {"id", new object[] {c.Id}},
                    {UmbracoExamineIndex.NodeKeyFieldName, new object[] {c.Key}},
                    {"parentID", new object[] {c.Level > 1 ? c.ParentId : -1}},
                    {"level", new object[] {c.Level}},
                    {"creatorID", new object[] {c.CreatorId}},
                    {"sortOrder", new object[] {c.SortOrder}},
                    {"createDate", new object[] {c.CreateDate}},    //Always add invariant createDate
                    {"updateDate", new object[] {c.UpdateDate}},    //Always add invariant updateDate
                    {"nodeName", (PublishedValuesOnly               //Always add invariant nodeName
                        ? c.PublishName?.Yield()
                        : c.Name?.Yield()) ?? Enumerable.Empty<string>()},
                    {"urlName", urlValue?.Yield() ?? Enumerable.Empty<string>()},                  //Always add invariant urlName
                    {"path", c.Path?.Yield() ?? Enumerable.Empty<string>()},
                    {"nodeType", c.ContentType.Id.ToString().Yield() ?? Enumerable.Empty<string>()},
                    {"creatorName", (c.GetCreatorProfile(_userService)?.Name ?? "??").Yield() },
                    {"writerName",(c.GetWriterProfile(_userService)?.Name ?? "??").Yield() },
                    {"writerID", new object[] {c.WriterId}},
                    {"templateID", new object[] {c.TemplateId ?? 0}},
                    {UmbracoContentIndex.VariesByCultureFieldName, new object[] {"n"}},
                };

                if (isVariant)
                {
                    values[UmbracoContentIndex.VariesByCultureFieldName] = new object[] { "y" };

                    foreach (var culture in c.AvailableCultures)
                    {
                        var variantUrl = c.GetUrlSegment(_urlSegmentProviders, culture);
                        var lowerCulture = culture.ToLowerInvariant();
                        values[$"urlName_{lowerCulture}"] = variantUrl?.Yield() ?? Enumerable.Empty<string>();
                        values[$"nodeName_{lowerCulture}"] = (PublishedValuesOnly
                            ? c.GetPublishName(culture)?.Yield()
                            : c.GetCultureName(culture)?.Yield()) ?? Enumerable.Empty<string>();
                        values[$"{UmbracoExamineIndex.PublishedFieldName}_{lowerCulture}"] = (c.IsCulturePublished(culture) ? "y" : "n").Yield<object>();
                        values[$"updateDate_{lowerCulture}"] = (PublishedValuesOnly
                            ? c.GetPublishDate(culture)
                            : c.GetUpdateDate(culture))?.Yield<object>() ?? Enumerable.Empty<object>();
                    }
                }

                foreach (var property in c.Properties)
                {
                    if (!property.PropertyType.VariesByCulture())
                    {
                        AddPropertyValue(property, null, null, values);
                    }
                    else
                    {
                        foreach (var culture in c.AvailableCultures)
                            AddPropertyValue(property, culture.ToLowerInvariant(), null, values);
                    }
                }

                var vs = new ValueSet(c.Id.ToInvariantString(), IndexTypes.Content, c.ContentType.Alias, values);

                yield return vs;
            }
        }
    }

}
