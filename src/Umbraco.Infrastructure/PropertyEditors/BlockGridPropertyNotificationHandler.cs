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
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Core.PropertyEditors.BlockGridPropertyNotificationHandler"/> class,
    /// providing a logger for handling property notifications related to block grid properties.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger{BlockGridPropertyNotificationHandler}"/> used for logging notification handling events.</param>
    public BlockGridPropertyNotificationHandler(ILogger<BlockGridPropertyNotificationHandler> logger)
        : base(logger)
    {
    }

    protected override string EditorAlias => Constants.PropertyEditors.Aliases.BlockGrid;
}
