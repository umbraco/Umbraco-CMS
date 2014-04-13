using System;
using System.Globalization;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class MemberFactory : IEntityFactory<IMember, MemberDto>
    {
        private readonly Guid _nodeObjectTypeId;
        private readonly int _id;
        private int _primaryKey;

        public MemberFactory(Guid nodeObjectTypeId, int id)
        {
            _nodeObjectTypeId = nodeObjectTypeId;
            _id = id;
        }

        public IMember BuildEntity(MemberDto dto)
        {
            throw new System.NotImplementedException();
        }

        public MemberDto BuildDto(IMember entity)
        {
            var member = new MemberDto
                         {
                             NodeId = entity.Id,
                             Email = entity.Email,
                             LoginName = entity.Username,
                             Password = entity.RawPasswordValue,
                             ContentVersionDto = BuildDto(entity as Member)
                         };
            return member;
        }

        public void SetPrimaryKey(int primaryKey)
        {
            _primaryKey = primaryKey;
        }

        private ContentVersionDto BuildDto(Member entity)
        {
            var dto = new ContentVersionDto
                      {
                          NodeId = entity.Id,
                          VersionDate = entity.UpdateDate,
                          VersionId = entity.Version,
                          ContentDto = BuildContentDto(entity)
                      };
            return dto;
        }

        private ContentDto BuildContentDto(Member entity)
        {
            var contentDto = new ContentDto
            {
                NodeId = entity.Id,
                ContentTypeId = entity.ContentTypeId,
                NodeDto = BuildNodeDto(entity)
            };

            if (_primaryKey > 0)
            {
                contentDto.PrimaryKey = _primaryKey;
            }

            return contentDto;
        }

        private NodeDto BuildNodeDto(IUmbracoEntity entity)
        {
            var nodeDto = new NodeDto
                          {
                              CreateDate = entity.CreateDate,
                              NodeId = entity.Id,
                              Level = short.Parse(entity.Level.ToString(CultureInfo.InvariantCulture)),
                              NodeObjectType = _nodeObjectTypeId,
                              ParentId = entity.ParentId,
                              Path = entity.Path,
                              SortOrder = entity.SortOrder,
                              Text = entity.Name,
                              Trashed = entity.Trashed,
                              UniqueId = entity.Key,
                              UserId = entity.CreatorId
                          };

            return nodeDto;
        }
    }
}