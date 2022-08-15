// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Actions;

/// <summary>
///     This action is invoked when copying a document, media, member
/// </summary>
public class ActionCopy : IAction
{
    /// <summary>
    ///     The unique action letter
    /// </summary>
    public const char ActionLetter = 'O';

    /// <inheritdoc />
    public char Letter => ActionLetter;

    /// <inheritdoc />
    public string Alias => "copy";

    /// <inheritdoc />
    public string Category => Constants.Conventions.PermissionCategories.StructureCategory;

    /// <inheritdoc />
    public string Icon => "icon-documents";

    /// <inheritdoc />
    public bool ShowInNotifier => true;

    /// <inheritdoc />
    public bool CanBePermissionAssigned => true;
}
