using System;
using System.Collections.Generic;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;
using Umbraco.Web.Features;

namespace Umbraco.Web.Editors
{
    public class BackOfficePreviewModel : BackOfficeModel
    {
        private readonly UmbracoFeatures _features;
        public IEnumerable<ILanguage> Languages { get; }

        [Obsolete("Use the overload that injects IIconService.")]
        public BackOfficePreviewModel(
            UmbracoFeatures features,
            IGlobalSettings globalSettings,
            IEnumerable<ILanguage> languages)
            : this(features, globalSettings, languages, Current.IconService)
        {
        }

        public BackOfficePreviewModel(
            UmbracoFeatures features,
            IGlobalSettings globalSettings,
            IEnumerable<ILanguage> languages,
            IIconService iconService)
            : base(features, globalSettings, iconService)
        {
            _features = features;
            Languages = languages;
        }

        public bool DisableDevicePreview => _features.Disabled.DisableDevicePreview;
        public string PreviewExtendedHeaderView => _features.Enabled.PreviewExtendedView;
    }
}
