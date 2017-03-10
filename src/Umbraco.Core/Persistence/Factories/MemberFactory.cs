﻿using System;
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

        public static IMember BuildEntity(MemberDto dto, IMemberType contentType)
        {
            var member = new Member(
                dto.ContentVersionDto.ContentDto.NodeDto.Text,
                dto.Email, dto.LoginName, dto.Password, contentType);

            try
            {
                member.DisableChangeTracking();

                member.Id = dto.NodeId;
                member.Key = dto.ContentVersionDto.ContentDto.NodeDto.UniqueId;
                member.Path = dto.ContentVersionDto.ContentDto.NodeDto.Path;
                member.CreatorId = dto.ContentVersionDto.ContentDto.NodeDto.UserId.Value;
                member.Level = dto.ContentVersionDto.ContentDto.NodeDto.Level;
                member.ParentId = dto.ContentVersionDto.ContentDto.NodeDto.ParentId;
                member.SortOrder = dto.ContentVersionDto.ContentDto.NodeDto.SortOrder;
                member.Trashed = dto.ContentVersionDto.ContentDto.NodeDto.Trashed;
                member.CreateDate = dto.ContentVersionDto.ContentDto.NodeDto.CreateDate;
                member.UpdateDate = dto.ContentVersionDto.VersionDate;
                member.Version = dto.ContentVersionDto.VersionId;

                member.ProviderUserKey = member.Key;
                //on initial construction we don't want to have dirty properties tracked
                // http://issues.umbraco.org/issue/U4-1946
                member.ResetDirtyProperties(false);
                return member;
            }
            finally
            {
                member.EnableChangeTracking();
            }
        }

        [Obsolete("Use the static BuildEntity instead so we don't have to allocate one of these objects everytime we want to map values")]
        public IMember BuildEntity(MemberDto dto)
        {
            return BuildEntity(dto, _contentType);
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