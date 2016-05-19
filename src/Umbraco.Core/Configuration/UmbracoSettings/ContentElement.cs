using System;
using System.Collections.Generic;
using System.Configuration;
using Umbraco.Core.Macros;

namespace Umbraco.Core.Configuration.UmbracoSettings
{

    internal class ContentElement : ConfigurationElement, IContentSection
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
            get
            {
                return new OptionalInnerTextConfigurationElement<bool>(
                       (InnerTextConfigurationElement<bool>)this["ResolveUrlsFromTextString"],
                        //set the default
                       false);
            }
        }

        [ConfigurationProperty("UploadAllowDirectories")]
        internal InnerTextConfigurationElement<bool> UploadAllowDirectories
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<bool>(
                       (InnerTextConfigurationElement<bool>)this["UploadAllowDirectories"],
                    //set the default
                       true);
            }
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
            get
            {
                return new OptionalInnerTextConfigurationElement<bool>(
                      (InnerTextConfigurationElement<bool>)this["ensureUniqueNaming"],
                    //set the default
                      true);
            }
        }

        [ConfigurationProperty("XmlCacheEnabled")]
        internal InnerTextConfigurationElement<bool> XmlCacheEnabled
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<bool>(
                          (InnerTextConfigurationElement<bool>)this["XmlCacheEnabled"],
                    //set the default
                          true);
            }
        }

        [ConfigurationProperty("ContinouslyUpdateXmlDiskCache")]
        internal InnerTextConfigurationElement<bool> ContinouslyUpdateXmlDiskCache
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<bool>(
                          (InnerTextConfigurationElement<bool>)this["ContinouslyUpdateXmlDiskCache"],
                    //set the default
                          true);                
            }
        }

        [ConfigurationProperty("XmlContentCheckForDiskChanges")]
        internal InnerTextConfigurationElement<bool> XmlContentCheckForDiskChanges
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<bool>(
                          (InnerTextConfigurationElement<bool>)this["XmlContentCheckForDiskChanges"],
                    //set the default
                          false); 
            }
        }

        [ConfigurationProperty("EnableSplashWhileLoading")]
        internal InnerTextConfigurationElement<bool> EnableSplashWhileLoading
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<bool>(
                          (InnerTextConfigurationElement<bool>)this["EnableSplashWhileLoading"],
                    //set the default
                          false); 
            }
        }

        [ConfigurationProperty("PropertyContextHelpOption")]
        internal InnerTextConfigurationElement<string> PropertyContextHelpOption
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<string>(
                          (InnerTextConfigurationElement<string>)this["PropertyContextHelpOption"],
                    //set the default
                          "text"); 
            }
        }

        
        [ConfigurationProperty("ForceSafeAliases")]
        internal InnerTextConfigurationElement<bool> ForceSafeAliases
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<bool>(
                          (InnerTextConfigurationElement<bool>)this["ForceSafeAliases"],
                    //set the default
                          true); 
            }
        }

        [ConfigurationProperty("PreviewBadge")]
        internal InnerTextConfigurationElement<string> PreviewBadge
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<string>(
                          (InnerTextConfigurationElement<string>)this["PreviewBadge"],
                    //set the default
                          @"<a id=""umbracoPreviewBadge"" style=""position: absolute; top: 0; right: 0; border: 0; width: 149px; height: 149px; background: url('{1}/preview/previewModeBadge.png') no-repeat;"" href=""{0}/endPreview.aspx?redir={2}""><span style=""display:none;"">In Preview Mode - click to end</span></a>"); 
            }
        }

        [ConfigurationProperty("UmbracoLibraryCacheDuration")]
        internal InnerTextConfigurationElement<int> UmbracoLibraryCacheDuration
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<int>(
                          (InnerTextConfigurationElement<int>)this["UmbracoLibraryCacheDuration"],
                    //set the default
                          1800); 
                
            }
        }

        [ConfigurationProperty("MacroErrors")]
        internal InnerTextConfigurationElement<MacroErrorBehaviour> MacroErrors
        {
            get
            {

                return new OptionalInnerTextConfigurationElement<MacroErrorBehaviour>(
                          (InnerTextConfigurationElement<MacroErrorBehaviour>)this["MacroErrors"],
                    //set the default
                          MacroErrorBehaviour.Inline); 
            }
        }

        [ConfigurationProperty("disallowedUploadFiles")]
        internal CommaDelimitedConfigurationElement DisallowedUploadFiles
        {
            get
            {
                return new OptionalCommaDelimitedConfigurationElement(
                       (CommaDelimitedConfigurationElement)this["disallowedUploadFiles"],
                    //set the default
                       new[] { "ashx", "aspx", "ascx", "config", "cshtml", "vbhtml", "asmx", "air", "axd" });

            }
        }

        [ConfigurationProperty("cloneXmlContent")]
        internal InnerTextConfigurationElement<bool> CloneXmlContent
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<bool>(
                    (InnerTextConfigurationElement<bool>)this["cloneXmlContent"],
                    //set the default
                    true);
            }
        }

        [ConfigurationProperty("GlobalPreviewStorageEnabled")]
        internal InnerTextConfigurationElement<bool> GlobalPreviewStorageEnabled
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<bool>(
                    (InnerTextConfigurationElement<bool>)this["GlobalPreviewStorageEnabled"],
                    //set the default
                    false);
            }
        }

        [ConfigurationProperty("defaultDocumentTypeProperty")]
        internal InnerTextConfigurationElement<string> DefaultDocumentTypeProperty
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<string>(
                    (InnerTextConfigurationElement<string>)this["defaultDocumentTypeProperty"],
                    //set the default
                    "Textstring");
            }
        }

        [ConfigurationProperty("EnableInheritedDocumentTypes")]
        internal InnerTextConfigurationElement<bool> EnableInheritedDocumentTypes
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<bool>(
                    (InnerTextConfigurationElement<bool>) this["EnableInheritedDocumentTypes"],
                    //set the default
                    true);
            }
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

        bool IContentSection.EnableInheritedDocumentTypes
        {
            get { return EnableInheritedDocumentTypes; }
        }
    }
}