// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Actions;

/// <summary>
///     This action is invoked upon modifying the notification of a content
/// </summary>
public class ActionNotify : IAction
{
    /// <inheritdoc />
    public char Letter => 'N';

    /// <inheritdoc />
    public bool ShowInNotifier => false;

    /// <inheritdoc />
    public bool CanBePermissionAssigned => true;

    /// <inheritdoc />
    public string Icon => "icon-megaphone";

    /// <inheritdoc />
    public string Alias => "notify";

    /// <inheritdoc />
    public string Category => Constants.Conventions.PermissionCategories.ContentCategory;
}
