using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ContentElement : ConfigurationElement
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

        [ConfigurationProperty("EnableCanvasEditing")]
        internal InnerTextConfigurationElement<bool> EnableCanvasEditing
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<bool>(
                       (InnerTextConfigurationElement<bool>)this["EnableCanvasEditing"],
                        //set the default
                       false);
            }
        }

        [ConfigurationProperty("ResolveUrlsFromTextString")]
        internal InnerTextConfigurationElement<bool> ResolveUrlsFromTextString
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<bool>(
                       (InnerTextConfigurationElement<bool>)this["ResolveUrlsFromTextString"],
                        //set the default
                       true);
            }
        }

        [ConfigurationProperty("UploadAllowDirectories")]
        internal InnerTextConfigurationElement<bool> UploadAllowDirectories
        {
            get { return (InnerTextConfigurationElement<bool>)this["UploadAllowDirectories"]; }
        }

        [ConfigurationProperty("errors")]
        public ContentErrorsElement Errors
        {
            get { return (ContentErrorsElement)base["errors"]; }
        }

        [ConfigurationProperty("notifications")]
        public NotificationsElement Notifications
        {
            get { return (NotificationsElement)base["notifications"]; }
        }

        [ConfigurationProperty("ensureUniqueNaming")]
        public InnerTextConfigurationElement<bool> EnsureUniqueNaming
        {
            get { return (InnerTextConfigurationElement<bool>)base["ensureUniqueNaming"]; }
        }

        [ConfigurationProperty("TidyEditorContent")]
        public InnerTextConfigurationElement<bool> TidyEditorContent
        {
            get { return (InnerTextConfigurationElement<bool>)base["TidyEditorContent"]; }
        }

        [ConfigurationProperty("TidyCharEncoding")]
        public InnerTextConfigurationElement<string> TidyCharEncoding
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<string>(
                          (InnerTextConfigurationElement<string>)this["TidyCharEncoding"],
                            //set the default
                          "Raw");
            }
        }

        [ConfigurationProperty("XmlCacheEnabled")]
        public InnerTextConfigurationElement<bool> XmlCacheEnabled
        {
            get { return (InnerTextConfigurationElement<bool>)base["XmlCacheEnabled"]; }
        }

        [ConfigurationProperty("ContinouslyUpdateXmlDiskCache")]
        public InnerTextConfigurationElement<bool> ContinouslyUpdateXmlDiskCache
        {
            get { return (InnerTextConfigurationElement<bool>)base["ContinouslyUpdateXmlDiskCache"]; }
        }

        [ConfigurationProperty("XmlContentCheckForDiskChanges")]
        public InnerTextConfigurationElement<bool> XmlContentCheckForDiskChanges
        {
            get { return (InnerTextConfigurationElement<bool>)base["XmlContentCheckForDiskChanges"]; }
        }

        [ConfigurationProperty("EnableSplashWhileLoading")]
        public InnerTextConfigurationElement<bool> EnableSplashWhileLoading
        {
            get { return (InnerTextConfigurationElement<bool>)base["EnableSplashWhileLoading"]; }
        }

        [ConfigurationProperty("PropertyContextHelpOption")]
        public InnerTextConfigurationElement<string> PropertyContextHelpOption
        {
            get { return (InnerTextConfigurationElement<string>)base["PropertyContextHelpOption"]; }
        }

        [ConfigurationProperty("UseLegacyXmlSchema")]
        public InnerTextConfigurationElement<bool> UseLegacyXmlSchema
        {
            get { return (InnerTextConfigurationElement<bool>)base["UseLegacyXmlSchema"]; }
        }
        
        [ConfigurationProperty("ForceSafeAliases")]
        public InnerTextConfigurationElement<bool> ForceSafeAliases
        {
            get { return (InnerTextConfigurationElement<bool>)base["ForceSafeAliases"]; }
        }

        [ConfigurationProperty("PreviewBadge")]
        public InnerTextConfigurationElement<string> PreviewBadge
        {
            get { return (InnerTextConfigurationElement<string>)base["PreviewBadge"]; }
        }

        [ConfigurationProperty("UmbracoLibraryCacheDuration")]
        public InnerTextConfigurationElement<int> UmbracoLibraryCacheDuration
        {
            get { return (InnerTextConfigurationElement<int>)base["UmbracoLibraryCacheDuration"]; }
        }

        [ConfigurationProperty("MacroErrors")]
        public InnerTextConfigurationElement<string> MacroErrors
        {
            get { return (InnerTextConfigurationElement<string>)base["MacroErrors"]; }
        }

        [ConfigurationProperty("DocumentTypeIconList")]
        public InnerTextConfigurationElement<IconPickerBehaviour> DocumentTypeIconList
        {
            get { return (InnerTextConfigurationElement<IconPickerBehaviour>)base["DocumentTypeIconList"]; }
        }

        [ConfigurationProperty("disallowedUploadFiles")]
        public CommaDelimitedConfigurationElement DisallowedUploadFiles
        {
            get { return (CommaDelimitedConfigurationElement)base["disallowedUploadFiles"]; }
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
    }
}