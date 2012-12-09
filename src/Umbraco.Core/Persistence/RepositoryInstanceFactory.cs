using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence
{
	/// <summary>
	/// Used to instantiate each repository type
	/// </summary>
	public class RepositoryInstanceFactory
	{

		[RepositoryInstanceType(typeof(IUserTypeRepository))]
		internal virtual IUserTypeRepository CreateUserTypeRepository(IUnitOfWork uow)
		{
			return new UserTypeRepository(
				uow,
				NullCacheProvider.Current);

		}

		[RepositoryInstanceType(typeof(IUserRepository))]
		internal virtual IUserRepository CreateUserRepository(IUnitOfWork uow)
		{
			return new UserRepository(
				uow,
				NullCacheProvider.Current,
				CreateUserTypeRepository(uow));

		}

		[RepositoryInstanceType(typeof(IContentRepository))]
		public virtual IContentRepository CreateContentRepository(IUnitOfWork uow)
		{
			return new ContentRepository(
				uow,
				RuntimeCacheProvider.Current,
				CreateContentTypeRepository(uow),
				CreateTemplateRepository(uow));
		}

		[RepositoryInstanceType(typeof(IContentTypeRepository))]
		public virtual IContentTypeRepository CreateContentTypeRepository(IUnitOfWork uow)
		{
			return new ContentTypeRepository(
				uow,
				InMemoryCacheProvider.Current,
				new TemplateRepository(uow, NullCacheProvider.Current));
		}

		[RepositoryInstanceType(typeof(IDataTypeDefinitionRepository))]
		public virtual IDataTypeDefinitionRepository CreateDataTypeDefinitionRepository(IUnitOfWork uow)
		{
			return new DataTypeDefinitionRepository(
				uow,
				NullCacheProvider.Current);
		}

		//TODO: Shouldn't IDictionaryRepository be public?
		[RepositoryInstanceType(typeof(IDictionaryRepository))]
		internal virtual IDictionaryRepository CreateDictionaryRepository(IUnitOfWork uow)
		{
			return new DictionaryRepository(
				uow,
				InMemoryCacheProvider.Current,
				CreateLanguageRepository(uow));
		}

		[RepositoryInstanceType(typeof(ILanguageRepository))]
		public virtual ILanguageRepository CreateLanguageRepository(IUnitOfWork uow)
		{
			return new LanguageRepository(
				uow,
				InMemoryCacheProvider.Current);
		}

		//TODO: Shouldn't IMacroRepository be public?
		[RepositoryInstanceType(typeof(IMacroRepository))]
		internal virtual IMacroRepository CreateMacroRepository(IUnitOfWork uow)
		{
			return new MacroRepository(
				uow,
				InMemoryCacheProvider.Current);
		}

		[RepositoryInstanceType(typeof(IMediaRepository))]
		public virtual IMediaRepository CreateMediaRepository(IUnitOfWork uow)
		{
			return new MediaRepository(
				uow,
				RuntimeCacheProvider.Current,
				CreateMediaTypeRepository(uow));
		}

		[RepositoryInstanceType(typeof(IMediaTypeRepository))]
		public virtual IMediaTypeRepository CreateMediaTypeRepository(IUnitOfWork uow)
		{
			return new MediaTypeRepository(
				uow,
				InMemoryCacheProvider.Current);
		}

		[RepositoryInstanceType(typeof(IRelationRepository))]
		public virtual IRelationRepository CreateRelationRepository(IUnitOfWork uow)
		{
			return new RelationRepository(
				uow,
				NullCacheProvider.Current, 
				CreateRelationTypeRepository(uow));
		}

		[RepositoryInstanceType(typeof(IRelationTypeRepository))]
		public virtual IRelationTypeRepository CreateRelationTypeRepository(IUnitOfWork uow)
		{
			return new RelationTypeRepository(
				uow,
				NullCacheProvider.Current);
		}

		[RepositoryInstanceType(typeof(IScriptRepository))]
		public virtual IScriptRepository CreateScriptRepository(IUnitOfWork uow)
		{
			return new ScriptRepository(uow);
		}

		[RepositoryInstanceType(typeof(IStylesheetRepository))]
		public virtual IStylesheetRepository CreateStylesheetRepository(IUnitOfWork uow)
		{
			return new StylesheetRepository(uow);
		}

		[RepositoryInstanceType(typeof(ITemplateRepository))]
		public virtual ITemplateRepository CreateTemplateRepository(IUnitOfWork uow)
		{
			return new TemplateRepository(uow, NullCacheProvider.Current);
		}


	}
}