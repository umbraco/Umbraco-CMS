using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class PublicAccessEntryFactory
    {
        public PublicAccessEntry BuildEntity(AccessDto dto)
        {
            var entity = new PublicAccessEntry(dto.Id, dto.NodeId, dto.LoginNodeId, dto.NoAccessNodeId, 
                dto.Rules.Select(x => new PublicAccessRule(x.Id, x.AccessId)
                {
                    RuleValue = x.RuleValue,
                    RuleType = x.RuleType,
                    CreateDate = x.CreateDate,
                    UpdateDate = x.UpdateDate
                }))
            {
                CreateDate = dto.CreateDate,
                UpdateDate = dto.UpdateDate
            };

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            entity.ResetDirtyProperties(false);
            return entity;
        }

        public AccessDto BuildDto(PublicAccessEntry entity)
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
                    UpdateDate = x.UpdateDate
                }).ToList()
            };

            return dto;
        }
    }
}