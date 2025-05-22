// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for content settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigContent)]
public class ContentSettings
{
    internal const bool StaticResolveUrlsFromTextString = false;

    internal const string StaticDefaultPreviewBadge = @"
<script src=""{0}/website/preview.js""></script>
<umb-website-preview path=""{0}"" url=""{1}"" unique=""{2}""></umb-website-preview>";

    internal const string StaticDisallowedUploadFiles = "ashx,aspx,ascx,config,cshtml,vbhtml,asmx,air,axd,xamlx";
    internal const bool StaticShowDeprecatedPropertyEditors = false;
    internal const string StaticLoginBackgroundImage = "assets/login.jpg";
    internal const string StaticLoginLogoImage = "assets/logo_light.svg";
    internal const string StaticLoginLogoImageAlternative = "assets/logo_dark.svg";
    internal const string StaticBackOfficeLogo = "assets/logo.svg";
    internal const string StaticBackOfficeLogoAlternative = "assets/logo_blue.svg";
    internal const bool StaticHideBackOfficeLogo = false;
    internal const bool StaticDisableDeleteWhenReferenced = false;
    internal const bool StaticDisableUnpublishWhenReferenced = false;
    internal const bool StaticAllowEditInvariantFromNonDefault = false;
    internal const bool StaticShowDomainWarnings = true;
    internal const bool StaticShowUnroutableContentWarnings = true;

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
    public ISet<ContentErrorPage> Error404Collection { get; set; } = new HashSet<ContentErrorPage>();

    /// <summary>
    ///     Gets or sets a value for the preview badge mark-up.
    /// </summary>
    [DefaultValue(StaticDefaultPreviewBadge)]
    public string PreviewBadge { get; set; } = StaticDefaultPreviewBadge;

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
    ///     Gets or sets a value for the path to the login screen logo image
    ///     shown on top of the background image set in <see cref="LoginBackgroundImage" />.
    /// </summary>
    /// <remarks>The alternative version of this logo can be found at <see cref="LoginLogoImageAlternative"/>.</remarks>
    [DefaultValue(StaticLoginLogoImage)]
    public string LoginLogoImage { get; set; } = StaticLoginLogoImage;

    /// <summary>
    ///     Gets or sets a value for the path to the login screen logo image when shown on top
    ///     of a light background (e.g. in mobile resolutions).
    /// </summary>
    /// <remarks>This is the alternative version to the regular logo found at <see cref="LoginLogoImage"/>.</remarks>
    [DefaultValue(StaticLoginLogoImageAlternative)]
    public string LoginLogoImageAlternative { get; set; } = StaticLoginLogoImageAlternative;

    /// <summary>
    ///     Gets or sets a value for the path to the backoffice logo.
    /// </summary>
    /// <remarks>The alternative version of this logo can be found at <see cref="BackOfficeLogoAlternative"/>.</remarks>
    [DefaultValue(StaticBackOfficeLogo)]
    public string BackOfficeLogo { get; set; } = StaticBackOfficeLogo;

    /// <summary>
    ///     Gets or sets a value for the path to the alternative backoffice logo, which can be shown
    ///     on top of a light background.
    /// </summary>
    /// <remarks>This is the alternative version to the regular logo found at <see cref="BackOfficeLogo"/>.</remarks>
    [DefaultValue(StaticBackOfficeLogoAlternative)]
    public string BackOfficeLogoAlternative { get; set; } = StaticBackOfficeLogoAlternative;

    /// <summary>
    ///     Gets or sets a value indicating whether to hide the backoffice umbraco logo or not.
    /// </summary>
    [DefaultValue(StaticHideBackOfficeLogo)]
    [Obsolete("This setting is no longer used and will be removed in future versions. An alternative BackOffice logo can be set using the BackOfficeLogo setting.")]
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
    ///     Gets or sets the model representing the global content version cleanup policy
    /// </summary>
    public ContentVersionCleanupPolicySettings ContentVersionCleanupPolicy { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether to allow editing invariant properties from a non-default language variation.
    /// </summary>
    [DefaultValue(StaticAllowEditInvariantFromNonDefault)]
    public bool AllowEditInvariantFromNonDefault { get; set; } = StaticAllowEditInvariantFromNonDefault;

    /// <summary>
    ///     Gets or sets a value for the collection of file extensions that are allowed for upload.
    /// </summary>
    public ISet<string> AllowedUploadedFileExtensions { get; set; } = new HashSet<string>();

    /// <summary>
    ///     Gets or sets a value for the collection of file extensions that are disallowed for upload.
    /// </summary>
    [DefaultValue(StaticDisallowedUploadFiles)]
    public ISet<string> DisallowedUploadedFileExtensions { get; set; } = new HashSet<string>(StaticDisallowedUploadFiles.Split(Constants.CharArrays.Comma));

    /// <summary>
    /// Gets or sets the allowed external host for media. If empty only relative paths are allowed.
    /// </summary>
    public ISet<string> AllowedMediaHosts { get; set; } = new HashSet<string>();

    /// <summary>
    /// Gets or sets a value indicating whether to show domain warnings.
    /// </summary>
    [DefaultValue(StaticShowDomainWarnings)]
    public bool ShowDomainWarnings { get; set; } = StaticShowDomainWarnings;

    /// <summary>
    /// Gets or sets a value indicating whether to show unroutable content warnings.
    /// </summary>
    [DefaultValue(StaticShowUnroutableContentWarnings)]
    public bool ShowUnroutableContentWarnings { get; set; } = StaticShowUnroutableContentWarnings;
}
