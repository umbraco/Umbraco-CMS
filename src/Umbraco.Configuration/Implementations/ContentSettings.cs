using System.Collections.Generic;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Macros;

namespace Umbraco.Configuration.Implementations
{
    internal class ContentSettings : ConfigurationManagerConfigBase, IContentSettings
    {
        public string NotificationEmailAddress => UmbracoSettingsSection.Content.Notifications.NotificationEmailAddress;
        public bool DisableHtmlEmail => UmbracoSettingsSection.Content.Notifications.DisableHtmlEmail;
        public IEnumerable<string> ImageFileTypes => UmbracoSettingsSection.Content.Imaging.ImageFileTypes;
        public IEnumerable<IImagingAutoFillUploadField> ImageAutoFillProperties => UmbracoSettingsSection.Content.Imaging.ImageAutoFillProperties;
        public bool ResolveUrlsFromTextString => UmbracoSettingsSection.Content.ResolveUrlsFromTextString;
        public IEnumerable<IContentErrorPage> Error404Collection => UmbracoSettingsSection.Content.Error404Collection;
        public string PreviewBadge => UmbracoSettingsSection.Content.PreviewBadge;
        public MacroErrorBehaviour MacroErrorBehaviour => UmbracoSettingsSection.Content.MacroErrors;
        public IEnumerable<string> DisallowedUploadFiles => UmbracoSettingsSection.Content.DisallowedUploadFiles;
        public IEnumerable<string> AllowedUploadFiles => UmbracoSettingsSection.Content.AllowedUploadFiles;
        public bool ShowDeprecatedPropertyEditors => UmbracoSettingsSection.Content.ShowDeprecatedPropertyEditors;
        public string LoginBackgroundImage => UmbracoSettingsSection.Content.LoginBackgroundImage;
    }
}
