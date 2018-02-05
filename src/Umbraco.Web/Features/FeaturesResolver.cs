using Umbraco.Core.ObjectResolution;

namespace Umbraco.Web.Features
{
    internal class FeaturesResolver : SingleObjectResolverBase<FeaturesResolver, UmbracoFeatures>
    {
        public FeaturesResolver(UmbracoFeatures value)
            : base(value)
        { }

        /// <summary>
        /// Sets the features.
        /// </summary>
        /// <remarks>For developers, at application startup.</remarks>
        public void SetFeatures(UmbracoFeatures features)
        {
            Value = features;
        }

        /// <summary>
        /// Gets the features.
        /// </summary>
        public UmbracoFeatures Features
        {
            get { return Value; }
        }
    }
}