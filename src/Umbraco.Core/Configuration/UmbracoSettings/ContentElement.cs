using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ContentElement : ConfigurationElement, IContent
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

        [ConfigurationProperty("TidyEditorContent")]
        internal InnerTextConfigurationElement<bool> TidyEditorContent
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
        internal InnerTextConfigurationElement<string> TidyCharEncoding
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

        [ConfigurationProperty("UseLegacyXmlSchema")]
        internal InnerTextConfigurationElement<bool> UseLegacyXmlSchema
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

        [ConfigurationProperty("DocumentTypeIconList")]
        internal InnerTextConfigurationElement<IconPickerBehaviour> DocumentTypeIconList
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

        IContentImaging IContent.Imaging
        {
            get { return Imaging; }
        }

        IContentScriptEditor IContent.ScriptEditor
        {
            get { return ScriptEditor; }
        }

        bool IContent.EnableCanvasEditing
        {
            get { return EnableCanvasEditing; }
        }

        bool IContent.ResolveUrlsFromTextString
        {
            get { return ResolveUrlsFromTextString; }
        }

        bool IContent.UploadAllowDirectories
        {
            get { return UploadAllowDirectories; }
        }

        IContentErrors IContent.Errors
        {
            get { return Errors; }
        }

        INotifications IContent.Notifications
        {
            get { return Notifications; }
        }

        bool IContent.EnsureUniqueNaming
        {
            get { return EnsureUniqueNaming; }
        }

        bool IContent.TidyEditorContent
        {
            get { return TidyEditorContent; }
        }

        string IContent.TidyCharEncoding
        {
            get { return TidyCharEncoding; }
        }

        bool IContent.XmlCacheEnabled
        {
            get { return XmlCacheEnabled; }
        }

        bool IContent.ContinouslyUpdateXmlDiskCache
        {
            get { return ContinouslyUpdateXmlDiskCache; }
        }

        bool IContent.XmlContentCheckForDiskChanges
        {
            get { return XmlContentCheckForDiskChanges; }
        }

        bool IContent.EnableSplashWhileLoading
        {
            get { return EnableSplashWhileLoading; }
        }

        string IContent.PropertyContextHelpOption
        {
            get { return PropertyContextHelpOption; }
        }

        bool IContent.UseLegacyXmlSchema
        {
            get { return UseLegacyXmlSchema; }
        }

        bool IContent.ForceSafeAliases
        {
            get { return ForceSafeAliases; }
        }

        string IContent.PreviewBadge
        {
            get { return PreviewBadge; }
        }

        int IContent.UmbracoLibraryCacheDuration
        {
            get { return UmbracoLibraryCacheDuration; }
        }

        MacroErrorBehaviour IContent.MacroErrors
        {
            get { return MacroErrors; }
        }

        IconPickerBehaviour IContent.DocumentTypeIconList
        {
            get { return DocumentTypeIconList; }
        }

        IEnumerable<string> IContent.DisallowedUploadFiles
        {
            get { return DisallowedUploadFiles; }
        }

        bool IContent.CloneXmlContent
        {
            get { return CloneXmlContent; }
        }

        bool IContent.GlobalPreviewStorageEnabled
        {
            get { return GlobalPreviewStorageEnabled; }
        }

        string IContent.DefaultDocumentTypeProperty
        {
            get { return DefaultDocumentTypeProperty; }
        }
    }
}