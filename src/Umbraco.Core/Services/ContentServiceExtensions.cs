using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Content service extension methods
    /// </summary>
    public static class ContentServiceExtensions
    {
        public static IEnumerable<IContent> GetByIds(this IContentService contentService, IEnumerable<Udi> ids)
        {
            var guids = new List<GuidUdi>();
            foreach (var udi in ids)
            {
                var guidUdi = udi as GuidUdi;
                if (guidUdi == null)
                    throw new InvalidOperationException("The UDI provided isn't of type " + typeof(GuidUdi) + " which is required by content");
                guids.Add(guidUdi);
            }

            return contentService.GetByIds(guids.Select(x => x.Guid));
        }

        /// <summary>
        /// Method to create an IContent object based on the Udi of a parent
        /// </summary>
        /// <param name="contentService"></param>
        /// <param name="name"></param>
        /// <param name="parentId"></param>
        /// <param name="mediaTypeAlias"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static IContent CreateContent(this IContentService contentService, string name, Udi parentId, string mediaTypeAlias, int userId = 0)
        {
            var guidUdi = parentId as GuidUdi;
            if (guidUdi == null)
                throw new InvalidOperationException("The UDI provided isn't of type " + typeof(GuidUdi) + " which is required by content");
            var parent = contentService.GetById(guidUdi.Guid);
            return contentService.CreateContent(name, parent, mediaTypeAlias, userId);
        }

        /// <summary>
        /// Remove all permissions for this user for all nodes
        /// </summary>
        /// <param name="contentService"></param>
        /// <param name="contentId"></param>
        public static void RemoveContentPermissions(this IContentService contentService, int contentId)
        {
            contentService.ReplaceContentPermissions(new EntityPermissionSet(contentId, new EntityPermissionCollection()));
        }

        /// <summary>
        /// Returns true if there is any content in the recycle bin
        /// </summary>
        /// <param name="contentService"></param>
        /// <returns></returns>
        public static bool RecycleBinSmells(this IContentService contentService)
        {
            return contentService.CountChildren(Constants.System.RecycleBinContent) > 0;
        }

        /// <summary>
        /// Returns true if there is any media in the recycle bin
        /// </summary>
        /// <param name="mediaService"></param>
        /// <returns></returns>
        public static bool RecycleBinSmells(this IMediaService mediaService)
        {
            return mediaService.CountChildren(Constants.System.RecycleBinMedia) > 0;
        }

        /// <summary>
        /// Used for the DeleteContentOfType(s) methods to find content items to be deleted based on the content type ids passed in
        /// </summary>
        /// <typeparam name="TContent"></typeparam>
        /// <param name="contentService"></param>
        /// <param name="contentTypeIds">The content type ids being deleted</param>
        /// <param name="repository"></param>
        /// <param name="rootItems">
        /// Returns a dictionary (path, TContent) of the root items discovered in the data set of items to be deleted, this can then be used
        /// to search for content that needs to be trashed as a result of this.
        /// </param>
        /// <returns>
        /// The content items to be deleted
        /// </returns>
        /// <remarks>
        /// An internal extension method used for the DeleteContentOfTypes (DeleteMediaOfTypes) methods so that logic can be shared to avoid code duplication.
        /// </remarks>
        internal static IEnumerable<TContent> TrackDeletionsForDeleteContentOfTypes<TContent>(this IContentServiceBase contentService,
            IEnumerable<int> contentTypeIds, 
            IRepositoryVersionable<int, TContent> repository,
            out IDictionary<string, TContent> rootItems)
            where TContent: IContentBase
        {
            var contentToDelete = new List<TContent>();

            //track the 'root' items of the collection of nodes discovered to delete, we need to use
            //these items to lookup descendants that are not of this doc type so they can be transfered
            //to the recycle bin
            rootItems = new Dictionary<string, TContent>();

            var query = Query<TContent>.Builder.Where(x => contentTypeIds.Contains(x.ContentTypeId));

            //TODO: What about content that has the contenttype as part of its composition?

            long pageIndex = 0;
            const int pageSize = 10000;
            int currentPageSize;
            do
            {
                long total;

                //start at the highest level
                var contents = repository.GetPagedResultsByQuery(query, pageIndex, pageSize, out total, "umbracoNode.level", Direction.Ascending, true).ToArray();

                // need to store decendants count before filtering, in order for loop to work correctly
                currentPageSize = contents.Length;

                //loop through the items, check if if the item exists already in the hierarchy of items tracked
                //and if not, we need to add it as a 'root' item to be used to lookup later
                foreach (var content in contents)
                {
                    var pathParts = content.Path.Split(',');
                    var found = false;

                    for (int i = 1; i < pathParts.Length; i++)
                    {
                        var currPath = "-1," + string.Join(",", Enumerable.Range(1, i).Select(x => pathParts[x]));
                        if (rootItems.Keys.Contains(currPath))
                        {
                            //this content item's ancestor already exists in the root collection
                            found = true;
                            break;
                        }
                    }

                    if (found == false)
                    {
                        rootItems[content.Path] = content;
                    }

                    //track content for deletion
                    contentToDelete.Add(content);
                }

                pageIndex++;
            } while (currentPageSize == pageSize);

            return contentToDelete;
        }

        /// <summary>
        /// Used for the DeleteContentOfType(s) methods to find content items to be trashed based on the content type ids passed in
        /// </summary>
        /// <typeparam name="TContent"></typeparam>
        /// <param name="contentService"></param>
        /// <param name="contentTypeIds">The content type ids being deleted</param>
        /// <param name="rootItems"></param>
        /// <param name="repository"></param>
        /// <returns>
        /// The content items to be trashed
        /// </returns>
        /// <remarks>
        /// An internal extension method used for the DeleteContentOfTypes (DeleteMediaOfTypes) methods so that logic can be shared to avoid code duplication.
        /// </remarks>
        internal static IEnumerable<TContent> TrackTrashedForDeleteContentOfTypes<TContent>(this IContentServiceBase contentService,
            IEnumerable<int> contentTypeIds,
            IDictionary<string, TContent> rootItems,
            IRepositoryVersionable<int, TContent> repository)
            where TContent : IContentBase
        {
            const int pageSize = 10000;
            var contentToRecycle = new List<TContent>();

            //iterate over the root items found in the collection to be deleted, then discover which descendant items
            //need to be moved to the recycle bin
            foreach (var content in rootItems)
            {
                //Look for children of current content and move that to trash before the current content is deleted
                var c = content;
                var pathMatch = string.Format("{0},", c.Value.Path);
                var descendantQuery = Query<TContent>.Builder.Where(x => x.Path.StartsWith(pathMatch));

                long pageIndex = 0;                
                int currentPageSize;

                do
                {
                    long total;

                    var descendants = repository.GetPagedResultsByQuery(descendantQuery, pageIndex, pageSize, out total, "umbracoNode.id", Direction.Ascending, true).ToArray();

                    foreach (var d in descendants)
                    {
                        //track for recycling if this item is not of a contenttype that is being deleted
                        if (contentTypeIds.Contains(d.ContentTypeId) == false)
                        {
                            contentToRecycle.Add(d);
                        }
                    }

                    // need to store decendants count before filtering, in order for loop to work correctly
                    currentPageSize = descendants.Length;

                    pageIndex++;
                } while (currentPageSize == pageSize);
            }

            return contentToRecycle;
        }
    }
}