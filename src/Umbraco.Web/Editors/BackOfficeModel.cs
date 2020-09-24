using System;
using Umbraco.Core.Configuration;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;
using Umbraco.Web.Features;

namespace Umbraco.Web.Editors
{

    public class BackOfficeModel
    {


        [Obsolete("Use the overload that injects IIconService.")]
        public BackOfficeModel(UmbracoFeatures features, IGlobalSettings globalSettings) : this(features, globalSettings, Current.IconService)
        {

        }
        public BackOfficeModel(UmbracoFeatures features, IGlobalSettings globalSettings, IIconService iconService)
        {
            Features = features;
            GlobalSettings = globalSettings;
            IconCheckData = iconService.GetIcon("icon-check")?.SvgString;
            IconDeleteData = iconService.GetIcon("icon-delete")?.SvgString;
        }

        public UmbracoFeatures Features { get; }
        public IGlobalSettings GlobalSettings { get; }
        public string IconCheckData { get; }
        public string IconDeleteData { get; }
    }
}
