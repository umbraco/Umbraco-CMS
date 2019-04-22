using System;
using System.Collections.Generic;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Tests.TestHelpers.Stubs
{
    internal class TestPublishedContent : PublishedElement, IPublishedContent
    {
        private readonly Dictionary<string, string> _names = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _urlSegments = new Dictionary<string, string>();
        private readonly Dictionary<string, DateTime> _cultures;

        public TestPublishedContent(IPublishedContentType contentType, int id, Guid key, Dictionary<string, object> values, bool previewing, Dictionary<string, DateTime> cultures = null)
            : base(contentType, key, values, previewing)
        {
            Id = id;
            _cultures = cultures ??  new Dictionary<string, DateTime>();
        }

        public int Id { get; }
        public int? TemplateId { get; set; }
        public int SortOrder { get; set; }
        public string Name(string culture = null) => _names.TryGetValue(culture ?? "", out var name) ? name : null;
        public void SetName(string name, string culture = null) => _names[culture ?? ""] = name;
        public IVariationContextAccessor VariationContextAccessor { get; set; }
        public DateTime CultureDate(string culture = null)
        {
            // handle context culture
            if (culture == null)
                culture = VariationContextAccessor?.VariationContext?.Culture;

            // no invariant culture infos
            if (culture == "" || Cultures == null) return UpdateDate;

            // get
            return _cultures.TryGetValue(culture, out var date) ? date : DateTime.MinValue;
        }
        public IReadOnlyList<string> Cultures { get; set; }
        public string UrlSegment(string culture = null) => _urlSegments.TryGetValue(culture ?? "", out var urlSegment) ? urlSegment : null;
        public void SetUrlSegment(string urlSegment, string culture = null) => _urlSegments[culture ?? ""] = urlSegment;
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
        public string Url(string culture = null, UrlMode mode = UrlMode.Auto) => throw new NotSupportedException();
        public bool IsDraft(string culture = null) => false;
        public bool IsPublished(string culture = null) => true;
        private IPublishedContent _parent;
        public IPublishedContent Parent() => _parent;
        public void SetParent(IPublishedContent parent) => _parent = parent;
        private IEnumerable<IPublishedContent> _children;
        public IEnumerable<IPublishedContent> Children(string culture = null) => _children;
        public void SetChildren(IEnumerable<IPublishedContent> children) => _children = children;

        // copied from PublishedContentBase
        public IPublishedProperty GetProperty(string alias, bool recurse)
        {
            var property = GetProperty(alias);
            if (recurse == false) return property;

            IPublishedContent content = this;
            var firstNonNullProperty = property;
            while (content != null && (property == null || property.HasValue() == false))
            {
                content = content.Parent();
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
