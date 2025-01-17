// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Actions;

/// <summary>
///     This action is invoked when children to a document, media, member is being sorted.
/// </summary>
public class ActionSort : IAction
{
    /// <inheritdoc cref="IAction.ActionLetter"/>
    public const string ActionLetter = "Umb.Document.Sort";

    /// <inheritdoc cref="IAction.ActionAlias"/>
    public const string ActionAlias = "sort";

    /// <inheritdoc/>
    public string Letter => ActionLetter;

    /// <inheritdoc/>
    public string Alias => ActionAlias;

    /// <inheritdoc />
    public string Category => Constants.Conventions.PermissionCategories.StructureCategory;

    /// <inheritdoc />
    public string Icon => "icon-navigation-vertical";

    /// <inheritdoc />
    public bool ShowInNotifier => true;

    /// <inheritdoc />
    public bool CanBePermissionAssigned => true;
}
