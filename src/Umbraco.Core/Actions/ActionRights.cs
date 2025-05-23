// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Actions;

/// <summary>
///     This action is invoked when rights are changed on a document.
/// </summary>
public class ActionRights : IAction
{
    /// <inheritdoc cref="IAction.ActionLetter"/>
    public const string ActionLetter = "Umb.Document.Permissions";

    /// <inheritdoc cref="IAction.ActionAlias"/>
    public const string ActionAlias = "rights";

    /// <inheritdoc/>
    public string Letter => ActionLetter;

    /// <inheritdoc/>
    public string Alias => ActionAlias;

    /// <inheritdoc />
    public string Category => Constants.Conventions.PermissionCategories.ContentCategory;

    /// <inheritdoc />
    public string Icon => "icon-vcard";

    /// <inheritdoc />
    public bool ShowInNotifier => true;

    /// <inheritdoc />
    public bool CanBePermissionAssigned => true;
}
