using System.Collections.Generic;
using System.Configuration;
using Umbraco.Core.Macros;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ContentElement : UmbracoConfigurationElement, IContentSection
    {
        private const string DefaultPreviewBadge = @"<a id=""umbracoPreviewBadge"" style=""z-index:99999; position: absolute; top: 0; right: 0; border: 0; width: 149px; height: 149px; background: url('{0}/assets/img/preview-mode-badge.png') no-repeat;"" href=""#"" OnClick=""javascript:window.top.location.href = '{0}/preview/end?redir={1}'""><span style=""display:none;"">In Preview Mode - click to end</span></a>";

        [ConfigurationProperty("imaging")]
        internal ContentImagingElement Imaging => (ContentImagingElement) this["imaging"];

        [ConfigurationProperty("ResolveUrlsFromTextString")]
        internal InnerTextConfigurationElement<bool> ResolveUrlsFromTextString => GetOptionalTextElement("ResolveUrlsFromTextString", false);

        public IEnumerable<IContentErrorPage> Error404Collection => Errors.Error404Collection;

        [ConfigurationProperty("errors", IsRequired = true)]
        internal ContentErrorsElement Errors => (ContentErrorsElement) base["errors"];

        [ConfigurationProperty("notifications", IsRequired = true)]
        internal NotificationsElement Notifications => (NotificationsElement) base["notifications"];

        [ConfigurationProperty("PropertyContextHelpOption")]
        internal InnerTextConfigurationElement<string> PropertyContextHelpOption => GetOptionalTextElement("PropertyContextHelpOption", "text");

        [ConfigurationProperty("PreviewBadge")]
        internal InnerTextConfigurationElement<string> PreviewBadge => GetOptionalTextElement("PreviewBadge", DefaultPreviewBadge);

        [ConfigurationProperty("MacroErrors")]
        internal InnerTextConfigurationElement<MacroErrorBehaviour> MacroErrors => GetOptionalTextElement("MacroErrors", MacroErrorBehaviour.Inline);

        [ConfigurationProperty("disallowedUploadFiles")]
        internal CommaDelimitedConfigurationElement DisallowedUploadFiles => GetOptionalDelimitedElement("disallowedUploadFiles", new[] {"ashx", "aspx", "ascx", "config", "cshtml", "vbhtml", "asmx", "air", "axd"});

        [ConfigurationProperty("allowedUploadFiles")]
        internal CommaDelimitedConfigurationElement AllowedUploadFiles => GetOptionalDelimitedElement("allowedUploadFiles", new string[0]);
        
        [ConfigurationProperty("GlobalPreviewStorageEnabled")]
        internal InnerTextConfigurationElement<bool> GlobalPreviewStorageEnabled => GetOptionalTextElement("GlobalPreviewStorageEnabled", false);

        [ConfigurationProperty("defaultDocumentTypeProperty")]
        internal InnerTextConfigurationElement<string> DefaultDocumentTypeProperty => GetOptionalTextElement("defaultDocumentTypeProperty", "Textstring");

        [ConfigurationProperty("showDeprecatedPropertyEditors")]
        internal InnerTextConfigurationElement<bool> ShowDeprecatedPropertyEditors => GetOptionalTextElement("showDeprecatedPropertyEditors", false);

        [ConfigurationProperty("EnableInheritedDocumentTypes")]
        internal InnerTextConfigurationElement<bool> EnableInheritedDocumentTypes => GetOptionalTextElement("EnableInheritedDocumentTypes", true);

        [ConfigurationProperty("EnableInheritedMediaTypes")]
        internal InnerTextConfigurationElement<bool> EnableInheritedMediaTypes => GetOptionalTextElement("EnableInheritedMediaTypes", true);

        [ConfigurationProperty("loginBackgroundImage")]
        internal InnerTextConfigurationElement<string> LoginBackgroundImage => GetOptionalTextElement("loginBackgroundImage", string.Empty);

        string IContentSection.NotificationEmailAddress => Notifications.NotificationEmailAddress;

        bool IContentSection.DisableHtmlEmail => Notifications.DisableHtmlEmail;

        IEnumerable<string> IContentSection.ImageFileTypes => Imaging.ImageFileTypes;

        IEnumerable<IImagingAutoFillUploadField> IContentSection.ImageAutoFillProperties => Imaging.ImageAutoFillProperties;

        bool IContentSection.ResolveUrlsFromTextString => ResolveUrlsFromTextString;

        string IContentSection.PreviewBadge => PreviewBadge;

        MacroErrorBehaviour IContentSection.MacroErrorBehaviour => MacroErrors;

        IEnumerable<string> IContentSection.DisallowedUploadFiles => DisallowedUploadFiles;

        IEnumerable<string> IContentSection.AllowedUploadFiles => AllowedUploadFiles;

        bool IContentSection.GlobalPreviewStorageEnabled => GlobalPreviewStorageEnabled;

        string IContentSection.DefaultDocumentTypeProperty => DefaultDocumentTypeProperty;

        bool IContentSection.ShowDeprecatedPropertyEditors => ShowDeprecatedPropertyEditors;

        bool IContentSection.EnableInheritedDocumentTypes => EnableInheritedDocumentTypes;

        bool IContentSection.EnableInheritedMediaTypes => EnableInheritedMediaTypes;

        
        string IContentSection.LoginBackgroundImage => LoginBackgroundImage;
    }
}
