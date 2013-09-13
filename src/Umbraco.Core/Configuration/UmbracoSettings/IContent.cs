using System.Collections.Generic;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IContent
    {
        IContentImaging Imaging { get; }

        IContentScriptEditor ScriptEditor { get; }

        bool EnableCanvasEditing { get; }

        bool ResolveUrlsFromTextString { get; }

        bool UploadAllowDirectories { get; }

        IContentErrors Errors { get; }

        INotifications Notifications { get; }

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

        IconPickerBehaviour IconPickerBehaviour { get; }

        IEnumerable<string> DisallowedUploadFiles { get; }

        bool CloneXmlContent { get; }

        bool GlobalPreviewStorageEnabled { get; }

        string DefaultDocumentTypeProperty { get; }
    }
}