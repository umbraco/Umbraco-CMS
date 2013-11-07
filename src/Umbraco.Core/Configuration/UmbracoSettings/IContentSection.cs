using System.Collections.Generic;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IContentSection : IUmbracoConfigurationSection
    {
        string NotificationEmailAddress { get; }

        bool DisableHtmlEmail { get; }

        IEnumerable<string> ImageFileTypes { get; }

        IEnumerable<string> ImageTagAllowedAttributes { get; }

        IEnumerable<IImagingAutoFillUploadField> ImageAutoFillProperties { get; }

        string ScriptFolderPath { get; }

        IEnumerable<string> ScriptFileTypes { get; }

        bool ScriptEditorDisable { get; }

        bool ResolveUrlsFromTextString { get; }

        bool UploadAllowDirectories { get; }

        IEnumerable<IContentErrorPage> Error404Collection { get; }

        bool EnsureUniqueNaming { get; }

        bool TidyEditorContent { get; }

        string TidyCharEncoding { get; }

        bool XmlCacheEnabled { get; }

        bool ContinouslyUpdateXmlDiskCache { get; }

        bool XmlContentCheckForDiskChanges { get; }

        bool EnableSplashWhileLoading { get; }

        string PropertyContextHelpOption { get; }

        bool UseLegacyXmlSchema { get; }

        bool ForceSafeAliases { get; }

        string PreviewBadge { get; }

        int UmbracoLibraryCacheDuration { get; }

        MacroErrorBehaviour MacroErrorBehaviour { get; }
        
        IEnumerable<string> DisallowedUploadFiles { get; }

        bool CloneXmlContent { get; }

        bool GlobalPreviewStorageEnabled { get; }

        string DefaultDocumentTypeProperty { get; }
    }
}