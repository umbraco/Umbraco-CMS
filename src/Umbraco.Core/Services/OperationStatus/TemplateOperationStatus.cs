using System;

namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum TemplateOperationStatus
{
    Success,
    CancelledByNotification,
    InvalidAlias,
    DuplicateAlias,
    TemplateNotFound,
    LayoutNotFound,
    CircularLayoutReference,
    LayoutCannotBeDeleted,

    /// <summary>
    ///     The master template was not found
    /// </summary>
    [Obsolete("Use LayoutNotFound instead. This will be removed in Umbraco 19.")]
    MasterTemplateNotFound = LayoutNotFound,

    /// <summary>
    ///     A circular master template reference was detected
    /// </summary>
    [Obsolete("Use CircularLayoutReference instead. This will be removed in Umbraco 19.")]
    CircularMasterTemplateReference = CircularLayoutReference,

    /// <summary>
    ///     The master template cannot be deleted
    /// </summary>
    [Obsolete("Use LayoutCannotBeDeleted instead. This will be removed in Umbraco 19.")]
    MasterTemplateCannotBeDeleted = LayoutCannotBeDeleted,
}
