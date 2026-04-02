// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published before a package is imported.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the package import
///     by setting <see cref="Cancel"/> to <c>true</c>.
/// </remarks>
public class ImportingPackageNotification : StatefulNotification, ICancelableNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ImportingPackageNotification"/> class.
    /// </summary>
    /// <param name="packageName">The name of the package being imported.</param>
    public ImportingPackageNotification(string packageName) => PackageName = packageName;

    /// <summary>
    ///     Gets the name of the package being imported.
    /// </summary>
    public string PackageName { get; }

    /// <inheritdoc />
    public bool Cancel { get; set; }
}
