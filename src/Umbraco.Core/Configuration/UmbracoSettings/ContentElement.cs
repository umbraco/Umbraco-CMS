using System;
using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ContentElement : UmbracoConfigurationElement, IContentSection
    {
        [ConfigurationProperty("imaging")]
        internal ContentImagingElement Imaging
        {
            get { return (ContentImagingElement)this["imaging"]; }
        }

        [ConfigurationProperty("scripteditor")]
        internal ContentScriptEditorElement ScriptEditor
        {
            get { return (ContentScriptEditorElement)this["scripteditor"]; }
        }

        [ConfigurationProperty("ResolveUrlsFromTextString")]
        internal InnerTextConfigurationElement<bool> ResolveUrlsFromTextString
        {
            get { return GetOptionalTextElement<bool>("ResolveUrlsFromTextString", false); }
        }

        [ConfigurationProperty("UploadAllowDirectories")]
        internal InnerTextConfigurationElement<bool> UploadAllowDirectories
        {
            get { return GetOptionalTextElement("UploadAllowDirectories", true); }
        }

        public IEnumerable<IContentErrorPage> Error404Collection
        {
            get { return Errors.Error404Collection; }
        }

        [ConfigurationProperty("errors", IsRequired = true)]
        internal ContentErrorsElement Errors
        {
            get { return (ContentErrorsElement) base["errors"]; }
        }

        [ConfigurationProperty("notifications", IsRequired = true)]
        internal NotificationsElement Notifications
        {
            get { return (NotificationsElement)base["notifications"]; }
        }

        [ConfigurationProperty("ensureUniqueNaming")]
        internal InnerTextConfigurationElement<bool> EnsureUniqueNaming
        {
            get { return GetOptionalTextElement("ensureUniqueNaming", true); }
        }

        [ConfigurationProperty("TidyEditorContent")]
        internal InnerTextConfigurationElement<bool> TidyEditorContent
        {
            get { return GetOptionalTextElement("TidyEditorContent", false); }
        }

        [ConfigurationProperty("TidyCharEncoding")]
        internal InnerTextConfigurationElement<string> TidyCharEncoding
        {
            get { return GetOptionalTextElement("TidyCharEncoding", "UTF8"); }
        }

        [ConfigurationProperty("XmlCacheEnabled")]
        internal InnerTextConfigurationElement<bool> XmlCacheEnabled
        {
            get { return GetOptionalTextElement("XmlCacheEnabled", true); }
        }

        [ConfigurationProperty("ContinouslyUpdateXmlDiskCache")]
        internal InnerTextConfigurationElement<bool> ContinouslyUpdateXmlDiskCache
        {
            get { return GetOptionalTextElement("ContinouslyUpdateXmlDiskCache", true); }
        }

        [ConfigurationProperty("XmlContentCheckForDiskChanges")]
        internal InnerTextConfigurationElement<bool> XmlContentCheckForDiskChanges
        {
            get { return GetOptionalTextElement("XmlContentCheckForDiskChanges", false); }
        }

        [ConfigurationProperty("EnableSplashWhileLoading")]
        internal InnerTextConfigurationElement<bool> EnableSplashWhileLoading
        {
            get { return GetOptionalTextElement("EnableSplashWhileLoading", false); }
        }

        [ConfigurationProperty("PropertyContextHelpOption")]
        internal InnerTextConfigurationElement<string> PropertyContextHelpOption
        {
            get { return GetOptionalTextElement("PropertyContextHelpOption", "text"); }
        }

        [ConfigurationProperty("UseLegacyXmlSchema")]
        internal InnerTextConfigurationElement<bool> UseLegacyXmlSchema
        {
            get { return GetOptionalTextElement("UseLegacyXmlSchema", false); }
        }
        
        [ConfigurationProperty("ForceSafeAliases")]
        internal InnerTextConfigurationElement<bool> ForceSafeAliases
        {
            get { return GetOptionalTextElement("ForceSafeAliases", true); }
        }

        [ConfigurationProperty("PreviewBadge")]
        internal InnerTextConfigurationElement<string> PreviewBadge
        {
            get
            {
                return GetOptionalTextElement("PreviewBadge", @"<a id=""umbracoPreviewBadge"" style=""z-index:99999; position: absolute; top: 0; right: 0; border: 0; width: 149px; height: 149px; background: url('{1}/preview/previewModeBadge.png') no-repeat;"" href=""{0}/endPreview.aspx?redir={2}""><span style=""display:none;"">In Preview Mode - click to end</span></a>");                
            }
        }

        [ConfigurationProperty("UmbracoLibraryCacheDuration")]
        internal InnerTextConfigurationElement<int> UmbracoLibraryCacheDuration
        {
            get { return GetOptionalTextElement("UmbracoLibraryCacheDuration", 1800); }
        }

        [ConfigurationProperty("MacroErrors")]
        internal InnerTextConfigurationElement<MacroErrorBehaviour> MacroErrors
        {
            get { return GetOptionalTextElement("MacroErrors", MacroErrorBehaviour.Inline); }
        }

        [Obsolete("This is here so that if this config element exists we won't get a YSOD, it is not used whatsoever and will be removed in future versions")]
        [ConfigurationProperty("DocumentTypeIconList")]
        internal InnerTextConfigurationElement<IconPickerBehaviour> DocumentTypeIconList
        {
            get { return GetOptionalTextElement("DocumentTypeIconList", IconPickerBehaviour.HideFileDuplicates); }
        }

        [ConfigurationProperty("disallowedUploadFiles")]
        internal CommaDelimitedConfigurationElement DisallowedUploadFiles
        {
            get { return GetOptionalDelimitedElement("disallowedUploadFiles", new[] {"ashx", "aspx", "ascx", "config", "cshtml", "vbhtml", "asmx", "air", "axd"}); }
        }

        [ConfigurationProperty("allowedUploadFiles")]
        internal CommaDelimitedConfigurationElement AllowedUploadFiles
        {
            get { return GetOptionalDelimitedElement("allowedUploadFiles", new string[0]); }
        }

        [ConfigurationProperty("cloneXmlContent")]
        internal InnerTextConfigurationElement<bool> CloneXmlContent
        {
            get { return GetOptionalTextElement("cloneXmlContent", true); }
        }

        [ConfigurationProperty("GlobalPreviewStorageEnabled")]
        internal InnerTextConfigurationElement<bool> GlobalPreviewStorageEnabled
        {
            get { return GetOptionalTextElement("GlobalPreviewStorageEnabled", false); }
        }

        [ConfigurationProperty("defaultDocumentTypeProperty")]
        internal InnerTextConfigurationElement<string> DefaultDocumentTypeProperty
        {
            get { return GetOptionalTextElement("defaultDocumentTypeProperty", "Textstring"); }
        }

        [ConfigurationProperty("showDeprecatedPropertyEditors")]
        internal InnerTextConfigurationElement<bool> ShowDeprecatedPropertyEditors
        {
            get { return GetOptionalTextElement("showDeprecatedPropertyEditors", false); }
        }

        [ConfigurationProperty("EnableInheritedDocumentTypes")]
        internal InnerTextConfigurationElement<bool> EnableInheritedDocumentTypes
        {
            get { return GetOptionalTextElement("EnableInheritedDocumentTypes", true); }
        }

        [ConfigurationProperty("EnableInheritedMediaTypes")]
        internal InnerTextConfigurationElement<bool> EnableInheritedMediaTypes
        {
            get { return GetOptionalTextElement("EnableInheritedMediaTypes", true); }
        }

        [ConfigurationProperty("EnablePropertyValueConverters")]
        internal InnerTextConfigurationElement<bool> EnablePropertyValueConverters
        {
            get { return GetOptionalTextElement("EnablePropertyValueConverters", false); }
        }

        [ConfigurationProperty("loginBackgroundImage")]
        internal InnerTextConfigurationElement<string> LoginBackgroundImage
        {
            get { return GetOptionalTextElement("loginBackgroundImage", string.Empty); }
        }

        string IContentSection.NotificationEmailAddress
        {
            get { return Notifications.NotificationEmailAddress; }
        }

        bool IContentSection.DisableHtmlEmail
        {
            get { return Notifications.DisableHtmlEmail; }
        }

        IEnumerable<string> IContentSection.ImageFileTypes
        {
            get { return Imaging.ImageFileTypes; }
        }

        IEnumerable<string> IContentSection.ImageTagAllowedAttributes
        {
            get { return Imaging.ImageTagAllowedAttributes; }
        }

        IEnumerable<IImagingAutoFillUploadField> IContentSection.ImageAutoFillProperties
        {
            get { return Imaging.ImageAutoFillProperties; }
        }

        bool IContentSection.ScriptEditorDisable
        {
            get { return ScriptEditor.ScriptEditorDisable; }
        }

        string IContentSection.ScriptFolderPath
        {
            get { return ScriptEditor.ScriptFolderPath; }
        }

        IEnumerable<string> IContentSection.ScriptFileTypes
        {
            get { return ScriptEditor.ScriptFileTypes; }
        }

        bool IContentSection.ResolveUrlsFromTextString
        {
            get { return ResolveUrlsFromTextString; }
        }

        bool IContentSection.UploadAllowDirectories
        {
            get { return UploadAllowDirectories; }
        }        

        bool IContentSection.EnsureUniqueNaming
        {
            get { return EnsureUniqueNaming; }
        }

        bool IContentSection.TidyEditorContent
        {
            get { return TidyEditorContent; }
        }

        string IContentSection.TidyCharEncoding
        {
            get { return TidyCharEncoding; }
        }

        bool IContentSection.XmlCacheEnabled
        {
            get { return XmlCacheEnabled; }
        }

        bool IContentSection.ContinouslyUpdateXmlDiskCache
        {
            get { return ContinouslyUpdateXmlDiskCache; }
        }

        bool IContentSection.XmlContentCheckForDiskChanges
        {
            get { return XmlContentCheckForDiskChanges; }
        }

        bool IContentSection.EnableSplashWhileLoading
        {
            get { return EnableSplashWhileLoading; }
        }

        string IContentSection.PropertyContextHelpOption
        {
            get { return PropertyContextHelpOption; }
        }

        bool IContentSection.UseLegacyXmlSchema
        {
            get { return UseLegacyXmlSchema; }
        }

        bool IContentSection.ForceSafeAliases
        {
            get { return ForceSafeAliases; }
        }

        string IContentSection.PreviewBadge
        {
            get { return PreviewBadge; }
        }

        int IContentSection.UmbracoLibraryCacheDuration
        {
            get { return UmbracoLibraryCacheDuration; }
        }

        MacroErrorBehaviour IContentSection.MacroErrorBehaviour
        {
            get { return MacroErrors; }
        }

        IEnumerable<string> IContentSection.DisallowedUploadFiles
        {
            get { return DisallowedUploadFiles; }
        }

        IEnumerable<string> IContentSection.AllowedUploadFiles
        {
            get { return AllowedUploadFiles; }
        }

        bool IContentSection.CloneXmlContent
        {
            get { return CloneXmlContent; }
        }

        bool IContentSection.GlobalPreviewStorageEnabled
        {
            get { return GlobalPreviewStorageEnabled; }
        }

        string IContentSection.DefaultDocumentTypeProperty
        {
            get { return DefaultDocumentTypeProperty; }
        }

        bool IContentSection.ShowDeprecatedPropertyEditors
        {
            get { return ShowDeprecatedPropertyEditors; }
        }

        bool IContentSection.EnableInheritedDocumentTypes
        {
            get { return EnableInheritedDocumentTypes; }
        }

        bool IContentSection.EnableInheritedMediaTypes
        {
            get { return EnableInheritedMediaTypes; }
        }
        bool IContentSection.EnablePropertyValueConverters
        {
            get { return EnablePropertyValueConverters; }
        }

        string IContentSection.LoginBackgroundImage
        {
            get { return LoginBackgroundImage; }
        }
        
    }
}
