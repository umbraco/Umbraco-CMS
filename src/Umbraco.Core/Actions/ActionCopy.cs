// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Actions;

/// <summary>
///     This action is invoked when copying a document, media, member
/// </summary>
public class ActionCopy : IAction
{
    /// <inheritdoc cref="IAction.ActionLetter" />
    public const string ActionLetter = "Umb.Document.Duplicate";

    /// <inheritdoc cref="IAction.ActionAlias" />
    public const string ActionAlias = "copy";

    /// <inheritdoc/>
    public string Letter => ActionLetter;

    /// <inheritdoc/>
    public string Alias => ActionAlias;

    /// <inheritdoc />
    public string Category => Constants.Conventions.PermissionCategories.StructureCategory;

    /// <inheritdoc />
    public string Icon => "icon-documents";

    /// <inheritdoc />
    public bool ShowInNotifier => true;

    /// <inheritdoc />
    public bool CanBePermissionAssigned => true;
}
