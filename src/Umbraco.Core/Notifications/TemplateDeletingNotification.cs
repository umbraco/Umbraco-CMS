// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
///  A notification that is used to trigger the <see cref="Services.ITemplateService"/> when a template is being deleted.
/// </summary>
public class TemplateDeletingNotification : DeletingNotification<ITemplate>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TemplateDeletingNotification"/> class
    ///     with a single template.
    /// </summary>
    /// <param name="target">The template being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public TemplateDeletingNotification(ITemplate target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TemplateDeletingNotification"/> class
    ///     with multiple templates.
    /// </summary>
    /// <param name="target">The templates being deleted.</param>
    /// <param name="messages">The event messages collection.</param>
    public TemplateDeletingNotification(IEnumerable<ITemplate> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
