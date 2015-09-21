using System;
using System.Globalization;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class MemberFactory
    {
        private readonly IMemberType _contentType;
        private readonly Guid _nodeObjectTypeId;
        private readonly int _id;
        private int _primaryKey;

        public MemberFactory(IMemberType contentType, Guid nodeObjectTypeId, int id)
        {
            _contentType = contentType;
            _nodeObjectTypeId = nodeObjectTypeId;
            _id = id;
        }

        public MemberFactory(Guid nodeObjectTypeId, int id)
        {
            _nodeObjectTypeId = nodeObjectTypeId;
            _id = id;
        }

        #region Implementation of IEntityFactory<IMedia,ContentVersionDto>

        public IMember BuildEntity(MemberDto dto)
        {
            var member = new Member(
                dto.ContentVersionDto.ContentDto.NodeDto.Text, 
                dto.Email,dto.LoginName,dto.Password, _contentType)
            {
                Id = _id,
                Key = dto.ContentVersionDto.ContentDto.NodeDto.UniqueId,
                Path = dto.ContentVersionDto.ContentDto.NodeDto.Path,
                CreatorId = dto.ContentVersionDto.ContentDto.NodeDto.UserId.Value,
                Level = dto.ContentVersionDto.ContentDto.NodeDto.Level,
                ParentId = dto.ContentVersionDto.ContentDto.NodeDto.ParentId,
                SortOrder = dto.ContentVersionDto.ContentDto.NodeDto.SortOrder,
                Trashed = dto.ContentVersionDto.ContentDto.NodeDto.Trashed,
                CreateDate = dto.ContentVersionDto.ContentDto.NodeDto.CreateDate,
                UpdateDate = dto.ContentVersionDto.VersionDate,
                Version = dto.ContentVersionDto.VersionId
            };
            member.ProviderUserKey = member.Key;
            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            member.ResetDirtyProperties(false);
            return member;
        }

        public MemberDto BuildDto(IMember entity)
        {
            var dto = new MemberDto
            {
                ContentVersionDto = new ContentVersionDto
                {
                    NodeId = entity.Id,
                    VersionDate = entity.UpdateDate,
                    VersionId = entity.Version,
                    ContentDto = BuildContentDto(entity)
                },
                Email = entity.Email,
                LoginName = entity.Username,
                NodeId = entity.Id,
                Password = entity.RawPasswordValue
            };
            return dto;
        }

        #endregion

        public void SetPrimaryKey(int primaryKey)
        {
            _primaryKey = primaryKey;
        }

        private ContentDto BuildContentDto(IMember entity)
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

        private NodeDto BuildNodeDto(IMember entity)
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