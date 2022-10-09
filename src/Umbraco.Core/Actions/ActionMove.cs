// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Actions;

/// <summary>
///     This action is invoked upon creation of a document, media, member
/// </summary>
public class ActionMove : IAction
{
    /// <summary>
    ///     The unique action letter
    /// </summary>
    public const char ActionLetter = 'M';

    /// <inheritdoc />
    public char Letter => ActionLetter;

    /// <inheritdoc />
    public string Alias => "move";

    /// <inheritdoc />
    public string Category => Constants.Conventions.PermissionCategories.StructureCategory;

    /// <inheritdoc />
    public string Icon => "icon-enter";

    /// <inheritdoc />
    public bool ShowInNotifier => true;

    /// <inheritdoc />
    public bool CanBePermissionAssigned => true;
}
