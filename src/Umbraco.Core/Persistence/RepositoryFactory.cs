using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence
{
    /// <summary>
    /// Used to instantiate each repository type
    /// </summary>
    public class RepositoryFactory
    {
        public virtual IContentRepository CreateContentRepository(IDatabaseUnitOfWork uow)
        {
            return new ContentRepository(
                uow,
                RuntimeCacheProvider.Current,
                CreateContentTypeRepository(uow),
                CreateTemplateRepository(uow)) { EnsureUniqueNaming = Umbraco.Core.Configuration.UmbracoSettings.EnsureUniqueNaming };
        }

        public virtual IContentTypeRepository CreateContentTypeRepository(IDatabaseUnitOfWork uow)
        {
            return new ContentTypeRepository(
                uow,
                InMemoryCacheProvider.Current,
                new TemplateRepository(uow, NullCacheProvider.Current));
        }

        public virtual IDataTypeDefinitionRepository CreateDataTypeDefinitionRepository(IDatabaseUnitOfWork uow)
        {
            return new DataTypeDefinitionRepository(
                uow,
                NullCacheProvider.Current);
        }

        public virtual IDictionaryRepository CreateDictionaryRepository(IDatabaseUnitOfWork uow)
        {
            return new DictionaryRepository(
                uow,
                InMemoryCacheProvider.Current,
                CreateLanguageRepository(uow));
        }

        public virtual ILanguageRepository CreateLanguageRepository(IDatabaseUnitOfWork uow)
        {
            return new LanguageRepository(
                uow,
                InMemoryCacheProvider.Current);
        }

        public virtual IMediaRepository CreateMediaRepository(IDatabaseUnitOfWork uow)
        {
            return new MediaRepository(
                uow,
                RuntimeCacheProvider.Current,
                CreateMediaTypeRepository(uow)) { EnsureUniqueNaming = Umbraco.Core.Configuration.UmbracoSettings.EnsureUniqueNaming };
        }

        public virtual IMediaTypeRepository CreateMediaTypeRepository(IDatabaseUnitOfWork uow)
        {
            return new MediaTypeRepository(
                uow,
                InMemoryCacheProvider.Current);
        }

        public virtual IRelationRepository CreateRelationRepository(IDatabaseUnitOfWork uow)
        {
            return new RelationRepository(
                uow,
                NullCacheProvider.Current,
                CreateRelationTypeRepository(uow));
        }

        public virtual IRelationTypeRepository CreateRelationTypeRepository(IDatabaseUnitOfWork uow)
        {
            return new RelationTypeRepository(
                uow,
                NullCacheProvider.Current);
        }

        public virtual IScriptRepository CreateScriptRepository(IUnitOfWork uow)
        {
            return new ScriptRepository(uow);
        }

        public virtual IStylesheetRepository CreateStylesheetRepository(IUnitOfWork uow)
        {
            return new StylesheetRepository(uow);
        }

        public virtual ITemplateRepository CreateTemplateRepository(IDatabaseUnitOfWork uow)
        {
            return new TemplateRepository(uow, RuntimeCacheProvider.Current);
        }

        internal virtual ServerRegistrationRepository CreateServerRegistrationRepository(IDatabaseUnitOfWork uow)
        {
            return new ServerRegistrationRepository(
                uow,
                NullCacheProvider.Current);
        }

        internal virtual IUserTypeRepository CreateUserTypeRepository(IDatabaseUnitOfWork uow)
        {
            return new UserTypeRepository(
                uow,
                NullCacheProvider.Current);
        }

        internal virtual IUserRepository CreateUserRepository(IDatabaseUnitOfWork uow)
        {
            return new UserRepository(
                uow,
                NullCacheProvider.Current,
                CreateUserTypeRepository(uow));
        }

        internal virtual IEntityRepository CreateEntityRepository(IDatabaseUnitOfWork uow)
        {
            return new EntityRepository(uow);
        }

        internal virtual RecycleBinRepository CreateRecycleBinRepository(IDatabaseUnitOfWork uow)
        {
            return new RecycleBinRepository(uow);
        }
    }
}