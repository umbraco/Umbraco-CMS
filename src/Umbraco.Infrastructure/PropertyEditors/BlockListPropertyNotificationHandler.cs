// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// A handler for Block list editors used to bind to notifications
/// </summary>
public class BlockListPropertyNotificationHandler : BlockEditorPropertyNotificationHandlerBase<BlockListLayoutItem>
{
    public BlockListPropertyNotificationHandler(ILogger<BlockListPropertyNotificationHandler> logger)
        : base(logger)
    {
    }

    protected override string EditorAlias => Constants.PropertyEditors.Aliases.BlockList;
}
