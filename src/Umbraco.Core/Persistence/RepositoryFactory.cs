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

        internal virtual IContentRepository CreateContentRepository(IDatabaseUnitOfWork uow)
        {
            return new ContentRepository(
                uow,
                RuntimeCacheProvider.Current,
                CreateContentTypeRepository(uow),
                CreateTemplateRepository(uow));
        }

        internal virtual IContentTypeRepository CreateContentTypeRepository(IDatabaseUnitOfWork uow)
        {
            return new ContentTypeRepository(
                uow,
                InMemoryCacheProvider.Current,
                new TemplateRepository(uow, NullCacheProvider.Current));
        }

        internal virtual IDataTypeDefinitionRepository CreateDataTypeDefinitionRepository(IDatabaseUnitOfWork uow)
        {
            return new DataTypeDefinitionRepository(
                uow,
                NullCacheProvider.Current);
        }

        internal virtual IDictionaryRepository CreateDictionaryRepository(IDatabaseUnitOfWork uow)
        {
            return new DictionaryRepository(
                uow,
                InMemoryCacheProvider.Current,
                CreateLanguageRepository(uow));
        }

        internal virtual ILanguageRepository CreateLanguageRepository(IDatabaseUnitOfWork uow)
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

        internal virtual IMediaRepository CreateMediaRepository(IDatabaseUnitOfWork uow)
        {
            return new MediaRepository(
                uow,
                RuntimeCacheProvider.Current,
                CreateMediaTypeRepository(uow));
        }

        internal virtual IMediaTypeRepository CreateMediaTypeRepository(IDatabaseUnitOfWork uow)
        {
            return new MediaTypeRepository(
                uow,
                InMemoryCacheProvider.Current);
        }

        internal virtual IRelationRepository CreateRelationRepository(IDatabaseUnitOfWork uow)
        {
            return new RelationRepository(
                uow,
                NullCacheProvider.Current,
                CreateRelationTypeRepository(uow));
        }

        internal virtual IRelationTypeRepository CreateRelationTypeRepository(IDatabaseUnitOfWork uow)
        {
            return new RelationTypeRepository(
                uow,
                NullCacheProvider.Current);
        }

        internal virtual IScriptRepository CreateScriptRepository(IUnitOfWork uow)
        {
            return new ScriptRepository(uow);
        }

        internal virtual IStylesheetRepository CreateStylesheetRepository(IUnitOfWork uow)
        {
            return new StylesheetRepository(uow);
        }

        internal virtual ITemplateRepository CreateTemplateRepository(IDatabaseUnitOfWork uow)
        {
            return new TemplateRepository(uow, NullCacheProvider.Current);
        }
    }
}