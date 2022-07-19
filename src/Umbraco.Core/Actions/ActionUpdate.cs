// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Actions;

/// <summary>
///     This action is invoked when copying a document or media
/// </summary>
public class ActionUpdate : IAction
{
    /// <summary>
    ///     The unique action letter
    /// </summary>
    public const char ActionLetter = 'A';

    /// <inheritdoc />
    public char Letter => ActionLetter;

    /// <inheritdoc />
    public string Alias => "update";

    /// <inheritdoc />
    public string Category => Constants.Conventions.PermissionCategories.ContentCategory;

    /// <inheritdoc />
    public string Icon => "icon-save";

    /// <inheritdoc />
    public bool ShowInNotifier => true;

    /// <inheritdoc />
    public bool CanBePermissionAssigned => true;
}
