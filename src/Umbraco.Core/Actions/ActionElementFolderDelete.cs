// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Actions;

/// <summary>
///     This action is used as a security constraint that grants a user the ability to delete element folders.
/// </summary>
public class ActionElementFolderDelete : IAction
{
    /// <inheritdoc cref="IAction.ActionLetter" />
    public const string ActionLetter = "Umb.ElementFolder.Delete";

    /// <inheritdoc cref="IAction.ActionAlias" />
    public const string ActionAlias = "elementfolderdelete";

    /// <inheritdoc/>
    public string Letter => ActionLetter;

    /// <inheritdoc/>
    public string Alias => ActionAlias;

    /// <inheritdoc />
    public bool ShowInNotifier => true;

    /// <inheritdoc />
    public bool CanBePermissionAssigned => true;
}
