using System;
using System.Globalization;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class ContentFactory : IEntityFactory<IContent, DocumentDto>
    {
        private readonly IContentType _contentType;
        private readonly Guid _nodeObjectTypeId;
        private readonly int _id;
        private int _primaryKey;
        private readonly IProfile _user;
        private readonly IProfile _writer;

        public ContentFactory(IContentType contentType, Guid nodeObjectTypeId, int id, IProfile user, IProfile writer)
        {
            _contentType = contentType;
            _nodeObjectTypeId = nodeObjectTypeId;
            _id = id;
            _user = user;
            _writer = writer;
        }

        public ContentFactory(Guid nodeObjectTypeId, int id)
        {
            _nodeObjectTypeId = nodeObjectTypeId;
            _id = id;
        }

        #region Implementation of IEntityFactory<IContent,DocumentDto>

        public IContent BuildEntity(DocumentDto dto)
        {
            return new Content(dto.ContentVersionDto.ContentDto.NodeDto.ParentId, _contentType)
                       {
                           Id = _id,
                           Key =
                               dto.ContentVersionDto.ContentDto.NodeDto.UniqueId.HasValue
                                   ? dto.ContentVersionDto.ContentDto.NodeDto.UniqueId.Value
                                   : _id.ToGuid(),
                           Name = dto.ContentVersionDto.ContentDto.NodeDto.Text,
                           Path = dto.ContentVersionDto.ContentDto.NodeDto.Path,
                           Creator = _user,
                           Writer = _writer,
                           Level = dto.ContentVersionDto.ContentDto.NodeDto.Level,
                           ParentId = dto.ContentVersionDto.ContentDto.NodeDto.ParentId,
                           SortOrder = dto.ContentVersionDto.ContentDto.NodeDto.SortOrder,
                           Trashed = dto.ContentVersionDto.ContentDto.NodeDto.Trashed,
                           Published = dto.Published,
                           CreateDate = dto.ContentVersionDto.ContentDto.NodeDto.CreateDate,
                           UpdateDate = dto.ContentVersionDto.VersionDate,
                           ExpireDate = dto.ExpiresDate,
                           ReleaseDate = dto.ReleaseDate,
                           Version = dto.ContentVersionDto.VersionId
                       };
        }

        public DocumentDto BuildDto(IContent entity)
        {
            //NOTE Currently doesn't add Alias and templateId (legacy stuff that eventually will go away)
            var documentDto = new DocumentDto
                                  {
                                      ExpiresDate = entity.ExpireDate,
                                      Newest = true,
                                      NodeId = entity.Id,
                                      Published = entity.Published,
                                      ReleaseDate = entity.ReleaseDate,
                                      Text = entity.Name,
                                      UpdateDate = entity.UpdateDate,
                                      WriterUserId = entity.Writer.Id.SafeCast<int>(),
                                      VersionId = entity.Version,
                                      ContentVersionDto = BuildContentVersionDto(entity)
                                  };
            return documentDto;
        }

        #endregion

        public void SetPrimaryKey(int primaryKey)
        {
            _primaryKey = primaryKey;
        }

        private ContentVersionDto BuildContentVersionDto(IContent entity)
        {
            var contentVersionDto = new ContentVersionDto
                                        {
                                            NodeId = entity.Id,
                                            VersionDate = entity.UpdateDate,
                                            VersionId = entity.Version,
                                            ContentDto = BuildContentDto(entity)
                                        };
            return contentVersionDto;
        }

        private ContentDto BuildContentDto(IContent entity)
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

        private NodeDto BuildNodeDto(IContent entity)
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
                                  UserId = entity.Creator.Id.SafeCast<int>()
                              };

            return nodeDto;
        }
    }
}