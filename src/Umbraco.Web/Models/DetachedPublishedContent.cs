using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Models
{
    public class DetachedPublishedContent : PublishedContentBase
    {
        private readonly PublishedContentType _contentType;
        private readonly IEnumerable<IPublishedProperty> _properties;
        private readonly IPublishedContent _containerNode;

        public DetachedPublishedContent(
            Guid key,
            string name,
            PublishedContentType contentType,
            IEnumerable<IPublishedProperty> properties,
            IPublishedContent containerNode = null,
            int sortOrder = 0,
            bool isPreviewing = false)
        {
            Key = key;
            Name = name;
            _contentType = contentType;
            _properties = properties;
            SortOrder = sortOrder;
            IsDraft = isPreviewing;
            _containerNode = containerNode;
        }

        public override Guid Key { get; }

        public override int Id => 0;

        public override string Name { get; }

        public override bool IsDraft { get; }

        public override PublishedItemType ItemType => PublishedItemType.Content;

        public override PublishedContentType ContentType => _contentType;

        public override string DocumentTypeAlias => _contentType.Alias;

        public override int DocumentTypeId => _contentType.Id;

        public override IEnumerable<IPublishedProperty> Properties => _properties.ToArray();

        public override IPublishedProperty GetProperty(string alias)
            => _properties.FirstOrDefault(x => x.PropertyTypeAlias.InvariantEquals(alias));

        public override IPublishedProperty GetProperty(string alias, bool recurse)
        {
            if (recurse)
                throw new NotSupportedException();

            return GetProperty(alias);
        }

        public override IPublishedContent Parent => null;

        public override IEnumerable<IPublishedContent> Children => Enumerable.Empty<IPublishedContent>();

        public override int TemplateId => 0;

        public override int SortOrder { get; }

        public override string UrlName => null;

        public override string WriterName => _containerNode?.WriterName;

        public override string CreatorName => _containerNode?.CreatorName;

        public override int WriterId => _containerNode?.WriterId ?? 0;

        public override int CreatorId => _containerNode?.CreatorId ?? 0;

        public override string Path => null;

        public override DateTime CreateDate => _containerNode?.CreateDate ?? DateTime.MinValue;

        public override DateTime UpdateDate => _containerNode?.UpdateDate ?? DateTime.MinValue;

        public override Guid Version => _containerNode?.Version ?? Guid.Empty;

        public override int Level => 0;
    }
}
