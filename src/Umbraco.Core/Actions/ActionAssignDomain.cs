// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Actions;

/// <summary>
///     This action is invoked when a domain is being assigned to a document
/// </summary>
public class ActionAssignDomain : IAction
{
    /// <inheritdoc cref="IAction.ActionLetter" />
    public const char ActionLetter = 'I';

    /// <inheritdoc cref="IAction.ActionAlias" />
    public const string ActionAlias = "assigndomain";

    /// <inheritdoc/>
    public char Letter => ActionLetter;

    /// <inheritdoc/>
    public string Alias => ActionAlias;

    /// <inheritdoc />
    public string Category => Constants.Conventions.PermissionCategories.AdministrationCategory;

    /// <inheritdoc />
    public string Icon => "icon-home";

    /// <inheritdoc />
    public bool ShowInNotifier => false;

    /// <inheritdoc />
    public bool CanBePermissionAssigned => true;
}
