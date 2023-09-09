// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Actions;

/// <summary>
///     This action is invoked when creating a blueprint from a content
/// </summary>
public class ActionCreateBlueprintFromContent : IAction
{
    /// <inheritdoc cref="IAction.ActionLetter"/>
    public const char ActionLetter = 'ï';

    /// <inheritdoc cref="IAction.ActionAlias" />
    public const string ActionAlias = "createblueprint";

    /// <inheritdoc/>
    public char Letter => ActionLetter;

    /// <inheritdoc/>
    public string Alias => ActionAlias;

    /// <inheritdoc />
    public bool ShowInNotifier => false;

    /// <inheritdoc />
    public bool CanBePermissionAssigned => true;

    /// <inheritdoc />
    public string Icon => Constants.Icons.Blueprint;

    /// <inheritdoc />
    public string Category => Constants.Conventions.PermissionCategories.ContentCategory;
}
