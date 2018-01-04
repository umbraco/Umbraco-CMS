using Umbraco.Core.ObjectResolution;

namespace Umbraco.Web.Features
{
    public class FeaturesResolver : SingleObjectResolverBase<FeaturesResolver, UmbracoFeatures>
    {
        public FeaturesResolver(UmbracoFeatures value) : base(value)
        {
        }
        /// <summary>
        /// Sets the disabled features
        /// </summary>
        /// <param name="finder"></param>
        /// <remarks>For developers, at application startup.</remarks>
        public void SetFeatures(UmbracoFeatures finder)
        {
            Value = finder;
        }

        /// <summary>
        /// Gets the features
        /// </summary>
        public UmbracoFeatures Features
        {
            get { return Value; }
        }
    }
}