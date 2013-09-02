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
            get
            {
                return new OptionalInnerTextConfigurationElement<bool>(
                       (InnerTextConfigurationElement<bool>)this["UploadAllowDirectories"],
                    //set the default
                       true);
            }
        }

        [ConfigurationProperty("errors", IsRequired = true)]
        public ContentErrorsElement Errors
        {
            get { return (ContentErrorsElement) base["errors"]; }
        }

        [ConfigurationProperty("notifications", IsRequired = true)]
        public NotificationsElement Notifications
        {
            get { return (NotificationsElement)base["notifications"]; }
        }

        [ConfigurationProperty("ensureUniqueNaming")]
        public InnerTextConfigurationElement<bool> EnsureUniqueNaming
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<bool>(
                      (InnerTextConfigurationElement<bool>)this["ensureUniqueNaming"],
                    //set the default
                      true);
            }
        }

        [ConfigurationProperty("TidyEditorContent")]
        public InnerTextConfigurationElement<bool> TidyEditorContent
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<bool>(
                      (InnerTextConfigurationElement<bool>)this["TidyEditorContent"],
                    //set the default
                      false);
            }
        }

        [ConfigurationProperty("TidyCharEncoding")]
        public InnerTextConfigurationElement<string> TidyCharEncoding
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<string>(
                          (InnerTextConfigurationElement<string>)this["TidyCharEncoding"],
                            //set the default
                          "UTF8");
            }
        }

        [ConfigurationProperty("XmlCacheEnabled")]
        public InnerTextConfigurationElement<bool> XmlCacheEnabled
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
        public InnerTextConfigurationElement<bool> ContinouslyUpdateXmlDiskCache
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
        public InnerTextConfigurationElement<bool> XmlContentCheckForDiskChanges
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
        public InnerTextConfigurationElement<bool> EnableSplashWhileLoading
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
        public InnerTextConfigurationElement<string> PropertyContextHelpOption
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<string>(
                          (InnerTextConfigurationElement<string>)this["PropertyContextHelpOption"],
                    //set the default
                          "text"); 
            }
        }

        [ConfigurationProperty("UseLegacyXmlSchema")]
        public InnerTextConfigurationElement<bool> UseLegacyXmlSchema
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<bool>(
                          (InnerTextConfigurationElement<bool>)this["UseLegacyXmlSchema"],
                    //set the default
                          false); 
            }
        }
        
        [ConfigurationProperty("ForceSafeAliases")]
        public InnerTextConfigurationElement<bool> ForceSafeAliases
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
        public InnerTextConfigurationElement<string> PreviewBadge
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
        public InnerTextConfigurationElement<int> UmbracoLibraryCacheDuration
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
        public InnerTextConfigurationElement<MacroErrorBehaviour> MacroErrors
        {
            get
            {

                return new OptionalInnerTextConfigurationElement<MacroErrorBehaviour>(
                          (InnerTextConfigurationElement<MacroErrorBehaviour>)this["MacroErrors"],
                    //set the default
                          MacroErrorBehaviour.Inline); 
            }
        }

        [ConfigurationProperty("DocumentTypeIconList")]
        public InnerTextConfigurationElement<IconPickerBehaviour> DocumentTypeIconList
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<IconPickerBehaviour>(
                          (InnerTextConfigurationElement<IconPickerBehaviour>)this["DocumentTypeIconList"],
                    //set the default
                          IconPickerBehaviour.HideFileDuplicates);
            }
        }

        [ConfigurationProperty("disallowedUploadFiles")]
        public OptionalCommaDelimitedConfigurationElement DisallowedUploadFiles
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
    }
}