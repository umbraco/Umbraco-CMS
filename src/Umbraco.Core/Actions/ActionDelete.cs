// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Actions;

/// <summary>
/// This action is invoked when a document, media, member is deleted
/// </summary>
public class ActionDelete : IAction
{
    /// <summary>
    /// The unique action alias
    /// </summary>
    public const string ActionAlias = "delete";

    /// <summary>
    /// The unique action letter
    /// </summary>
    public const char ActionLetter = 'D';

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
