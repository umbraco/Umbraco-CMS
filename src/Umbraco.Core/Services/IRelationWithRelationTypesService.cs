using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Services
{
    [Obsolete("This interface will be merged with IRelationService in Umbraco 11")]
    public interface IRelationWithRelationTypesService : IRelationService
    {
        /// <summary>
        /// Returns paged parent entities for a related child id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalChildren"></param>
        /// <param name="relationTypes">A list of relation types to filter</param>
        /// <returns>An enumerable list of <see cref="IUmbracoEntity"/></returns>
        IEnumerable<IUmbracoEntity> GetPagedParentEntitiesByChildId(int id, long pageIndex, int pageSize, out long totalChildren, string[] relationTypes, params UmbracoObjectTypes[] entityTypes);

        /// <summary>
        /// Returns paged parent entities for related child ids
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalChildren"></param>
        /// <param name="relationTypes">A list of relation types to filter</param>
        /// <returns>An enumerable list of <see cref="IUmbracoEntity"/></returns>
        IEnumerable<IUmbracoEntity> GetPagedParentEntitiesByChildIds(int[] ids, long pageIndex, int pageSize, out long totalChildren, string[] relationTypes, params UmbracoObjectTypes[] entityTypes);

        /// <summary>
        /// Returns paged child entities for a related parent id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalChildren"></param>
        /// <param name="relationTypes">A list of relation types to filter</param>
        /// <returns>An enumerable list of <see cref="IUmbracoEntity"/></returns>
        IEnumerable<IUmbracoEntity> GetPagedChildEntitiesByParentId(int id, long pageIndex, int pageSize, out long totalChildren, string[] relationTypes, params UmbracoObjectTypes[] entityTypes);

        /// <summary>
        /// Returns paged entities for only the items used in a relation
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalItems"></param>
        /// <param name="entityTypes"></param>
        /// <returns>An enumerable list of <see cref="IUmbracoEntity"/></returns>
        IEnumerable<IUmbracoEntity> GetPagedEntitiesForItemsInRelation(int[] ids, long pageIndex, int pageSize, out long totalItems, params UmbracoObjectTypes[] entityTypes);


        /// <summary>
        /// Returns paged entities for only the items used in a relation
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalItems"></param>
        /// <param name="entityTypes"></param>
        /// <returns>An enumerable list of <see cref="IUmbracoEntity"/></returns>
        PagedResult<RelationItem> GetPagedRelationsForItems(int[] ids, long pageIndex, int pageSize, bool filterMustBeIsDependency);


        PagedResult<RelationItem> GetPagedDescendantsInReferences(int parentId, long pageIndex, int pageSize, bool filterMustBeIsDependency);


        PagedResult<RelationItem> GetPagedItemsWithRelations(int[] ids, long pageIndex, int pageSize, bool filterMustBeIsDependency);
    }
}
