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

        [ConfigurationProperty("scripteditor")]
        internal ContentScriptEditorElement ScriptEditor => (ContentScriptEditorElement) this["scripteditor"];

        [ConfigurationProperty("ResolveUrlsFromTextString")]
        internal InnerTextConfigurationElement<bool> ResolveUrlsFromTextString => GetOptionalTextElement("ResolveUrlsFromTextString", false);

        [ConfigurationProperty("UploadAllowDirectories")]
        internal InnerTextConfigurationElement<bool> UploadAllowDirectories => GetOptionalTextElement("UploadAllowDirectories", true);

        public IEnumerable<IContentErrorPage> Error404Collection => Errors.Error404Collection;

        [ConfigurationProperty("errors", IsRequired = true)]
        internal ContentErrorsElement Errors => (ContentErrorsElement) base["errors"];

        [ConfigurationProperty("notifications", IsRequired = true)]
        internal NotificationsElement Notifications => (NotificationsElement) base["notifications"];

        [ConfigurationProperty("ensureUniqueNaming")]
        internal InnerTextConfigurationElement<bool> EnsureUniqueNaming => GetOptionalTextElement("ensureUniqueNaming", true);

        [ConfigurationProperty("XmlCacheEnabled")]
        internal InnerTextConfigurationElement<bool> XmlCacheEnabled => GetOptionalTextElement("XmlCacheEnabled", true);

        [ConfigurationProperty("ContinouslyUpdateXmlDiskCache")]
        internal InnerTextConfigurationElement<bool> ContinouslyUpdateXmlDiskCache => GetOptionalTextElement("ContinouslyUpdateXmlDiskCache", true);

        [ConfigurationProperty("XmlContentCheckForDiskChanges")]
        internal InnerTextConfigurationElement<bool> XmlContentCheckForDiskChanges => GetOptionalTextElement("XmlContentCheckForDiskChanges", false);

        [ConfigurationProperty("PropertyContextHelpOption")]
        internal InnerTextConfigurationElement<string> PropertyContextHelpOption => GetOptionalTextElement("PropertyContextHelpOption", "text");

        [ConfigurationProperty("ForceSafeAliases")]
        internal InnerTextConfigurationElement<bool> ForceSafeAliases => GetOptionalTextElement("ForceSafeAliases", true);

        [ConfigurationProperty("PreviewBadge")]
        internal InnerTextConfigurationElement<string> PreviewBadge => GetOptionalTextElement("PreviewBadge", DefaultPreviewBadge);

        [ConfigurationProperty("MacroErrors")]
        internal InnerTextConfigurationElement<MacroErrorBehaviour> MacroErrors => GetOptionalTextElement("MacroErrors", MacroErrorBehaviour.Inline);

        [ConfigurationProperty("disallowedUploadFiles")]
        internal CommaDelimitedConfigurationElement DisallowedUploadFiles => GetOptionalDelimitedElement("disallowedUploadFiles", new[] {"ashx", "aspx", "ascx", "config", "cshtml", "vbhtml", "asmx", "air", "axd"});

        [ConfigurationProperty("allowedUploadFiles")]
        internal CommaDelimitedConfigurationElement AllowedUploadFiles => GetOptionalDelimitedElement("allowedUploadFiles", new string[0]);

        [ConfigurationProperty("cloneXmlContent")]
        internal InnerTextConfigurationElement<bool> CloneXmlContent => GetOptionalTextElement("cloneXmlContent", true);

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

        IEnumerable<string> IContentSection.ImageTagAllowedAttributes => Imaging.ImageTagAllowedAttributes;

        IEnumerable<IImagingAutoFillUploadField> IContentSection.ImageAutoFillProperties => Imaging.ImageAutoFillProperties;

        bool IContentSection.ScriptEditorDisable => ScriptEditor.ScriptEditorDisable;

        string IContentSection.ScriptFolderPath => ScriptEditor.ScriptFolderPath;

        IEnumerable<string> IContentSection.ScriptFileTypes => ScriptEditor.ScriptFileTypes;

        bool IContentSection.ResolveUrlsFromTextString => ResolveUrlsFromTextString;

        bool IContentSection.UploadAllowDirectories => UploadAllowDirectories;

        bool IContentSection.EnsureUniqueNaming => EnsureUniqueNaming;

        bool IContentSection.XmlCacheEnabled => XmlCacheEnabled;

        bool IContentSection.ContinouslyUpdateXmlDiskCache => ContinouslyUpdateXmlDiskCache;

        bool IContentSection.XmlContentCheckForDiskChanges => XmlContentCheckForDiskChanges;

        string IContentSection.PropertyContextHelpOption => PropertyContextHelpOption;

        bool IContentSection.ForceSafeAliases => ForceSafeAliases;

        string IContentSection.PreviewBadge => PreviewBadge;

        MacroErrorBehaviour IContentSection.MacroErrorBehaviour => MacroErrors;

        IEnumerable<string> IContentSection.DisallowedUploadFiles => DisallowedUploadFiles;

        IEnumerable<string> IContentSection.AllowedUploadFiles => AllowedUploadFiles;

        bool IContentSection.CloneXmlContent => CloneXmlContent;

        bool IContentSection.GlobalPreviewStorageEnabled => GlobalPreviewStorageEnabled;

        string IContentSection.DefaultDocumentTypeProperty => DefaultDocumentTypeProperty;

        bool IContentSection.ShowDeprecatedPropertyEditors => ShowDeprecatedPropertyEditors;

        bool IContentSection.EnableInheritedDocumentTypes => EnableInheritedDocumentTypes;

        bool IContentSection.EnableInheritedMediaTypes => EnableInheritedMediaTypes;

        
        string IContentSection.LoginBackgroundImage => LoginBackgroundImage;
    }
}
