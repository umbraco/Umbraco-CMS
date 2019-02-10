using System;
using System.Collections.Generic;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Tests.TestHelpers.Stubs
{
    internal class TestPublishedContent : PublishedElement, IPublishedContent
    {
        public TestPublishedContent(PublishedContentType contentType, int id, Guid key, Dictionary<string, object> values, bool previewing, Dictionary<string, PublishedCultureInfo> cultures = null)
            : base(contentType, key, values, previewing)
        {
            Id = id;
            Cultures = cultures;
        }

        public int Id { get; }
        public int? TemplateId { get; set; }
        public int SortOrder { get; set; }
        public string Name { get; set; }
        public IVariationContextAccessor VariationContextAccessor { get; set; }
        public PublishedCultureInfo GetCulture(string culture = null)
        {
            // handle context culture
            if (culture == null)
                culture = VariationContextAccessor?.VariationContext?.Culture;

            // no invariant culture infos
            if (culture == "" || Cultures == null) return null;

            // get
            return Cultures.TryGetValue(culture, out var cultureInfos) ? cultureInfos : null;
        }
        public IReadOnlyDictionary<string, PublishedCultureInfo> Cultures { get; set; }
        public string UrlSegment { get; set; }
        public string DocumentTypeAlias => ContentType.Alias;
        public int DocumentTypeId { get; set; }
        public string WriterName { get; set; }
        public string CreatorName { get; set; }
        public int WriterId { get; set; }
        public int CreatorId { get; set; }
        public string Path { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public Guid Version { get; set; }
        public int Level { get; set; }
        public string Url { get; set; }
        public string GetUrl(string culture = null) => throw new NotSupportedException();
        public PublishedItemType ItemType => ContentType.ItemType;
        public bool IsDraft(string culture = null) => false;
        public bool IsPublished(string culture = null) => true;
        public IPublishedContent Parent { get; set; }
        public IEnumerable<IPublishedContent> Children { get; set; }

        // copied from PublishedContentBase
        public IPublishedProperty GetProperty(string alias, bool recurse)
        {
            var property = GetProperty(alias);
            if (recurse == false) return property;

            IPublishedContent content = this;
            var firstNonNullProperty = property;
            while (content != null && (property == null || property.HasValue() == false))
            {
                content = content.Parent;
                property = content?.GetProperty(alias);
                if (firstNonNullProperty == null && property != null) firstNonNullProperty = property;
            }

            // if we find a content with the property with a value, return that property
            // if we find no content with the property, return null
            // if we find a content with the property without a value, return that property
            //   have to save that first property while we look further up, hence firstNonNullProperty

            return property != null && property.HasValue() ? property : firstNonNullProperty;
        }
    }
}
