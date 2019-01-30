using System.Collections.Generic;
using Umbraco.Core.Macros;

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

        bool XmlCacheEnabled { get; }

        bool ContinouslyUpdateXmlDiskCache { get; }

        bool XmlContentCheckForDiskChanges { get; }

        string PropertyContextHelpOption { get; }

        bool ForceSafeAliases { get; }

        string PreviewBadge { get; }

        MacroErrorBehaviour MacroErrorBehaviour { get; }

        IEnumerable<string> DisallowedUploadFiles { get; }

        IEnumerable<string> AllowedUploadFiles { get; }

        bool CloneXmlContent { get; }

        bool GlobalPreviewStorageEnabled { get; }

        string DefaultDocumentTypeProperty { get; }

        /// <summary>
        /// Gets a value indicating whether to show deprecated property editors in
        /// a datatype list of available editors.
        /// </summary>
        bool ShowDeprecatedPropertyEditors { get; }

        bool EnableInheritedDocumentTypes { get; }

        bool EnableInheritedMediaTypes { get; }

        string LoginBackgroundImage { get; }
    }
}
