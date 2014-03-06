using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Models;

namespace Umbraco.Web.PublishedCache.XmlPublishedCache
{
    class PublishedFragment : PublishedContentBase
    {
        private readonly PublishedContentType _contentType;
        private readonly IPublishedProperty[] _properties;

        public PublishedFragment(string contentTypeAlias, IDictionary<string, object> dataValues,
            bool isPreviewing, bool managed)
        {
            IsPreviewing = isPreviewing;
            _contentType = PublishedContentType.Get(PublishedItemType.Content, contentTypeAlias);

            // we don't care about managed because in both cases, XmlPublishedCache stores
            // converted property values in the IPublishedContent, which is not meant to
            // survive the request

            var dataValues2 = new Dictionary<string, object>();
            foreach (var kvp in dataValues)
                dataValues2[kvp.Key.ToLowerInvariant()] = kvp.Value;

            _properties = _contentType.PropertyTypes
                .Select(x =>
                {
                    object dataValue;
                    return dataValues2.TryGetValue(x.PropertyTypeAlias.ToLowerInvariant(), out dataValue)
                        ? new PublishedFragmentProperty(x, this, dataValue)
                        : new PublishedFragmentProperty(x, this);
                })
                .Cast<IPublishedProperty>()
                .ToArray();
        }

        #region IPublishedContent

        public override int Id
        {
            get { throw new NotImplementedException(); }
        }

        public override int DocumentTypeId
        {
            get { return _contentType.Id; }
        }

        public override string DocumentTypeAlias
        {
            get { return _contentType.Alias; }
        }

        public override PublishedItemType ItemType
        {
            get { return PublishedItemType.Content; }
        }

        public override string Name
        {
            get { throw new NotImplementedException(); }
        }

        public override int Level
        {
            get { throw new NotImplementedException(); }
        }

        public override string Path
        {
            get { throw new NotImplementedException(); }
        }

        public override int SortOrder
        {
            // note - could a published fragment have a sort order?
            get { throw new NotImplementedException(); }
        }

        public override Guid Version
        {
            get { throw new NotImplementedException(); }
        }

        public override int TemplateId
        {
            get { throw new NotImplementedException(); }
        }

        public override string UrlName
        {
            get { return string.Empty; }
        }

        public override DateTime CreateDate
        {
            get { throw new NotImplementedException(); }
        }

        public override DateTime UpdateDate
        {
            get { throw new NotImplementedException(); }
        }

        public override int CreatorId
        {
            get { throw new NotImplementedException(); }
        }

        public override string CreatorName
        {
            get { throw new NotImplementedException(); }
        }

        public override int WriterId
        {
            get { throw new NotImplementedException(); }
        }

        public override string WriterName
        {
            get { throw new NotImplementedException(); }
        }

        public override bool IsDraft
        {
            get { throw new NotImplementedException(); }
        }

        public override IPublishedContent Parent
        {
            get { throw new NotImplementedException(); }
        }

        public override IEnumerable<IPublishedContent> Children
        {
            get { throw new NotImplementedException(); }
        }

        public override ICollection<IPublishedProperty> Properties
        {
            get { return _properties; }
        }

        public override IPublishedProperty GetProperty(string alias)
        {
            return _properties.FirstOrDefault(x => x.PropertyTypeAlias.InvariantEquals(alias));
        }

        public override PublishedContentType ContentType
        {
            get { return _contentType; }
        }

        #endregion

        #region Internal

        // used by PublishedFragmentProperty
        internal bool IsPreviewing { get; private set; }

        #endregion
        }
}
