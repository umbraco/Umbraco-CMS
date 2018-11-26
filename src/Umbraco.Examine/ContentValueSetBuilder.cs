using Examine;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;

namespace Umbraco.Examine
{
    public class ContentValueSetBuilder : BaseValueSetBuilder, IValueSetBuilder<IContent>
    {
        private readonly PropertyEditorCollection _propertyEditors;
        private readonly IEnumerable<IUrlSegmentProvider> _urlSegmentProviders;
        private readonly IUserService _userService;

        public ContentValueSetBuilder(PropertyEditorCollection propertyEditors,
            IEnumerable<IUrlSegmentProvider> urlSegmentProviders,
            IUserService userService)
            : base(propertyEditors)
        {
            _propertyEditors = propertyEditors;
            _urlSegmentProviders = urlSegmentProviders;
            _userService = userService;
        }

        /// <summary>
        /// Creates a collection of <see cref="ValueSet"/> for a <see cref="IContent"/> collection
        /// </summary>
        /// <param name="urlSegmentProviders"></param>
        /// <param name="userService"></param>
        /// <param name="content"></param>
        /// <returns>Yield returns <see cref="ValueSet"/></returns>
        public IEnumerable<ValueSet> GetValueSets(params IContent[] content)
        {
            //TODO: There is a lot of boxing going on here and ultimately all values will be boxed by Lucene anyways
            // but I wonder if there's a way to reduce the boxing that we have to do or if it will matter in the end since
            // Lucene will do it no matter what? One idea was to create a `FieldValue` struct which would contain `object`, `object[]`, `ValueType` and `ValueType[]`
            // references and then each array is an array of `FieldValue[]` and values are assigned accordingly. Not sure if it will make a difference or not.

            foreach (var c in content)
            {
                var isVariant = c.ContentType.VariesByCulture();

                var urlValue = c.GetUrlSegment(_urlSegmentProviders); //Always add invariant urlName
                var values = new Dictionary<string, object[]>
                {
                    {"icon", new [] {c.ContentType.Icon}},
                    {UmbracoExamineIndexer.PublishedFieldName, new object[] {c.Published ? 1 : 0}},   //Always add invariant published value
                    {"id", new object[] {c.Id}},
                    {"key", new object[] {c.Key}},
                    {"parentID", new object[] {c.Level > 1 ? c.ParentId : -1}},
                    {"level", new object[] {c.Level}},
                    {"creatorID", new object[] {c.CreatorId}},
                    {"sortOrder", new object[] {c.SortOrder}},
                    {"createDate", new object[] {c.CreateDate}},    //Always add invariant createDate
                    {"updateDate", new object[] {c.UpdateDate}},    //Always add invariant updateDate
                    {"nodeName", new object[] {c.Name}},            //Always add invariant nodeName
                    {"urlName", new object[] {urlValue}},           //Always add invariant urlName
                    {"path", new object[] {c.Path}},
                    {"nodeType", new object[] {c.ContentType.Id}},
                    {"creatorName", new object[] {c.GetCreatorProfile(_userService)?.Name ?? "??"}},
                    {"writerName", new object[] {c.GetWriterProfile(_userService)?.Name ?? "??"}},
                    {"writerID", new object[] {c.WriterId}},
                    {"template", new object[] {c.Template?.Id ?? 0}},
                    {$"{UmbracoExamineIndexer.SpecialFieldPrefix}VariesByCulture", new object[] {0}},
                };

                if (isVariant)
                {
                    values[$"{UmbracoExamineIndexer.SpecialFieldPrefix}VariesByCulture"] = new object[] { 1 };

                    foreach (var culture in c.AvailableCultures)
                    {
                        var variantUrl = c.GetUrlSegment(_urlSegmentProviders, culture);
                        var lowerCulture = culture.ToLowerInvariant();
                        values[$"urlName_{lowerCulture}"] = new object[] { variantUrl };
                        values[$"nodeName_{lowerCulture}"] = new object[] { c.GetCultureName(culture) };
                        values[$"{UmbracoExamineIndexer.PublishedFieldName}_{lowerCulture}"] = new object[] { c.IsCulturePublished(culture) ? 1 : 0 };
                        values[$"updateDate_{lowerCulture}"] = new object[] { c.GetUpdateDate(culture) };
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
