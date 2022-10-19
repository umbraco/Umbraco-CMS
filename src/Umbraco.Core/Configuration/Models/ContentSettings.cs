// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;
using Umbraco.Cms.Core.Macros;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for content settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigContent)]
public class ContentSettings
{
    internal const bool StaticResolveUrlsFromTextString = false;

    internal const string StaticDefaultPreviewBadge =
        @"
            <div id=""umbracoPreviewBadge"" class=""umbraco-preview-badge"">
                <span class=""umbraco-preview-badge__header"">Preview mode</span>
                <a href=""{0}/preview/?id={2}"" class=""umbraco-preview-badge__a open"" title=""Open preview in BackOffice"">
                    …
                </a>
                <a href=""{0}/preview/end?redir={1}"" class=""umbraco-preview-badge__a end"" title=""End preview mode"">
                    <svg viewBox=""0 0 100 100"" xmlns=""http://www.w3.org/2000/svg""><title>Click to end preview mode</title><path fill=""#fff"" d=""M5273.1 2400.1v-2c0-2.8-5-4-9.7-4s-9.7 1.3-9.7 4v2a7 7 0 002 4.9l5 4.9c.3.3.4.6.4 1v6.4c0 .4.2.7.6.8l2.9.9c.5.1 1-.2 1-.8v-7.2c0-.4.2-.7.4-1l5.1-5a7 7 0 002-4.9zm-9.7-.1c-4.8 0-7.4-1.3-7.5-1.8.1-.5 2.7-1.8 7.5-1.8s7.3 1.3 7.5 1.8c-.2.5-2.7 1.8-7.5 1.8z""/><path fill=""#fff"" d=""M5268.4 2410.3c-.6 0-1 .4-1 1s.4 1 1 1h4.3c.6 0 1-.4 1-1s-.4-1-1-1h-4.3zM5272.7 2413.7h-4.3c-.6 0-1 .4-1 1s.4 1 1 1h4.3c.6 0 1-.4 1-1s-.4-1-1-1zM5272.7 2417h-4.3c-.6 0-1 .4-1 1s.4 1 1 1h4.3c.6 0 1-.4 1-1 0-.5-.4-1-1-1z""/><path fill=""#fff"" d=""M78.2 13l-8.7 11.7a32.5 32.5 0 11-51.9 25.8c0-10.3 4.7-19.7 12.9-25.8L21.8 13a47 47 0 1056.4 0z""/><path fill=""#fff"" d=""M42.7 2.5h14.6v49.4H42.7z""/></svg>
                </a>
            </div>
            <style type=""text/css"">
                .umbraco-preview-badge {{
                    position: fixed;
                    bottom: 0;
                    display: inline-flex;
                    background: rgba(27, 38, 79, 0.9);
                    color: #fff;
                    font-size: 12px;
                    z-index: 99999999;
                    justify-content: center;
                    align-items: center;
                    box-shadow: 0 5px 10px rgba(0, 0, 0, .2), 0 1px 2px rgba(0, 0, 0, .2);
                    line-height: 1;
                    pointer-events:none;
                    left: 50%;
                    transform: translate(-50%, 40px);
                    animation: umbraco-preview-badge--effect 10s 1.2s ease both;
                    border-radius: 3px 3px 0 0;
                }}
                @keyframes umbraco-preview-badge--effect {{
                    0% {{
                        transform: translate(-50%, 40px);
                        animation-timing-function: ease-out;
                    }}
                    1.5% {{
                        transform: translate(-50%, -20px);
                        animation-timing-function: ease-in;
                    }}
                    5.0% {{
                        transform: translate(-50%, -8px);
                        animation-timing-function: ease-in;
                    }}
                    7.5% {{
                        transform: translate(-50%, -4px);
                        animation-timing-function: ease-in;
                    }}
                    9.2% {{
                        transform: translate(-50%, -2px);
                        animation-timing-function: ease-in;
                    }}
                    3.5%,
                    6.5%,
                    8.5% {{
                        transform: translate(-50%, 0);
                        animation-timing-function: ease-out;
                    }}
                    9.7% {{
                        transform: translate(-50%, 0);
                        animation-timing-function: ease-out;
                    }}
                    10.0% {{
                        transform: translate(-50%, 0);
                    }}


                    60% {{
                        transform: translate(-50%, 0);
                        animation-timing-function: ease-out;
                    }}
                    61.5% {{
                        transform: translate(-50%, -20px);
                        animation-timing-function: ease-in;
                    }}
                    65.0% {{
                        transform: translate(-50%, -8px);
                        animation-timing-function: ease-in;
                    }}
                    67.5% {{
                        transform: translate(-50%, -4px);
                        animation-timing-function: ease-in;
                    }}
                    69.2% {{
                        transform: translate(-50%, -2px);
                        animation-timing-function: ease-in;
                    }}
                    63.5%,
                    66.5%,
                    68.5% {{
                        transform: translate(-50%, 0);
                        animation-timing-function: ease-out;
                    }}
                    69.7% {{
                        transform: translate(-50%, 0);
                        animation-timing-function: ease-out;
                    }}
                    70.0% {{
                        transform: translate(-50%, 0);
                    }}
                    100.0% {{
                        transform: translate(-50%, 0);
                    }}
                }}
                .umbraco-preview-badge__header {{
                    padding: 1em;
                    font-weight: bold;
                    pointer-events:none;
                }}
                .umbraco-preview-badge__a {{
                    width: 3em;
                    padding: 1em;
                    display: flex;
                    flex-shrink: 0;
                    align-items: center;
                    align-self: stretch;
                    color:white;
                    text-decoration:none;
                    font-weight: bold;
                    border-left: 1px solid hsla(0,0%,100%,.25);
                    pointer-events:all;
                }}
                .umbraco-preview-badge__a svg {{
                    width: 1em;
                    height:1em;
                }}
                .umbraco-preview-badge__a:hover {{
                    background: #202d5e;
                }}
                .umbraco-preview-badge__end svg {{
                    fill: #fff;
                    width:1em;
                }}
            </style>
            <script type=""text/javascript"" data-umbraco-path=""{0}"" src=""{0}/js/umbraco.websitepreview.min.js""></script>";

    internal const string StaticMacroErrors = "Inline";
    internal const string StaticDisallowedUploadFiles = "ashx,aspx,ascx,config,cshtml,vbhtml,asmx,air,axd,xamlx";
    internal const bool StaticShowDeprecatedPropertyEditors = false;
    internal const string StaticLoginBackgroundImage = "assets/img/login.jpg";
    internal const string StaticLoginLogoImage = "assets/img/application/umbraco_logo_white.svg";
    internal const bool StaticHideBackOfficeLogo = false;
    internal const bool StaticDisableDeleteWhenReferenced = false;
    internal const bool StaticDisableUnpublishWhenReferenced = false;
    internal const bool StaticAllowEditInvariantFromNonDefault = false;

    /// <summary>
    ///     Gets or sets a value for the content notification settings.
    /// </summary>
    public ContentNotificationSettings Notifications { get; set; } = new();

    /// <summary>
    ///     Gets or sets a value for the content imaging settings.
    /// </summary>
    public ContentImagingSettings Imaging { get; set; } = new();

    /// <summary>
    ///     Gets or sets a value indicating whether URLs should be resolved from text strings.
    /// </summary>
    [DefaultValue(StaticResolveUrlsFromTextString)]
    public bool ResolveUrlsFromTextString { get; set; } = StaticResolveUrlsFromTextString;

    /// <summary>
    ///     Gets or sets a value for the collection of error pages.
    /// </summary>
    public ContentErrorPage[] Error404Collection { get; set; } = Array.Empty<ContentErrorPage>();

    /// <summary>
    ///     Gets or sets a value for the preview badge mark-up.
    /// </summary>
    [DefaultValue(StaticDefaultPreviewBadge)]
    public string PreviewBadge { get; set; } = StaticDefaultPreviewBadge;

    /// <summary>
    ///     Gets or sets a value for the macro error behaviour.
    /// </summary>
    [DefaultValue(StaticMacroErrors)]
    public MacroErrorBehaviour MacroErrors { get; set; } = Enum<MacroErrorBehaviour>.Parse(StaticMacroErrors);

    /// <summary>
    ///     Gets or sets a value for the collection of file extensions that are disallowed for upload.
    /// </summary>
    [DefaultValue(StaticDisallowedUploadFiles)]
    public IEnumerable<string> DisallowedUploadFiles { get; set; } = StaticDisallowedUploadFiles.Split(',');

    /// <summary>
    ///     Gets or sets a value for the collection of file extensions that are allowed for upload.
    /// </summary>
    public IEnumerable<string> AllowedUploadFiles { get; set; } = Array.Empty<string>();

    /// <summary>
    ///     Gets or sets a value indicating whether deprecated property editors should be shown.
    /// </summary>
    [DefaultValue(StaticShowDeprecatedPropertyEditors)]
    public bool ShowDeprecatedPropertyEditors { get; set; } = StaticShowDeprecatedPropertyEditors;

    /// <summary>
    ///     Gets or sets a value for the path to the login screen background image.
    /// </summary>
    [DefaultValue(StaticLoginBackgroundImage)]
    public string LoginBackgroundImage { get; set; } = StaticLoginBackgroundImage;

    /// <summary>
    ///     Gets or sets a value for the path to the login screen logo image.
    /// </summary>
    [DefaultValue(StaticLoginLogoImage)]
    public string LoginLogoImage { get; set; } = StaticLoginLogoImage;

    /// <summary>
    ///     Gets or sets a value indicating whether to hide the backoffice umbraco logo or not.
    /// </summary>
    [DefaultValue(StaticHideBackOfficeLogo)]
    public bool HideBackOfficeLogo { get; set; } = StaticHideBackOfficeLogo;

    /// <summary>
    ///     Gets or sets a value indicating whether to disable the deletion of items referenced by other items.
    /// </summary>
    [DefaultValue(StaticDisableDeleteWhenReferenced)]
    public bool DisableDeleteWhenReferenced { get; set; } = StaticDisableDeleteWhenReferenced;

    /// <summary>
    ///     Gets or sets a value indicating whether to disable the unpublishing of items referenced by other items.
    /// </summary>
    [DefaultValue(StaticDisableUnpublishWhenReferenced)]
    public bool DisableUnpublishWhenReferenced { get; set; } = StaticDisableUnpublishWhenReferenced;

    /// <summary>
    ///     Get or sets the model representing the global content version cleanup policy
    /// </summary>
    public ContentVersionCleanupPolicySettings ContentVersionCleanupPolicy { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether to allow editing invariant properties from a non-default language variation.
    /// </summary>
    [DefaultValue(StaticAllowEditInvariantFromNonDefault)]
    public bool AllowEditInvariantFromNonDefault { get; set; } = StaticAllowEditInvariantFromNonDefault;
}
