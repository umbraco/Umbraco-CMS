using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence.Repositories;

namespace Umbraco.Core.Persistence.Factories
{
    internal class UmbracoEntityFactory 
    {
        internal void AddAdditionalData(UmbracoEntity entity, IDictionary<string, object> originalEntityProperties)
        {
            var entityProps = typeof(IUmbracoEntity).GetPublicProperties().Select(x => x.Name).ToArray();
            
            //figure out what extra properties we have that are not on the IUmbracoEntity and add them to additional data
            foreach (var k in originalEntityProperties.Keys
                .Select(x => new { orig = x, title = x.ConvertCase(StringAliasCaseType.PascalCase) })
                .Where(x => entityProps.InvariantContains(x.title) == false))
            {
                entity.AdditionalData[k.title] = originalEntityProperties[k.orig];
            }
        }

        internal UmbracoEntity BuildEntityFromDynamic(dynamic d)
        {
            var asDictionary = (IDictionary<string, object>)d;

            var entity = new UmbracoEntity(d.trashed)
            {
                CreateDate = d.createDate,
                CreatorId = d.nodeUser,
                Id = d.id,
                Key = d.uniqueID,
                Level = d.level,
                Name = d.text,
                NodeObjectTypeId = d.nodeObjectType,
                ParentId = d.parentID,
                Path = d.path,
                SortOrder = d.sortOrder,
                HasChildren = d.children > 0,
                ContentTypeAlias = asDictionary.ContainsKey("alias") ? (d.alias ?? string.Empty) : string.Empty,
                ContentTypeIcon = asDictionary.ContainsKey("icon") ? (d.icon ?? string.Empty) : string.Empty,
                ContentTypeThumbnail = asDictionary.ContainsKey("thumbnail") ? (d.thumbnail ?? string.Empty) : string.Empty,
            };

            var publishedVersion = default(Guid);            
            //some content items don't have a published version
            if (asDictionary.ContainsKey("publishedVersion") && asDictionary["publishedVersion"] != null)
            {
                Guid.TryParse(d.publishedVersion.ToString(), out publishedVersion);    
            }
            var newestVersion = default(Guid);
            if (asDictionary.ContainsKey("newestVersion") && d.newestVersion != null)
            {
                Guid.TryParse(d.newestVersion.ToString(), out newestVersion);    
            }

            entity.IsPublished = publishedVersion != default(Guid) || (newestVersion != default(Guid) && publishedVersion == newestVersion);
            entity.IsDraft = newestVersion != default(Guid) && (publishedVersion == default(Guid) || publishedVersion != newestVersion);
            entity.HasPendingChanges = (publishedVersion != default(Guid) && newestVersion != default(Guid)) && publishedVersion != newestVersion;
            
            //Now we can assign the additional data!                        
            AddAdditionalData(entity, asDictionary);
            
            return entity;
        }

        public UmbracoEntity BuildEntity(EntityRepository.UmbracoEntityDto dto)
        {
            var entity = new UmbracoEntity(dto.Trashed)
                             {
                                 CreateDate = dto.CreateDate,
                                 CreatorId = dto.UserId.Value,
                                 Id = dto.NodeId,
                                 Key = dto.UniqueId,
                                 Level = dto.Level,
                                 Name = dto.Text,
                                 NodeObjectTypeId = dto.NodeObjectType.Value,
                                 ParentId = dto.ParentId,
                                 Path = dto.Path,
                                 SortOrder = dto.SortOrder,
                                 HasChildren = dto.Children > 0,
                                 ContentTypeAlias = dto.Alias ?? string.Empty,
                                 ContentTypeIcon = dto.Icon ?? string.Empty,
                                 ContentTypeThumbnail = dto.Thumbnail ?? string.Empty,
                             };

            entity.IsPublished = dto.PublishedVersion != default(Guid) || (dto.NewestVersion != default(Guid) && dto.PublishedVersion == dto.NewestVersion);
            entity.IsDraft = dto.NewestVersion != default(Guid) && (dto.PublishedVersion == default(Guid) || dto.PublishedVersion != dto.NewestVersion);
            entity.HasPendingChanges = (dto.PublishedVersion != default(Guid) && dto.NewestVersion != default(Guid)) && dto.PublishedVersion != dto.NewestVersion;

            if (dto.UmbracoPropertyDtos != null)
            {
                foreach (var propertyDto in dto.UmbracoPropertyDtos)
                {
                    entity.AdditionalData[propertyDto.PropertyAlias] = new UmbracoEntity.EntityProperty
                    {
                        PropertyEditorAlias = propertyDto.PropertyEditorAlias,
                        Value = propertyDto.NTextValue.IsNullOrWhiteSpace()
                            ? propertyDto.NVarcharValue
                            : propertyDto.NTextValue.ConvertToJsonIfPossible()
                    };
                }
            }

            return entity;
        }

        public EntityRepository.UmbracoEntityDto BuildDto(UmbracoEntity entity)
        {
            var node = new EntityRepository.UmbracoEntityDto
                           {
                               CreateDate = entity.CreateDate,
                               Level = short.Parse(entity.Level.ToString(CultureInfo.InvariantCulture)),
                               NodeId = entity.Id,
                               NodeObjectType = entity.NodeObjectTypeId,
                               ParentId = entity.ParentId,
                               Path = entity.Path,
                               SortOrder = entity.SortOrder,
                               Text = entity.Name,
                               Trashed = entity.Trashed,
                               UniqueId = entity.Key,
                               UserId = entity.CreatorId
                           };
            return node;
        }
    }
}