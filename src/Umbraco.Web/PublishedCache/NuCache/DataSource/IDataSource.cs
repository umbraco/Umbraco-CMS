using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Scoping;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    /// <summary>
    /// Defines a data source for NuCache.
    /// </summary>
    public interface IDataSource
    {
        //TODO: For these required sort orders, would sorting on Path 'just work'?

        ContentNodeKit GetContentSource(IScope scope, int id);

        /// <summary>
        /// Returns all content ordered by level + sortOrder
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        /// <remarks>
        /// MUST be ordered by level + parentId + sortOrder!
        /// </remarks>
        IEnumerable<ContentNodeKit> GetAllContentSources(IScope scope);

        /// <summary>
        /// Returns branch for content ordered by level + sortOrder
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        /// <remarks>
        /// MUST be ordered by level + parentId + sortOrder!
        /// </remarks>
        IEnumerable<ContentNodeKit> GetBranchContentSources(IScope scope, int id);

        /// <summary>
        /// Returns content by Ids ordered by level + sortOrder
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        /// <remarks>
        /// MUST be ordered by level + parentId + sortOrder!
        /// </remarks>
        IEnumerable<ContentNodeKit> GetTypeContentSources(IScope scope, IEnumerable<int> ids);

        ContentNodeKit GetMediaSource(IScope scope, int id);

        /// <summary>
        /// Returns all media ordered by level + sortOrder
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        /// <remarks>
        /// MUST be ordered by level + parentId + sortOrder!
        /// </remarks>
        IEnumerable<ContentNodeKit> GetAllMediaSources(IScope scope);

        /// <summary>
        /// Returns branch for media ordered by level + sortOrder
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        /// <remarks>
        /// MUST be ordered by level + parentId + sortOrder!
        /// </remarks>
        IEnumerable<ContentNodeKit> GetBranchMediaSources(IScope scope, int id); // must order by level, sortOrder

        /// <summary>
        /// Returns media by Ids ordered by level + sortOrder
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        /// <remarks>
        /// MUST be ordered by level + parentId + sortOrder!
        /// </remarks>
        IEnumerable<ContentNodeKit> GetTypeMediaSources(IScope scope, IEnumerable<int> ids);

        /// <summary>
        /// Removes all versions of an entity from the cache (both published and current
        /// </summary>
        /// <param name="id"></param>
        void RemoveEntity(IScope scope, int id);

        /// <summary>
        /// Removes published version of an entity from the cache
        /// </summary>
        /// <param name="id"></param>
        void RemovePublishedEntity(IScope scope, int id);

        /// <summary>
        /// Deletes all entities, optionally restricted to only specified content types ids
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="contentTypeIds"></param>
        void DeleteAllContentEntities(IScope scope, IEnumerable<int> contentTypeIds = null);

        /// <summary>
        /// Deletes all entities, optionally restricted to only specified content types ids
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="contentTypeIds"></param>
        void DeleteAllMediaEntities(IScope scope,  IEnumerable<int> contentTypeIds = null);

        /// <summary>
        /// Deletes all entities, optionally restricted to only specified content types ids
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="contentTypeIds"></param>
        void DeleteAllMemberEntities(IScope scope,  IEnumerable<int> contentTypeIds = null);

        /// <summary>
        /// Inserts or updates an entity
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="contentBase"></param>
        /// <param name="published"></param>
        /// <param name="serializer"></param>
        void UpsertContentEntity(IScope scope, IContentBase contentBase, bool published);
        /// <summary>
        /// Inserts or updates an entity
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="contentBase"></param>
        /// <param name="published"></param>
        /// <param name="serializer"></param>
        void UpsertMediaEntity(IScope scope, IContentBase contentBase, bool published);
        /// <summary>
        /// Inserts or updates an entity
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="contentBase"></param>
        /// <param name="published"></param>
        /// <param name="serializer"></param>
        void UpsertMemberEntity(IScope scope, IContentBase contentBase, bool published);

        void LoadAllContentEntities(IScope scope, IEnumerable<int> contentTypeIds = null);

        void LoadAllMediaEntities(IScope scope,  IEnumerable<int> contentTypeIds = null);

        void LoadAllMemberEntities(IScope scope, IEnumerable<int> contentTypeIds = null);

        bool MemberEntitiesValid(IScope scope);
        bool ContentEntitiesValid(IScope scope);
        bool MediaEntitiesValid(IScope scope);
    }
}
