// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Actions;

/// <summary>
///     This action is invoked upon creation of a document
/// </summary>
public class ActionNew : IAction
{
    /// <summary>
    ///     The unique action alias
    /// </summary>
    public const string ActionAlias = "create";

    /// <summary>
    ///     The unique action letter
    /// </summary>
    public const char ActionLetter = 'C';

    /// <inheritdoc />
    public char Letter => ActionLetter;

    /// <inheritdoc />
    public string Alias => ActionAlias;

    /// <inheritdoc />
    public string Icon => "icon-add";

    /// <inheritdoc />
    public bool ShowInNotifier => true;

    /// <inheritdoc />
    public bool CanBePermissionAssigned => true;

    /// <inheritdoc />
    public string Category => Constants.Conventions.PermissionCategories.ContentCategory;
}
