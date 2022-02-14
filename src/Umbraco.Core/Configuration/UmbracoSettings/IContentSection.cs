using System.Collections.Generic;
using Umbraco.Core.Macros;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IContentSection : IUmbracoConfigurationSection
    {
        string NotificationEmailAddress { get; }

        bool DisableHtmlEmail { get; }

        IEnumerable<string> ImageFileTypes { get; }

        IEnumerable<IImagingAutoFillUploadField> ImageAutoFillProperties { get; }

        bool ResolveUrlsFromTextString { get; }

        IEnumerable<IContentErrorPage> Error404Collection { get; }

        string PreviewBadge { get; }

        MacroErrorBehaviour MacroErrorBehaviour { get; }

        IEnumerable<string> DisallowedUploadFiles { get; }

        IEnumerable<string> AllowedUploadFiles { get; }

        IContentVersionCleanupPolicyGlobalSettings ContentVersionCleanupPolicyGlobalSettings { get; }

        /// <summary>
        /// Gets a value indicating whether to show deprecated property editors in
        /// a datatype list of available editors.
        /// </summary>
        bool ShowDeprecatedPropertyEditors { get; }

        string LoginBackgroundImage { get; }

        string LoginLogoImage { get; }
        bool HideBackOfficeLogo { get; }
    }
}
