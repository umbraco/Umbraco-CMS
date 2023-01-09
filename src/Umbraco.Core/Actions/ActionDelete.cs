// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Actions;

/// <summary>
/// This action is invoked when a document, media, member is deleted
/// </summary>
public class ActionDelete : IAction
{
    /// <inheritdoc cref="IAction.ActionLetter"/>
    public const char ActionLetter = 'D';

    /// <inheritdoc cref="IAction.ActionAlias"/>
    public const string ActionAlias = "delete";

    /// <inheritdoc/>
    public char Letter => ActionLetter;

    /// <inheritdoc/>
    public string Alias => ActionAlias;

    /// <inheritdoc/>
    public string Category => Constants.Conventions.PermissionCategories.ContentCategory;

    /// <inheritdoc/>
    public string Icon => "icon-delete";

    /// <inheritdoc/>
    public bool ShowInNotifier => true;

    /// <inheritdoc/>
    public bool CanBePermissionAssigned => true;
}
