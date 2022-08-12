// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// A handler for Block grid editors used to bind to notifications
/// </summary>
public class BlockGridPropertyNotificationHandler : BlockEditorPropertyNotificationHandlerBase<BlockGridLayoutItem>
{
    public BlockGridPropertyNotificationHandler(ILogger<BlockGridPropertyNotificationHandler> logger, IJsonSerializer jsonSerializer)
        : base(new BlockGridEditorDataConverter(jsonSerializer), logger)
    {
    }

    protected override string EditorAlias => Constants.PropertyEditors.Aliases.BlockGrid;

    /*
    Niels:
    Change:
    TODO: Here we need to take children of layout item into account.
    */
    protected override void UpdateBlocksRecursively(BlockEditorData blockEditorData, Func<Guid> createGuid)
    {
        base.UpdateBlocksRecursively(blockEditorData, createGuid);
    }
}
