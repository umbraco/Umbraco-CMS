using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Tag service to query for tags in the tags db table. The tags returned are only relavent for published content & saved media or members 
    /// </summary>
    /// <remarks>
    /// If there is unpublished content with tags, those tags will not be contained
    /// </remarks>
    public class TagService : ITagService
    {

        private readonly RepositoryFactory _repositoryFactory;
        private readonly IDatabaseUnitOfWorkProvider _uowProvider;

        public TagService()
            : this(new RepositoryFactory())
        {}

        public TagService(RepositoryFactory repositoryFactory)
            : this(new PetaPocoUnitOfWorkProvider(), repositoryFactory)
        {
        }

        public TagService(IDatabaseUnitOfWorkProvider provider)
            : this(provider, new RepositoryFactory())
        {
        }

        public TagService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
            _uowProvider = provider;
        }

        /// <summary>
        /// Get every tag stored in the database (with optional group)
        /// </summary>
        public IEnumerable<ITag> GetAllTags(string group = null)
        {
            using (var repository = _repositoryFactory.CreateTagsRepository(_uowProvider.GetUnitOfWork()))
            {
                if (group.IsNullOrWhiteSpace())
                {
                    return repository.GetAll();
                }

                var query = Query<ITag>.Builder.Where(x => x.Group == group);
                var definitions = repository.GetByQuery(query);
                return definitions;
            }  
        }

        /// <summary>
        /// Get all tags for content items (with optional group)
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public IEnumerable<ITag> GetAllContentTags(string group = null)
        {
            using (var repository = _repositoryFactory.CreateTagsRepository(_uowProvider.GetUnitOfWork()))
            {
                return repository.GetTagsForEntityType(TaggableObjectTypes.Content, group);
            }
        }

        /// <summary>
        /// Get all tags for media items (with optional group)
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public IEnumerable<ITag> GetAllMediaTags(string group = null)
        {
            using (var repository = _repositoryFactory.CreateTagsRepository(_uowProvider.GetUnitOfWork()))
            {
                return repository.GetTagsForEntityType(TaggableObjectTypes.Media, group);
            }
        }

        /// <summary>
        /// Get all tags for member items (with optional group)
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public IEnumerable<ITag> GetAllMemberTags(string group = null)
        {
            using (var repository = _repositoryFactory.CreateTagsRepository(_uowProvider.GetUnitOfWork()))
            {
                return repository.GetTagsForEntityType(TaggableObjectTypes.Member, group);
            }
        }

        /// <summary>
        /// Returns all tags attached to a property by entity id
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="propertyTypeAlias"></param>
        /// <param name="tagGroup"></param>
        /// <returns></returns>
        public IEnumerable<ITag> GetTagsForProperty(int contentId, string propertyTypeAlias, string tagGroup = null)
        {
            using (var repository = _repositoryFactory.CreateTagsRepository(_uowProvider.GetUnitOfWork()))
            {
                return repository.GetTagsForProperty(contentId, propertyTypeAlias, tagGroup);
            }
        }

        /// <summary>
        /// Returns all tags attached to an entity (content, media or member) by entity id
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="tagGroup"></param>
        /// <returns></returns>
        public IEnumerable<ITag> GetTagsForEntity(int contentId, string tagGroup = null)
        {
            using (var repository = _repositoryFactory.CreateTagsRepository(_uowProvider.GetUnitOfWork()))
            {
                return repository.GetTagsForEntity(contentId, tagGroup);
            }
        }
    }
}