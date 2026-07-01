using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class PublicAccessEntryFactory
{
    /// <summary>
    /// Creates and returns a <see cref="Umbraco.Cms.Core.Models.PublicAccessEntry"/> entity populated with data from the specified <see cref="Umbraco.Cms.Infrastructure.Persistence.Dtos.AccessDto"/>.
    /// </summary>
    /// <param name="dto">The <see cref="Umbraco.Cms.Infrastructure.Persistence.Dtos.AccessDto"/> containing the access entry data and associated rules.</param>
    /// <returns>A <see cref="Umbraco.Cms.Core.Models.PublicAccessEntry"/> entity initialized with values from the provided DTO.</returns>
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
                CreateDate = x.CreateDate.EnsureUtc(),
                UpdateDate = x.UpdateDate.EnsureUtc(),
            }))
        { CreateDate = dto.CreateDate.EnsureUtc(), UpdateDate = dto.UpdateDate.EnsureUtc() };

        // reset dirty initial properties (U4-1946)
        entity.ResetDirtyProperties(false);
        return entity;
    }

    /// <summary>
    /// Builds an <see cref="AccessDto"/> from the given <see cref="PublicAccessEntry"/> entity.
    /// </summary>
    /// <param name="entity">The <see cref="PublicAccessEntry"/> entity to convert.</param>
    /// <returns>An <see cref="AccessDto"/> representing the data from the entity.</returns>
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
