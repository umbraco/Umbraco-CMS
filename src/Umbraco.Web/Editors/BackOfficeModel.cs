using System;
using Umbraco.Core.Configuration;
using Umbraco.Web.Features;

namespace Umbraco.Web.Editors
{
    public class BackOfficeModel
    {

        public BackOfficeModel(UmbracoFeatures features, IGlobalSettings globalSettings)
        {
            Features = features;
            GlobalSettings = globalSettings;
        }

        public UmbracoFeatures Features { get; }
        public IGlobalSettings GlobalSettings { get; }
    }
}
