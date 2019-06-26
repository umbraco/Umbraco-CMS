using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Strings;

namespace Umbraco.Core.Persistence.Factories
{
    internal class UmbracoEntityFactory 
    {
        private static readonly Lazy<string[]> EntityProperties = new Lazy<string[]>(() => typeof(IUmbracoEntity).GetPublicProperties().Select(x => x.Name).ToArray());

        /// <summary>
        /// Figure out what extra properties we have that are not on the IUmbracoEntity and add them to additional data
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="originalEntityProperties"></param>
        internal void AddAdditionalData(UmbracoEntity entity, IDictionary<string, object> originalEntityProperties)
        {   
            foreach (var k in originalEntityProperties.Keys
                .Select(x => new { orig = x, title = x.ToCleanString(CleanStringType.PascalCase | CleanStringType.Ascii | CleanStringType.ConvertCase) })
                .Where(x => EntityProperties.Value.InvariantContains(x.title) == false))
            {
                entity.AdditionalData[k.title] = originalEntityProperties[k.orig];
            }
        }

        internal UmbracoEntity BuildEntityFromDynamic(dynamic d)
        {
            var asDictionary = (IDictionary<string, object>)d;

            var entity = new UmbracoEntity(d.trashed);

            try
            {
                entity.DisableChangeTracking();

                entity.CreateDate = d.createDate;
                entity.CreatorId = d.nodeUser == null ? 0 : d.nodeUser;
                entity.Id = d.id;
                entity.Key = d.uniqueID;
                entity.Level = d.level;
                entity.Name = d.text;
                entity.NodeObjectTypeId = d.nodeObjectType;
                entity.ParentId = d.parentID;
                entity.Path = d.path;
                entity.SortOrder = d.sortOrder;
                entity.HasChildren = d.children > 0;
                entity.ContentTypeAlias = asDictionary.ContainsKey("alias") ? (d.alias ?? string.Empty) : string.Empty;
                entity.ContentTypeIcon = asDictionary.ContainsKey("icon") ? (d.icon ?? string.Empty) : string.Empty;
                entity.ContentTypeThumbnail = asDictionary.ContainsKey("thumbnail") ? (d.thumbnail ?? string.Empty) : string.Empty;

                var publishedVersion = default(Guid);
                //some content items don't have a published/newest version
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
            finally
            {
                entity.EnableChangeTracking();
            }
        }
        
    }
}
