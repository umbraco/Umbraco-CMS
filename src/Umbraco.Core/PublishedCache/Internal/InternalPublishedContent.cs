using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PublishedCache.Internal
{
    // TODO: Only used in unit tests, needs to be moved to test project
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class InternalPublishedContent : IPublishedContent
    {
        public InternalPublishedContent(IPublishedContentType contentType)
        {
            // initialize boring stuff
            TemplateId = 0;
            WriterId = CreatorId = 0;
            CreateDate = UpdateDate = DateTime.Now;
            Version = Guid.Empty;
            Path = string.Empty;
            ContentType = contentType;
        }

        private Dictionary<string, PublishedCultureInfo>? _cultures;

        private Dictionary<string, PublishedCultureInfo> GetCultures() => new Dictionary<string, PublishedCultureInfo> { { string.Empty, new PublishedCultureInfo(string.Empty, Name, UrlSegment, UpdateDate) } };

        public int Id { get; set; }

        public Guid Key { get; set; }

        public int? TemplateId { get; set; }

        public int SortOrder { get; set; }

        public string? Name { get; set; }

        public IReadOnlyDictionary<string, PublishedCultureInfo> Cultures => _cultures ??= GetCultures();

        public string? UrlSegment { get; set; }

        public int WriterId { get; set; }

        public int CreatorId { get; set; }

        public string Path { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime UpdateDate { get; set; }

        public Guid Version { get; set; }

        public int Level { get; set; }

        public PublishedItemType ItemType => PublishedItemType.Content;

        public bool IsDraft(string? culture = null) => false;

        public bool IsPublished(string? culture = null) => true;

        public int ParentId { get; set; }

        public IEnumerable<int>? ChildIds { get; set; }

        public IPublishedContent? Parent { get; set; }

        public IEnumerable<IPublishedContent>? Children { get; set; }

        public IEnumerable<IPublishedContent>? ChildrenForAllCultures => Children;

        public IPublishedContentType ContentType { get; set; }

        public IEnumerable<IPublishedProperty> Properties { get; set; }

        public IPublishedProperty? GetProperty(string alias) => Properties?.FirstOrDefault(p => p.Alias.InvariantEquals(alias));

        public IPublishedProperty? GetProperty(string alias, bool recurse)
        {
            IPublishedProperty? property = GetProperty(alias);
            if (recurse == false)
            {
                return property;
            }

            IPublishedContent? content = this;
            while (content != null && (property == null || property.HasValue() == false))
            {
                content = content.Parent;
                property = content?.GetProperty(alias);
            }

            return property;
        }

        public object? this[string alias]
        {
            get
            {
                var property = GetProperty(alias);
                return property == null || property.HasValue() == false ? null : property.GetValue();
            }
        }
    }
}
