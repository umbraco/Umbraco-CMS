using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class PublicAccessEntryFactory
{
    public static PublicAccessEntry BuildEntity(AccessDto dto)
    {
        var entity = new PublicAccessEntry(
                dto.Id,
                dto.NodeId,
                dto.LoginNodeId,
                dto.NoAccessNodeId,
                dto.Rules.Select(x => new PublicAccessRule(x.Id, x.AccessId)
            {
                RuleValue = x.RuleValue,
                RuleType = x.RuleType,
                CreateDate = x.CreateDate,
                UpdateDate = x.UpdateDate,
            }))
        { CreateDate = dto.CreateDate, UpdateDate = dto.UpdateDate };

        // reset dirty initial properties (U4-1946)
        entity.ResetDirtyProperties(false);
        return entity;
    }

    public static AccessDto BuildDto(PublicAccessEntry entity)
    {
        var dto = new AccessDto
        {
            Id = entity.Key,
            NoAccessNodeId = entity.NoAccessNodeId,
            LoginNodeId = entity.LoginNodeId,
            NodeId = entity.ProtectedNodeId,
            CreateDate = entity.CreateDate,
            UpdateDate = entity.UpdateDate,
            Rules = entity.Rules.Select(x => new AccessRuleDto
            {
                AccessId = x.AccessEntryId,
                Id = x.Key,
                RuleValue = x.RuleValue,
                RuleType = x.RuleType,
                CreateDate = x.CreateDate,
                UpdateDate = x.UpdateDate,
            }).ToList(),
        };

        return dto;
    }
}
