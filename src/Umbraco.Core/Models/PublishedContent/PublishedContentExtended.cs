using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Umbraco.Core.Models.PublishedContent
{

    public class PublishedContentExtended : PublishedContentWrapped, IPublishedContentExtended
    {
        #region Constructor

        // protected for models, private for our static Extend method
        protected PublishedContentExtended(IPublishedContent content)
            : base(content)
        { }

        #endregion

        #region Index

        private int? _index;

        public override int GetIndex()
        {
            // fast
            if (_index.HasValue) return _index.Value;

            // slow -- and don't cache, not in a set
            if (_contentSet == null) return Content.GetIndex();

            // slow -- but cache for next time
            var index = _contentSet.FindIndex(x => x.Id == Id);
            if (index < 0)
                throw new IndexOutOfRangeException("Could not find content in the content set.");
            _index = index;
            return index;
        }

        #endregion

        #region Extend

        internal static IPublishedContentExtended Extend(IPublishedContent content, IEnumerable<IPublishedContent> contentSet)
        {
            var wrapped = content as PublishedContentExtended;
            while (wrapped != null && ((IPublishedContentExtended)wrapped).HasAddedProperties == false)
                wrapped = (content = wrapped.Unwrap()) as PublishedContentExtended;

            // if the factory returns something else than content it means it has created
            // a model, and then that model has to inherit from PublishedContentExtended,
            // => implements the internal IPublishedContentExtended.

            var model = PublishedContentModelFactory.CreateModel(content);
            var extended = model == content // == means the factory did not create a model
              ? new PublishedContentExtended(content) // so we have to extend
              : model; // else we can use what the factory returned

            var extended2 = extended as IPublishedContentExtended;
            if (extended2 != null) // always true, but keeps Resharper happy
                extended2.SetContentSet(contentSet);
            return extended2;
        }

        #endregion

        #region IPublishedContentExtended

        void IPublishedContentExtended.AddProperty(IPublishedProperty property)
        {
            if (_properties == null)
                _properties = new Collection<IPublishedProperty>();
            _properties.Add(property);
        }

        bool IPublishedContentExtended.HasAddedProperties
        {
            get { return _properties != null; }
        }

        void IPublishedContentExtended.SetContentSet(IEnumerable<IPublishedContent> contentSet)
        {
            _contentSet = contentSet;
        }

        void IPublishedContentExtended.ClearContentSet()
        {
            _contentSet = null;
        }

        void IPublishedContentExtended.SetIndex(int value)
        {
            _index = value;
        }

        void IPublishedContentExtended.ClearIndex()
        {
            _index = null;
        }

        #endregion

        #region Content set

        private IEnumerable<IPublishedContent> _contentSet;

        public override IEnumerable<IPublishedContent> ContentSet
        {
            get { return _contentSet ?? Content.ContentSet; }
        }

        #endregion

        #region Properties

        private ICollection<IPublishedProperty> _properties;

        public override ICollection<IPublishedProperty> Properties
        {
            get
            {
                return _properties == null
                    ? Content.Properties
                    : Content.Properties.Union(_properties).ToList();
            }
        }

        public override object this[string alias]
        {
            get
            {
                if (_properties != null)
                {
                    var property = _properties.FirstOrDefault(prop => prop.PropertyTypeAlias.InvariantEquals(alias));
                    if (property != null) return property.HasValue ? property.Value : null;
                }
                return Content[alias];
            }
        }

        public override IPublishedProperty GetProperty(string alias)
        {
            return _properties == null
                ? Content.GetProperty(alias)
                : _properties.FirstOrDefault(prop => prop.PropertyTypeAlias.InvariantEquals(alias)) ?? Content.GetProperty(alias);
        }

        #endregion
    }

}
