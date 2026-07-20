// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// A handler for Rich Text editors used to bind to notifications.
/// </summary>
public class RichTextPropertyNotificationHandler : BlockEditorPropertyNotificationHandlerBase<RichTextBlockLayoutItem>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Core.PropertyEditors.RichTextPropertyNotificationHandler"/> class.
    /// </summary>
    /// <param name="logger">An <see cref="ILogger{RichTextPropertyNotificationHandler}"/> used for logging within the handler.</param>
    public RichTextPropertyNotificationHandler(ILogger<RichTextPropertyNotificationHandler> logger)
        : base(logger)
    {
    }

    protected override string EditorAlias => Constants.PropertyEditors.Aliases.RichText;
}
