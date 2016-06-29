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

        #region Extend

        private static IPublishedContent Unwrap(IPublishedContent content)
        {
            if (content == null) return null;

            while (true)
            {
                var extended = content as PublishedContentExtended;
                if (extended != null)
                {
                    if (((IPublishedContentExtended)extended).HasAddedProperties) return extended;
                    content = extended.Unwrap();
                    continue;
                }
                var wrapped = content as PublishedContentWrapped;
                if (wrapped != null)
                {
                    content = wrapped.Unwrap();
                    continue;
                }
                return content;
            }
        }

        internal static IPublishedContentExtended Extend(IPublishedContent content)
        {
            // first unwrap content down to the lowest possible level, ie either the deepest inner
            // IPublishedContent or the first extended that has added properties. this is to avoid
            // nesting extended objects as much as possible, so we try to re-extend that lowest
            // object. Then extend. But do NOT create a model, else we would not be able to add
            // properties. BEWARE means that whatever calls Extend MUST create the model.

            return new PublishedContentExtended(Unwrap(content));
        }

        #endregion

        #region IPublishedContentExtended

        void IPublishedContentExtended.AddProperty(IPublishedProperty property)
        {
            if (_properties == null)
                _properties = new Collection<IPublishedProperty>();
            _properties.Add(property);
        }

        bool IPublishedContentExtended.HasAddedProperties => _properties != null;

        #endregion

        #region Properties

        private ICollection<IPublishedProperty> _properties;

        public override IEnumerable<IPublishedProperty> Properties => _properties == null
            ? Content.Properties
            : Content.Properties.Union(_properties).ToList();

        public override IPublishedProperty GetProperty(string alias)
        {
            return _properties == null
                ? Content.GetProperty(alias)
                : _properties.FirstOrDefault(prop => prop.PropertyTypeAlias.InvariantEquals(alias)) ?? Content.GetProperty(alias);
        }

        #endregion
    }

}
