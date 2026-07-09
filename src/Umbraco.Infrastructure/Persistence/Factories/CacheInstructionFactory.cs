using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class CacheInstructionFactory
{
    /// <summary>
    /// Builds a collection of <see cref="CacheInstruction"/> entities from the given collection of <see cref="CacheInstructionDto"/> data transfer objects.
    /// </summary>
    /// <param name="dtos">The collection of <see cref="CacheInstructionDto"/> objects to convert.</param>
    /// <returns>An enumerable collection of <see cref="CacheInstruction"/> entities.</returns>
    public static IEnumerable<CacheInstruction> BuildEntities(IEnumerable<CacheInstructionDto> dtos) =>
        dtos.Select(BuildEntity).ToList();

    /// <summary>
    /// Builds a <see cref="CacheInstruction"/> entity from the given <see cref="CacheInstructionDto"/>.
    /// </summary>
    /// <param name="dto">The data transfer object containing cache instruction data.</param>
    /// <returns>A <see cref="CacheInstruction"/> entity constructed from the DTO.</returns>
    public static CacheInstruction BuildEntity(CacheInstructionDto dto) =>
        new(dto.Id, dto.UtcStamp.EnsureUtc(), dto.Instructions, dto.OriginIdentity, dto.InstructionCount);

    /// <summary>
    /// Builds a <see cref="CacheInstructionDto"/> from the given <see cref="CacheInstruction"/> entity.
    /// </summary>
    /// <param name="entity">The <see cref="CacheInstruction"/> entity to convert.</param>
    /// <returns>A <see cref="CacheInstructionDto"/> representing the given entity.</returns>
    public static CacheInstructionDto BuildDto(CacheInstruction entity) =>
        new()
        {
            Id = entity.Id,
            UtcStamp = entity.UtcStamp,
            Instructions = entity.Instructions,
            OriginIdentity = entity.OriginIdentity,
            InstructionCount = entity.InstructionCount,
        };
}
