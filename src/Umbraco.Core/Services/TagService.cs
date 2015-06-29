using System;
using System.Collections.Generic;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Tag service to query for tags in the tags db table. The tags returned are only relevant for published content & saved media or members 
    /// </summary>
    /// <remarks>
    /// If there is unpublished content with tags, those tags will not be contained
    /// </remarks>
    public class TagService : RepositoryService, ITagService
    {

        [Obsolete("Use the constructors that specify all dependencies instead")]
        public TagService()
            : this(new RepositoryFactory())
        {}

        [Obsolete("Use the constructors that specify all dependencies instead")]
        public TagService(RepositoryFactory repositoryFactory)
            : this(new PetaPocoUnitOfWorkProvider(), repositoryFactory)
        {
        }

        [Obsolete("Use the constructors that specify all dependencies instead")]
        public TagService(IDatabaseUnitOfWorkProvider provider)
            : this(provider, new RepositoryFactory())
        {
        }

        [Obsolete("Use the constructors that specify all dependencies instead")]
        public TagService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory)
            : this(provider, repositoryFactory, LoggerResolver.Current.Logger)
        {
        }

        public TagService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, ILogger logger)
            : base(provider, repositoryFactory, logger)
        {
        }

        /// <summary>
        /// Gets tagged Content by a specific 'Tag Group'.
        /// </summary>
        /// <remarks>The <see cref="TaggedEntity"/> contains the Id and Tags of the Content, not the actual Content item.</remarks>
        /// <param name="tagGroup">Name of the 'Tag Group'</param>
        /// <returns>An enumerable list of <see cref="TaggedEntity"/></returns>
        public IEnumerable<TaggedEntity> GetTaggedContentByTagGroup(string tagGroup)
        {
            using (var repository = RepositoryFactory.CreateTagRepository(UowProvider.GetUnitOfWork()))
            {
                return repository.GetTaggedEntitiesByTagGroup(TaggableObjectTypes.Content, tagGroup);
            }         
        }

        /// <summary>
        /// Gets tagged Content by a specific 'Tag' and optional 'Tag Group'.
        /// </summary>
        /// <remarks>The <see cref="TaggedEntity"/> contains the Id and Tags of the Content, not the actual Content item.</remarks>
        /// <param name="tag">Tag</param>
        /// <param name="tagGroup">Optional name of the 'Tag Group'</param>
        /// <returns>An enumerable list of <see cref="TaggedEntity"/></returns>
        public IEnumerable<TaggedEntity> GetTaggedContentByTag(string tag, string tagGroup = null)
        {
            using (var repository = RepositoryFactory.CreateTagRepository(UowProvider.GetUnitOfWork()))
            {
                return repository.GetTaggedEntitiesByTag(TaggableObjectTypes.Content, tag, tagGroup);
            }         
        }

        /// <summary>
        /// Gets tagged Media by a specific 'Tag Group'.
        /// </summary>
        /// <remarks>The <see cref="TaggedEntity"/> contains the Id and Tags of the Media, not the actual Media item.</remarks>
        /// <param name="tagGroup">Name of the 'Tag Group'</param>
        /// <returns>An enumerable list of <see cref="TaggedEntity"/></returns>
        public IEnumerable<TaggedEntity> GetTaggedMediaByTagGroup(string tagGroup)
        {
            using (var repository = RepositoryFactory.CreateTagRepository(UowProvider.GetUnitOfWork()))
            {
                return repository.GetTaggedEntitiesByTagGroup(TaggableObjectTypes.Media, tagGroup);
            }    
        }

        /// <summary>
        /// Gets tagged Media by a specific 'Tag' and optional 'Tag Group'.
        /// </summary>
        /// <remarks>The <see cref="TaggedEntity"/> contains the Id and Tags of the Media, not the actual Media item.</remarks>
        /// <param name="tag">Tag</param>
        /// <param name="tagGroup">Optional name of the 'Tag Group'</param>
        /// <returns>An enumerable list of <see cref="TaggedEntity"/></returns>
        public IEnumerable<TaggedEntity> GetTaggedMediaByTag(string tag, string tagGroup = null)
        {
            using (var repository = RepositoryFactory.CreateTagRepository(UowProvider.GetUnitOfWork()))
            {
                return repository.GetTaggedEntitiesByTag(TaggableObjectTypes.Media, tag, tagGroup);
            }    
        }

        /// <summary>
        /// Gets tagged Members by a specific 'Tag Group'.
        /// </summary>
        /// <remarks>The <see cref="TaggedEntity"/> contains the Id and Tags of the Member, not the actual Member item.</remarks>
        /// <param name="tagGroup">Name of the 'Tag Group'</param>
        /// <returns>An enumerable list of <see cref="TaggedEntity"/></returns>
        public IEnumerable<TaggedEntity> GetTaggedMembersByTagGroup(string tagGroup)
        {
            using (var repository = RepositoryFactory.CreateTagRepository(UowProvider.GetUnitOfWork()))
            {
                return repository.GetTaggedEntitiesByTagGroup(TaggableObjectTypes.Member, tagGroup);
            }    
        }

        /// <summary>
        /// Gets tagged Members by a specific 'Tag' and optional 'Tag Group'.
        /// </summary>
        /// <remarks>The <see cref="TaggedEntity"/> contains the Id and Tags of the Member, not the actual Member item.</remarks>
        /// <param name="tag">Tag</param>
        /// <param name="tagGroup">Optional name of the 'Tag Group'</param>
        /// <returns>An enumerable list of <see cref="TaggedEntity"/></returns>
        public IEnumerable<TaggedEntity> GetTaggedMembersByTag(string tag, string tagGroup = null)
        {
            using (var repository = RepositoryFactory.CreateTagRepository(UowProvider.GetUnitOfWork()))
            {
                return repository.GetTaggedEntitiesByTag(TaggableObjectTypes.Member, tag, tagGroup);
            }    
        }

        /// <summary>
        /// Gets every tag stored in the database
        /// </summary>
        /// <param name="tagGroup">Optional name of the 'Tag Group'</param>
        /// <returns>An enumerable list of <see cref="ITag"/></returns>
        public IEnumerable<ITag> GetAllTags(string tagGroup = null)
        {
            using (var repository = RepositoryFactory.CreateTagRepository(UowProvider.GetUnitOfWork()))
            {
                return repository.GetTagsForEntityType(TaggableObjectTypes.All, tagGroup);
            }  
        }

        /// <summary>
        /// Gets all tags for content items
        /// </summary>
        /// <remarks>Use the optional tagGroup parameter to limit the 
        /// result to a specific 'Tag Group'.</remarks>
        /// <param name="tagGroup">Optional name of the 'Tag Group'</param>
        /// <returns>An enumerable list of <see cref="ITag"/></returns>
        public IEnumerable<ITag> GetAllContentTags(string tagGroup = null)
        {
            using (var repository = RepositoryFactory.CreateTagRepository(UowProvider.GetUnitOfWork()))
            {
                return repository.GetTagsForEntityType(TaggableObjectTypes.Content, tagGroup);
            }
        }

        /// <summary>
        /// Gets all tags for media items
        /// </summary>
        /// <remarks>Use the optional tagGroup parameter to limit the 
        /// result to a specific 'Tag Group'.</remarks>
        /// <param name="tagGroup">Optional name of the 'Tag Group'</param>
        /// <returns>An enumerable list of <see cref="ITag"/></returns>
        public IEnumerable<ITag> GetAllMediaTags(string tagGroup = null)
        {
            using (var repository = RepositoryFactory.CreateTagRepository(UowProvider.GetUnitOfWork()))
            {
                return repository.GetTagsForEntityType(TaggableObjectTypes.Media, tagGroup);
            }
        }

        /// <summary>
        /// Gets all tags for member items
        /// </summary>
        /// <remarks>Use the optional tagGroup parameter to limit the 
        /// result to a specific 'Tag Group'.</remarks>
        /// <param name="tagGroup">Optional name of the 'Tag Group'</param>
        /// <returns>An enumerable list of <see cref="ITag"/></returns>
        public IEnumerable<ITag> GetAllMemberTags(string tagGroup = null)
        {
            using (var repository = RepositoryFactory.CreateTagRepository(UowProvider.GetUnitOfWork()))
            {
                return repository.GetTagsForEntityType(TaggableObjectTypes.Member, tagGroup);
            }
        }

        /// <summary>
        /// Gets all tags attached to a property by entity id
        /// </summary>
        /// <remarks>Use the optional tagGroup parameter to limit the 
        /// result to a specific 'Tag Group'.</remarks>
        /// <param name="contentId">The content item id to get tags for</param>
        /// <param name="propertyTypeAlias">Property type alias</param>
        /// <param name="tagGroup">Optional name of the 'Tag Group'</param>
        /// <returns>An enumerable list of <see cref="ITag"/></returns>
        public IEnumerable<ITag> GetTagsForProperty(int contentId, string propertyTypeAlias, string tagGroup = null)
        {
            using (var repository = RepositoryFactory.CreateTagRepository(UowProvider.GetUnitOfWork()))
            {
                return repository.GetTagsForProperty(contentId, propertyTypeAlias, tagGroup);
            }
        }

        /// <summary>
        /// Gets all tags attached to an entity (content, media or member) by entity id
        /// </summary>
        /// <remarks>Use the optional tagGroup parameter to limit the 
        /// result to a specific 'Tag Group'.</remarks>
        /// <param name="contentId">The content item id to get tags for</param>
        /// <param name="tagGroup">Optional name of the 'Tag Group'</param>
        /// <returns>An enumerable list of <see cref="ITag"/></returns>
        public IEnumerable<ITag> GetTagsForEntity(int contentId, string tagGroup = null)
        {
            using (var repository = RepositoryFactory.CreateTagRepository(UowProvider.GetUnitOfWork()))
            {
                return repository.GetTagsForEntity(contentId, tagGroup);
            }
        }
    }
}