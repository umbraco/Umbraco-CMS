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

        internal static IPublishedContentExtended Extend(IPublishedContent content)
        {
            // first unwrap content down to the lowest possible level, ie either the deepest inner
            // IPublishedContent or the first extended that has added properties. this is to avoid
            // nesting extended objects as much as possible, so we try to re-extend that lowest
            // object.

            var wrapped = content as PublishedContentExtended;
            while (wrapped != null && ((IPublishedContentExtended)wrapped).HasAddedProperties == false)
                wrapped = (content = wrapped.Unwrap()) as PublishedContentExtended;

            // if the factory returns something else than content it means it has created
            // a model, and then that model has to inherit from PublishedContentExtended,
            // => implements the internal IPublishedContentExtended.

            // here we assume that either the factory just created a model that implements
            // IPublishedContentExtended and therefore does not need to be extended again,
            // because it can carry the extra property - or that it did *not* create a
            // model and therefore returned the original content unchanged.

            var model = content.CreateModel();
            IPublishedContent extended;
            if (model == content) // == means the factory did not create a model
            {
                // so we have to extend
                extended = new PublishedContentExtended(content);
            }
            else
            {
                // else we can use what the factory returned
                extended = model;
            }

            // so extended should always implement IPublishedContentExtended, however if
            // by mistake the factory returned a different object that does not implement
            // IPublishedContentExtended (which would be an error), throw.
            //
            // see also PublishedContentExtensionsForModels.CreateModel

            // NOTE
            // could we lift that constraint and accept that models just be IPublishedContent?
            // would then mean that we cannot assume a model is IPublishedContentExtended, so
            // either it is, or we need to wrap it. so instead of having
            //   (Model:IPublishedContentExtended (IPublishedContent))
            // we'd have
            //   (PublishedContentExtended (Model (IPublishedContent)))
            // and it is that bad? any other consequences?
            //
            // would also allow the factory to cache the model (though that should really
            // be done by the content cache, not by the factory).

            var extended2 = extended as IPublishedContentExtended;
            if (extended2 == null)
                throw new Exception("Extended does not implement IPublishedContentExtended.");
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
