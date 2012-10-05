using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class ContentFactory
    {
        internal static Content CreateContent(int key, IContentType contentType, DocumentDto documentDto, IEnumerable<PropertyDataDto> propertyDataDtos)
        {
            var properties = new List<Property>();
            foreach (var dto in propertyDataDtos)
            {
                var propertyType =
                    contentType.CompositionPropertyTypes.FirstOrDefault(x => x.Id == dto.PropertyTypeId);
                properties.Add(propertyType.CreatePropertyFromRawValue(dto.GetValue));
            }

            return new Content(documentDto.ContentVersionDto.ContentDto.NodeDto.ParentId, contentType)
            {
                Id = key,
                Key = documentDto.ContentVersionDto.ContentDto.NodeDto.UniqueId.HasValue ? documentDto.ContentVersionDto.ContentDto.NodeDto.UniqueId.Value : key.ToGuid(),
                Name = documentDto.ContentVersionDto.ContentDto.NodeDto.Text,
                Path = documentDto.ContentVersionDto.ContentDto.NodeDto.Path,
                UserId = documentDto.ContentVersionDto.ContentDto.NodeDto.UserId.HasValue ? documentDto.ContentVersionDto.ContentDto.NodeDto.UserId.Value : documentDto.UserId,
                Level = documentDto.ContentVersionDto.ContentDto.NodeDto.Level,
                ParentId = documentDto.ContentVersionDto.ContentDto.NodeDto.ParentId,
                SortOrder = documentDto.ContentVersionDto.ContentDto.NodeDto.SortOrder,
                Trashed = documentDto.ContentVersionDto.ContentDto.NodeDto.Trashed,
                Published = documentDto.Published,
                CreateDate = documentDto.ContentVersionDto.ContentDto.NodeDto.CreateDate,
                UpdateDate = documentDto.ContentVersionDto.VersionDate,
                ExpireDate = documentDto.ExpiresDate,
                ReleaseDate = documentDto.ReleaseDate,
                Version = documentDto.ContentVersionDto.VersionId,
                Properties = new PropertyCollection(properties)
            };
        }

        internal static NodeDto CreateNodeDto(IContent entity, string nodeObjectType)
        {
            var nodeDto = new NodeDto
            {
                CreateDate = entity.CreateDate,
                NodeId = entity.Id,
                Level = short.Parse(entity.Level.ToString(CultureInfo.InvariantCulture)),
                NodeObjectType = new Guid(nodeObjectType),
                ParentId = entity.ParentId,
                Path = entity.Path,
                SortOrder = entity.SortOrder,
                Text = entity.Name,
                Trashed = entity.Trashed,
                UniqueId = entity.Key,
                UserId = entity.UserId
            };

            return nodeDto;
        }

        internal static NodeDto CreateNodeDto(IContent entity, string nodeObjectType, string path, int level, int sortOrder)
        {
            var nodeDto = new NodeDto
            {
                CreateDate = entity.CreateDate,
                NodeId = entity.Id,
                Level = short.Parse(level.ToString(CultureInfo.InvariantCulture)),
                NodeObjectType = new Guid(nodeObjectType),
                ParentId = entity.ParentId,
                Path = path,
                SortOrder = sortOrder,
                Text = entity.Name,
                Trashed = entity.Trashed,
                UniqueId = entity.Key,
                UserId = entity.UserId
            };

            return nodeDto;
        }

        internal static ContentDto CreateContentDto(IContent entity, int primaryKey = 0)
        {
            var contentDto = new ContentDto
            {
                NodeId = entity.Id,
                ContentType = entity.ContentTypeId
            };

            if (primaryKey > 0)
            {
                contentDto.PrimaryKey = primaryKey;
            }

            return contentDto;
        }

        internal static ContentVersionDto CreateContentVersionDto(IContent entity)
        {
            var contentVersionDto = new ContentVersionDto
            {
                NodeId = entity.Id,
                VersionDate = entity.UpdateDate,
                VersionId = entity.Version
            };
            return contentVersionDto;
        }

        internal static DocumentDto CreateDocumentDto(IContent entity)
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
                UserId = entity.UserId,
                VersionId = entity.Version
            };
            return documentDto;
        }

        internal static List<PropertyDataDto> CreateProperties(int id, Guid version, IEnumerable<Property> properties)
        {
            var propertyDataDtos = new List<PropertyDataDto>();
            /*var serviceStackSerializer = new ServiceStackXmlSerializer();
            var service = new SerializationService(serviceStackSerializer);*/

            foreach (var property in properties)
            {
                var dto = new PropertyDataDto { NodeId = id, PropertyTypeId = property.PropertyTypeId, VersionId = version };
                /*if (property.Value is IEditorModel)
                {
                    var result = service.ToStream(property.Value);
                    dto.Text = result.ResultStream.ToJsonString();
                }*/
                if (property.Value is int)
                {
                    dto.Integer = int.Parse(property.Value.ToString());
                }
                else if (property.Value is DateTime)
                {
                    dto.Date = DateTime.Parse(property.Value.ToString());
                }
                else if (property.Value is string)
                {
                    dto.Text = property.Value.ToString();
                }
                else if (property.Value != null)
                {
                    dto.VarChar = property.Value.ToString();//TODO Check how/when NVarChar is actually set/used
                }

                propertyDataDtos.Add(dto);
            }
            return propertyDataDtos;
        }
    }
}