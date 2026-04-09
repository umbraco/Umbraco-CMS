// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Actions;

/// <summary>
///     This action is used as a security constraint that grants a user the ability to move element folders.
/// </summary>
public class ActionElementFolderMove : IAction
{
    /// <inheritdoc cref="IAction.ActionLetter" />
    public const string ActionLetter = "Umb.ElementFolder.Move";

    /// <inheritdoc cref="IAction.ActionAlias" />
    public const string ActionAlias = "elementfoldermove";

    /// <inheritdoc/>
    public string Letter => ActionLetter;

    /// <inheritdoc/>
    public string Alias => ActionAlias;

    /// <inheritdoc />
    public bool ShowInNotifier => true;

    /// <inheritdoc />
    public bool CanBePermissionAssigned => true;
}
