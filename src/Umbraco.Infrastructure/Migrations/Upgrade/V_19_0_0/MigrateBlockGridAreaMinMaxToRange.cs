// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json.Nodes;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_19_0_0;

/// <summary>
///     Converts the Block Grid's per-area <c>minAllowed</c>/<c>maxAllowed</c> configuration into a single
///     <c>validationLimit</c> range. The bounds are preserved as-is (they are already optional, so no zero handling).
/// </summary>
internal sealed class MigrateBlockGridAreaMinMaxToRange : MigrateMinMaxToRangeMigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MigrateBlockGridAreaMinMaxToRange"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public MigrateBlockGridAreaMinMaxToRange(IMigrationContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    protected override string EditorAlias => Constants.PropertyEditors.Aliases.BlockGrid;

    /// <inheritdoc />
    protected override bool TryMigrateConfiguration(JsonObject configuration)
        => MigrateAreas(configuration);

    /// <summary>
    ///     Walks the <c>blocks[].areas[]</c> structure, converting each area's <c>minAllowed</c>/<c>maxAllowed</c>
    ///     into a single <c>validationLimit</c> range.
    /// </summary>
    /// <param name="configuration">The Block Grid configuration to migrate in place.</param>
    /// <returns><c>true</c> when at least one area was changed.</returns>
    internal static bool MigrateAreas(JsonObject configuration)
    {
        var blocksKey = FindKey(configuration, "blocks");
        if (blocksKey is null || configuration[blocksKey] is not JsonArray blocks)
        {
            return false;
        }

        var changed = false;
        foreach (JsonNode? blockNode in blocks)
        {
            if (blockNode is JsonObject block)
            {
                changed |= MigrateBlockAreas(block);
            }
        }

        return changed;
    }

    private static bool MigrateBlockAreas(JsonObject block)
    {
        var areasKey = FindKey(block, "areas");
        if (areasKey is null || block[areasKey] is not JsonArray areas)
        {
            return false;
        }

        var changed = false;
        foreach (JsonNode? areaNode in areas)
        {
            if (areaNode is JsonObject area)
            {
                changed |= MigrateArea(area);
            }
        }

        return changed;
    }

    private static bool MigrateArea(JsonObject area)
    {
        var minName = FindKey(area, "minAllowed");
        var maxName = FindKey(area, "maxAllowed");
        if (minName is null && maxName is null)
        {
            return false;
        }

        decimal? min = minName is not null ? ToBound(area[minName], zeroIsUnbounded: false) : null;
        decimal? max = maxName is not null ? ToBound(area[maxName], zeroIsUnbounded: false) : null;

        if (minName is not null)
        {
            area.Remove(minName);
        }

        if (maxName is not null)
        {
            area.Remove(maxName);
        }

        SetRange(area, "validationLimit", min, max);
        return true;
    }
}
