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
    public RichTextPropertyNotificationHandler(ILogger<RichTextPropertyNotificationHandler> logger)
        : base(logger)
    {
    }

    protected override string EditorAlias => Constants.PropertyEditors.Aliases.RichText;
}
