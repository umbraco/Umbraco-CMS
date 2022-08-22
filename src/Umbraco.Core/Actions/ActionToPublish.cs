// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Actions;

/// <summary>
///     This action is invoked when children to a document is being sent to published (by an editor without publishrights)
/// </summary>
public class ActionToPublish : IAction
{
    /// <summary>
    ///     The unique action letter
    /// </summary>
    public const char ActionLetter = 'H';

    /// <inheritdoc />
    public char Letter => ActionLetter;

    /// <inheritdoc />
    public string Alias => "sendtopublish";

    /// <inheritdoc />
    public string Category => Constants.Conventions.PermissionCategories.ContentCategory;

    /// <inheritdoc />
    public string Icon => "icon-outbox";

    /// <inheritdoc />
    public bool ShowInNotifier => true;

    /// <inheritdoc />
    public bool CanBePermissionAssigned => true;
}
