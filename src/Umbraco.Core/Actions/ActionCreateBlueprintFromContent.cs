// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Actions;

/// <summary>
///     This action is invoked when creating a blueprint from a content
/// </summary>
public class ActionCreateBlueprintFromContent : IAction
{
    /// <inheritdoc />
    public char Letter => 'ï';

    /// <inheritdoc />
    public bool ShowInNotifier => false;

    /// <inheritdoc />
    public bool CanBePermissionAssigned => true;

    /// <inheritdoc />
    public string Icon => Constants.Icons.Blueprint;

    /// <inheritdoc />
    public string Alias => "createblueprint";

    /// <inheritdoc />
    public string Category => Constants.Conventions.PermissionCategories.ContentCategory;
}
