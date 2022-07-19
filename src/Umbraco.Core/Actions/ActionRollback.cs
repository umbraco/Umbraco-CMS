// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Actions;

/// <summary>
///     This action is invoked when copying a document is being rolled back
/// </summary>
public class ActionRollback : IAction
{
    /// <summary>
    ///     The unique action letter
    /// </summary>
    public const char ActionLetter = 'K';

    /// <inheritdoc />
    public char Letter => ActionLetter;

    /// <inheritdoc />
    public string Alias => "rollback";

    /// <inheritdoc />
    public string Category => Constants.Conventions.PermissionCategories.AdministrationCategory;

    /// <inheritdoc />
    public string Icon => "icon-undo";

    /// <inheritdoc />
    public bool ShowInNotifier => true;

    /// <inheritdoc />
    public bool CanBePermissionAssigned => true;
}
