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
        internal virtual IUserTypeRepository CreateUserTypeRepository(IUnitOfWork uow)
        {
            return new UserTypeRepository(
                uow,
                NullCacheProvider.Current);
        }

        internal virtual IUserRepository CreateUserRepository(IUnitOfWork uow)
        {
            return new UserRepository(
                uow,
                NullCacheProvider.Current,
                CreateUserTypeRepository(uow));
        }

        public virtual IContentRepository CreateContentRepository(IUnitOfWork uow)
        {
            return new ContentRepository(
                uow,
                RuntimeCacheProvider.Current,
                CreateContentTypeRepository(uow),
                CreateTemplateRepository(uow));
        }

        public virtual IContentTypeRepository CreateContentTypeRepository(IUnitOfWork uow)
        {
            return new ContentTypeRepository(
                uow,
                InMemoryCacheProvider.Current,
                new TemplateRepository(uow, NullCacheProvider.Current));
        }

        public virtual IDataTypeDefinitionRepository CreateDataTypeDefinitionRepository(IUnitOfWork uow)
        {
            return new DataTypeDefinitionRepository(
                uow,
                NullCacheProvider.Current);
        }

        public virtual IDictionaryRepository CreateDictionaryRepository(IUnitOfWork uow)
        {
            return new DictionaryRepository(
                uow,
                InMemoryCacheProvider.Current,
                CreateLanguageRepository(uow));
        }

        public virtual ILanguageRepository CreateLanguageRepository(IUnitOfWork uow)
        {
            return new LanguageRepository(
                uow,
                InMemoryCacheProvider.Current);
        }

        internal virtual IMacroRepository CreateMacroRepository(IUnitOfWork uow)
        {
            return new MacroRepository(
                uow,
                InMemoryCacheProvider.Current);
        }

        public virtual IMediaRepository CreateMediaRepository(IUnitOfWork uow)
        {
            return new MediaRepository(
                uow,
                RuntimeCacheProvider.Current,
                CreateMediaTypeRepository(uow));
        }

        public virtual IMediaTypeRepository CreateMediaTypeRepository(IUnitOfWork uow)
        {
            return new MediaTypeRepository(
                uow,
                InMemoryCacheProvider.Current);
        }

        public virtual IRelationRepository CreateRelationRepository(IUnitOfWork uow)
        {
            return new RelationRepository(
                uow,
                NullCacheProvider.Current,
                CreateRelationTypeRepository(uow));
        }

        public virtual IRelationTypeRepository CreateRelationTypeRepository(IUnitOfWork uow)
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

        public virtual ITemplateRepository CreateTemplateRepository(IUnitOfWork uow)
        {
            return new TemplateRepository(uow, NullCacheProvider.Current);
        }
    }
}