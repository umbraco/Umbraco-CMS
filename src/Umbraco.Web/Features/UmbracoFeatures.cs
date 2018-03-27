using System;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Features
{
    /// <summary>
    /// Represents the Umbraco features.
    /// </summary>
    internal class UmbracoFeatures
    {
        /// <summary>
        ///  Initializes a new instance of the <see cref="UmbracoFeatures"/> class.
        /// </summary>
        public UmbracoFeatures()
        {
            Disabled = new DisabledFeatures();
            Enabled = new EnabledFeatures();
        }

        // note
        // currently, the only thing a FeatureSet does is list disabled controllers,
        // but eventually we could enable and disable more parts of Umbraco. and then
        // we would need some logic to figure out what's enabled/disabled - hence it's
        // better to use IsEnabled, where the logic would go, rather than directly
        // accessing the Disabled collection.

        /// <summary>
        /// Gets the disabled features.
        /// </summary>
        public DisabledFeatures Disabled { get; }

        /// <summary>
        /// Gets the enabled features.
        /// </summary>
        public EnabledFeatures Enabled { get; }

        /// <summary>
        /// Determines whether a feature is enabled.
        /// </summary>
        public bool IsEnabled(Type feature)
        {
            if (typeof(UmbracoApiControllerBase).IsAssignableFrom(feature))
                return Disabled.Controllers.Contains(feature) == false;

            throw new NotSupportedException("Not a supported feature type.");
        }
    }
}
