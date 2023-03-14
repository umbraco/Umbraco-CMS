// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     A handler for Block editors used to bind to notifications
/// </summary>
// Scheduled for removal in v12
[Obsolete("Please use BlockListPropertyNotificationHandler instead")]
public class BlockEditorPropertyHandler : BlockListPropertyNotificationHandler
{
    public BlockEditorPropertyHandler(ILogger<BlockEditorPropertyHandler> logger)
        : base(logger)
    {
    }
}
