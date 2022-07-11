// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Actions;

/// <summary>
///     This action is invoked when a domain is being assigned to a document
/// </summary>
public class ActionAssignDomain : IAction
{
    /// <summary>
    ///     The unique action letter
    /// </summary>
    public const char ActionLetter = 'I';

    /// <inheritdoc />
    public char Letter => ActionLetter;

    /// <inheritdoc />
    // This is all lower-case because of case sensitive filesystems, see issue: https://github.com/umbraco/Umbraco-CMS/issues/11670
    public string Alias => "assigndomain";

    /// <inheritdoc />
    public string Category => Constants.Conventions.PermissionCategories.AdministrationCategory;

    /// <inheritdoc />
    public string Icon => "icon-home";

    /// <inheritdoc />
    public bool ShowInNotifier => false;

    /// <inheritdoc />
    public bool CanBePermissionAssigned => true;
}
