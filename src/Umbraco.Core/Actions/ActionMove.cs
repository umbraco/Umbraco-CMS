// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Actions;

/// <summary>
///     This action is invoked upon creation of a document, media, member.
/// </summary>
public class ActionMove : IAction
{
    /// <inheritdoc cref="IAction.ActionLetter"/>
    public const string ActionLetter = "Umb.Document.Move";

    /// <inheritdoc cref="IAction.ActionAlias"/>
    public const string ActionAlias = "move";

    /// <inheritdoc/>
    public string Letter => ActionLetter;

    /// <inheritdoc/>
    public string Alias => ActionAlias;

    /// <inheritdoc />
    public string Category => Constants.Conventions.PermissionCategories.StructureCategory;

    /// <inheritdoc />
    public string Icon => "icon-enter";

    /// <inheritdoc />
    public bool ShowInNotifier => true;

    /// <inheritdoc />
    public bool CanBePermissionAssigned => true;
}
