// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Actions;

/// <summary>
///     This action is used as a security constraint that grants a user the ability to view elements in a tree
///     that has permissions applied to it.
/// </summary>
/// <remarks>
///     This action should not be invoked. It is used as the minimum required permission to view elements in the element tree.
///     By granting a user this permission, the user is able to see the element in the tree but not edit it.
/// </remarks>
public class ActionElementBrowse : IAction
{
    /// <inheritdoc cref="IAction.ActionLetter" />
    public const string ActionLetter = "Umb.Element.Read";

    /// <inheritdoc cref="IAction.ActionAlias" />
    public const string ActionAlias = "elementbrowse";

    /// <inheritdoc/>
    public string Letter => ActionLetter;

    /// <inheritdoc/>
    public string Alias => ActionAlias;

    /// <inheritdoc />
    public bool ShowInNotifier => false;

    /// <inheritdoc />
    public bool CanBePermissionAssigned => true;
}