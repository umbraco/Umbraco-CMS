using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Models
{
    internal class NestedPublishedContent : PublishedContentWithKeyBase
    {
        private readonly Guid key;
        private readonly string name;
        private readonly PublishedContentType contentType;
        private readonly IEnumerable<IPublishedProperty> properties;
        private readonly int sortOrder;
        private readonly bool isPreviewing;
        private readonly IPublishedContent owner;

        public NestedPublishedContent(Guid key,
            string name,
            PublishedContentType contentType,
            IEnumerable<IPublishedProperty> properties,
            IPublishedContent owner = null,
            int sortOrder = 0,
            bool isPreviewing = false)
        {
            this.key = key;
            this.name = name;
            this.contentType = contentType;
            this.properties = properties;
            this.sortOrder = sortOrder;
            this.isPreviewing = isPreviewing;
            this.owner = owner;
        }

        public override int Id
        {
            get { return 0; }
        }

        public override Guid Key
        {
            get { return this.key; }
        }

        public override string Name
        {
            get { return this.name; }
        }

        public override bool IsDraft
        {
            get { return this.isPreviewing; }
        }

        public override PublishedItemType ItemType
        {
            get { return PublishedItemType.Content; }
        }

        public override PublishedContentType ContentType
        {
            get { return this.contentType; }
        }

        public override string DocumentTypeAlias
        {
            get { return this.contentType.Alias; }
        }

        public override int DocumentTypeId
        {
            get { return this.contentType.Id; }
        }

        public override ICollection<IPublishedProperty> Properties
        {
            get { return this.properties.ToArray(); }
        }

        public override IPublishedProperty GetProperty(string alias)
        {
            return this.properties.FirstOrDefault(x => x.PropertyTypeAlias.InvariantEquals(alias));
        }

        public override IPublishedProperty GetProperty(string alias, bool recurse)
        {
            if (recurse)
                throw new NotSupportedException();

            return GetProperty(alias);
        }

        public override IPublishedContent Parent
        {
            get { return null; }
        }

        public override IEnumerable<IPublishedContent> Children
        {
            get { return Enumerable.Empty<IPublishedContent>(); }
        }

        public override int TemplateId
        {
            get { return 0; }
        }

        public override int SortOrder
        {
            get { return this.sortOrder; }
        }

        public override string UrlName
        {
            get { return null; }
        }

        public override string WriterName
        {
            get { return this.owner != null ? this.owner.WriterName : null; }
        }

        public override string CreatorName
        {
            get { return this.owner != null ? this.owner.CreatorName : null; }
        }

        public override int WriterId
        {
            get { return this.owner != null ? this.owner.WriterId : 0; }
        }

        public override int CreatorId
        {
            get { return this.owner != null ? this.owner.CreatorId : 0; }
        }

        public override string Path
        {
            get { return null; }
        }

        public override DateTime CreateDate
        {
            get { return this.owner != null ? this.owner.CreateDate : DateTime.MinValue; }
        }

        public override DateTime UpdateDate
        {
            get { return this.owner != null ? this.owner.UpdateDate : DateTime.MinValue; }
        }

        public override Guid Version
        {
            get { return this.owner != null ? this.owner.Version : Guid.Empty; }
        }

        public override int Level
        {
            get { return 0; }
        }
    }
}
