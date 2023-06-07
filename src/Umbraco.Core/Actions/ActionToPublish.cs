// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Actions;

/// <summary>
///     This action is invoked when children to a document is being sent to published (by an editor without publishrights).
/// </summary>
[Obsolete("Scheduled for removal in v13")]
public class ActionToPublish : IAction
{
    /// <inheritdoc cref="IAction.ActionLetter"/>
    public const char ActionLetter = 'H';

    /// <inheritdoc cref="IAction.ActionAlias"/>
    public const string ActionAlias = "sendtopublish";

    /// <inheritdoc/>
    public char Letter => ActionLetter;

    /// <inheritdoc/>
    public string Alias => ActionAlias;

    /// <inheritdoc />
    public string Category => Constants.Conventions.PermissionCategories.ContentCategory;

    /// <inheritdoc />
    public string Icon => "icon-outbox";

    /// <inheritdoc />
    public bool ShowInNotifier => true;

    /// <inheritdoc />
    public bool CanBePermissionAssigned => true;
}
