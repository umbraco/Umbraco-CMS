// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Actions;

/// <summary>
///     This action is invoked upon modifying the notification of a content.
/// </summary>
public class ActionNotify : IAction
{
    /// <inheritdoc cref="IAction.ActionLetter"/>
    public const string ActionLetter = "Umb.Document.Notifications";

    /// <inheritdoc cref="IAction.ActionAlias"/>
    public const string ActionAlias = "notify";

    /// <inheritdoc/>
    public string Letter => ActionLetter;

    /// <inheritdoc/>
    public string Alias => ActionAlias;

    /// <inheritdoc />
    public bool ShowInNotifier => false;

    /// <inheritdoc />
    public bool CanBePermissionAssigned => true;

    /// <inheritdoc />
    public string Icon => "icon-megaphone";

    /// <inheritdoc />
    public string Category => Constants.Conventions.PermissionCategories.ContentCategory;
}
