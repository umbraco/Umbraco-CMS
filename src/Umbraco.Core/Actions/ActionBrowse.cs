// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Actions;

/// <summary>
///     This action is used as a security constraint that grants a user the ability to view nodes in a tree
///     that has  permissions applied to it.
/// </summary>
/// <remarks>
///     This action should not be invoked. It is used as the minimum required permission to view nodes in the content tree.
///     By
///     granting a user this permission, the user is able to see the node in the tree but not edit the document. This may
///     be used by other trees
///     that support permissions in the future.
/// </remarks>
public class ActionBrowse : IAction
{
    /// <summary>
    ///     The unique action letter
    /// </summary>
    public const char ActionLetter = 'F';

    /// <inheritdoc />
    public char Letter => ActionLetter;

    /// <inheritdoc />
    public bool ShowInNotifier => false;

    /// <inheritdoc />
    public bool CanBePermissionAssigned => true;

    /// <inheritdoc />
    public string Icon => string.Empty;

    /// <inheritdoc />
    public string Alias => "browse";

    /// <inheritdoc />
    public string Category => Constants.Conventions.PermissionCategories.ContentCategory;
}
