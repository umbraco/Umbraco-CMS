using System;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Features
{
    /// <summary>
    /// Represents the Umbraco features.
    /// </summary>
    public class UmbracoFeatures
    {
        /// <summary>
        ///  Initializes a new instance of the <see cref="UmbracoFeatures"/> class.
        /// </summary>
        public UmbracoFeatures()
        {
            Disabled = new DisabledFeatures();
            Enabled = new EnabledFeatures();
        }
        
        /// <summary>
        /// Gets the disabled features.
        /// </summary>
        internal DisabledFeatures Disabled { get; }

        /// <summary>
        /// Gets the enabled features.
        /// </summary>
        internal EnabledFeatures Enabled { get; }

        /// <summary>
        /// Determines whether a controller is enabled.
        /// </summary>
        internal bool IsControllerEnabled(Type feature)
        {
            if (typeof(UmbracoApiControllerBase).IsAssignableFrom(feature))
                return Disabled.Controllers.Contains(feature) == false;

            throw new NotSupportedException("Not a supported feature type.");
        }
    }
}
