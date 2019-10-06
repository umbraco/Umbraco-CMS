using System.Collections.Generic;
using System.Configuration;
using Umbraco.Core.Macros;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ContentElement : UmbracoConfigurationElement, IContentSection
    {
        private const string DefaultPreviewBadge = @"<div id=""umbracoPreviewBadge"" class=""umbraco-preview-badge""><span class=""umbraco-preview-badge__header"">Preview mode</span><a href=""{0}/preview/end?redir={1}"" class=""umbraco-preview-badge__end""><svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 357 357""><title>Click to end</title><path d=""M357 35.7L321.3 0 178.5 142.8 35.7 0 0 35.7l142.8 142.8L0 321.3 35.7 357l142.8-142.8L321.3 357l35.7-35.7-142.8-142.8z""></path></svg></a></div><style type=""text/css"">.umbraco-preview-badge {{position: absolute;top: 1em;right: 1em;display: inline-flex;background: #1b264f;color: #fff;padding: 1em;font-size: 12px;z-index: 99999999;justify-content: center;align-items: center;box-shadow: 0 10px 50px rgba(0, 0, 0, .1), 0 6px 20px rgba(0, 0, 0, .16);line-height: 1;}}.umbraco-preview-badge__header {{font-weight: bold;}}.umbraco-preview-badge__end {{width: 3em;padding: 1em;margin: -1em -1em -1em 2em;display: flex;align-items: center;align-self: stretch;}}.umbraco-preview-badge__end:hover,.umbraco-preview-badge__end:focus {{background: #f5c1bc;}}.umbraco-preview-badge__end svg {{fill: #fff;}}</style>";

        [ConfigurationProperty("imaging")]
        internal ContentImagingElement Imaging => (ContentImagingElement) this["imaging"];

        [ConfigurationProperty("ResolveUrlsFromTextString")]
        internal InnerTextConfigurationElement<bool> ResolveUrlsFromTextString => GetOptionalTextElement("ResolveUrlsFromTextString", false);

        public IEnumerable<IContentErrorPage> Error404Collection => Errors.Error404Collection;

        [ConfigurationProperty("errors", IsRequired = true)]
        internal ContentErrorsElement Errors => (ContentErrorsElement) base["errors"];

        [ConfigurationProperty("notifications", IsRequired = true)]
        internal NotificationsElement Notifications => (NotificationsElement) base["notifications"];

        [ConfigurationProperty("PreviewBadge")]
        internal InnerTextConfigurationElement<string> PreviewBadge => GetOptionalTextElement("PreviewBadge", DefaultPreviewBadge);

        [ConfigurationProperty("MacroErrors")]
        internal InnerTextConfigurationElement<MacroErrorBehaviour> MacroErrors => GetOptionalTextElement("MacroErrors", MacroErrorBehaviour.Inline);

        [ConfigurationProperty("disallowedUploadFiles")]
        internal CommaDelimitedConfigurationElement DisallowedUploadFiles => GetOptionalDelimitedElement("disallowedUploadFiles", new[] {"ashx", "aspx", "ascx", "config", "cshtml", "vbhtml", "asmx", "air", "axd"});

        [ConfigurationProperty("allowedUploadFiles")]
        internal CommaDelimitedConfigurationElement AllowedUploadFiles => GetOptionalDelimitedElement("allowedUploadFiles", new string[0]);
        
        [ConfigurationProperty("showDeprecatedPropertyEditors")]
        internal InnerTextConfigurationElement<bool> ShowDeprecatedPropertyEditors => GetOptionalTextElement("showDeprecatedPropertyEditors", false);

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

        bool IContentSection.ShowDeprecatedPropertyEditors => ShowDeprecatedPropertyEditors;

        string IContentSection.LoginBackgroundImage => LoginBackgroundImage;
    }
}
