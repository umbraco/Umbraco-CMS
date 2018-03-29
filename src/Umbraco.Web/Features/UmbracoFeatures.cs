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
        
        /// <summary>
        /// Gets the disabled features.
        /// </summary>
        public DisabledFeatures Disabled { get; set; }

        /// <summary>
        /// Gets the enabled features.
        /// </summary>
        public EnabledFeatures Enabled { get; set; }

        /// <summary>
        /// Determines whether a controller is enabled.
        /// </summary>
        public bool IsControllerEnabled(Type feature)
        {
            if (typeof(UmbracoApiControllerBase).IsAssignableFrom(feature))
                return Disabled.Controllers.Contains(feature) == false;

            throw new NotSupportedException("Not a supported feature type.");
        }
    }
}
