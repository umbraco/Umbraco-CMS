using System;
using System.Collections.Generic;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Tests.Published
{
    public class PublishedSnapshotTestObjects
    {
        #region Published models

        [PublishedModel("element1")]
        public class TestElementModel1 : PublishedElementModel
        {
            public TestElementModel1(IPublishedElement content)
                : base(content)
            { }

            public string Prop1 => this.Value<string>("prop1");
        }

        [PublishedModel("element2")]
        public class TestElementModel2 : PublishedElementModel
        {
            public TestElementModel2(IPublishedElement content)
                : base(content)
            { }

            public IEnumerable<TestContentModel1> Prop2 => this.Value<IEnumerable<TestContentModel1>>("prop2");
        }

        [PublishedModel("content1")]
        public class TestContentModel1 : PublishedContentModel
        {
            public TestContentModel1(IPublishedContent content)
                : base(content)
            { }

            public string Prop1 => this.Value<string>("prop1");
        }

        [PublishedModel("content2")]
        public class TestContentModel2 : PublishedContentModel
        {
            public TestContentModel2(IPublishedContent content)
                : base(content)
            { }

            public IEnumerable<TestContentModel1> Prop2 => this.Value<IEnumerable<TestContentModel1>>("prop2");
        }

        #endregion

        #region Support classes

        internal class TestPublishedContent : PublishedElement, IPublishedContent
        {
            public TestPublishedContent(PublishedContentType contentType, int id, Guid key, Dictionary<string, object> values, bool previewing)
                : base(contentType, key, values, previewing)
            {
                Id = id;
            }

            public int Id { get; }
            public int TemplateId { get; set; }
            public int SortOrder { get; set; }
            public string Name { get; set; }
            public IReadOnlyDictionary<string, PublishedCultureName> CultureNames => throw new NotSupportedException();
            public string UrlName { get; set; }
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
            public PublishedItemType ItemType => ContentType.ItemType;
            public bool IsDraft { get; set; }
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

        #endregion
    }
}
