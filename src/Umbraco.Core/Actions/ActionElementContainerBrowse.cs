// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Actions;

/// <summary>
///     This action is used as a security constraint that grants a user the ability to view element containers in a tree
///     that has permissions applied to it.
/// </summary>
/// <remarks>
///     This action should not be invoked. It is used as the minimum required permission to view element containers in the element tree.
///     By granting a user this permission, the user is able to see the element container in the tree but not edit it.
/// </remarks>
public class ActionElementContainerBrowse : IAction
{
    /// <inheritdoc cref="IAction.ActionLetter" />
    public const string ActionLetter = "Umb.ElementContainer.Read";

    /// <inheritdoc cref="IAction.ActionAlias" />
    public const string ActionAlias = "elementcontainerbrowse";

    /// <inheritdoc/>
    public string Letter => ActionLetter;

    /// <inheritdoc/>
    public string Alias => ActionAlias;

    /// <inheritdoc />
    public bool ShowInNotifier => false;

    /// <inheritdoc />
    public bool CanBePermissionAssigned => true;
}
