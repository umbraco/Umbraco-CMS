using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories
{
    internal static class CacheInstructionFactory
    {
        public static IEnumerable<CacheInstruction> BuildEntities(IEnumerable<CacheInstructionDto> dtos) => dtos.Select(BuildEntity).ToList();

        public static CacheInstruction BuildEntity(CacheInstructionDto dto) =>
            new CacheInstruction(dto.Id, dto.UtcStamp, dto.Instructions, dto.OriginIdentity, dto.InstructionCount);

        public static CacheInstructionDto BuildDto(CacheInstruction entity) =>
            new CacheInstructionDto
            {
                Id = entity.Id,
                UtcStamp = entity.UtcStamp,
                Instructions = entity.Instructions,
                OriginIdentity = entity.OriginIdentity,
                InstructionCount = entity.InstructionCount,
            };
    }
}
