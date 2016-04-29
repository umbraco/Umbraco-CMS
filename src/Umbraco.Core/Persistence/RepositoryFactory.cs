using System;
using LightInject;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence
{
    /// <summary>
    /// Instanciates repositories.
    /// </summary>
    public class RepositoryFactory
    {
        private readonly IServiceContainer _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryFactory"/> class with a container.
        /// </summary>
        /// <param name="container">A container.</param>
        public RepositoryFactory(IServiceContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            _container = container;
        }

        public virtual INotificationsRepository CreateNotificationsRepository(IDatabaseUnitOfWork uow)
        {
            return _container.GetInstance<IDatabaseUnitOfWork, INotificationsRepository>(uow);
        }

        public virtual IExternalLoginRepository CreateExternalLoginRepository(IDatabaseUnitOfWork uow)
        {
            return _container.GetInstance<IDatabaseUnitOfWork, IExternalLoginRepository>(uow);
        }

        public virtual IPublicAccessRepository CreatePublicAccessRepository(IDatabaseUnitOfWork uow)
        {
            return _container.GetInstance<IDatabaseUnitOfWork, IPublicAccessRepository>(uow);
        }

        public virtual ITaskRepository CreateTaskRepository(IDatabaseUnitOfWork uow)
        {
            return _container.GetInstance<IDatabaseUnitOfWork, ITaskRepository>(uow);
        }

        public virtual IAuditRepository CreateAuditRepository(IDatabaseUnitOfWork uow)
        {
            return _container.GetInstance<IDatabaseUnitOfWork, IAuditRepository>(uow);
        }

        public virtual ITagRepository CreateTagRepository(IDatabaseUnitOfWork uow)
        {
            return _container.GetInstance<IDatabaseUnitOfWork, ITagRepository>(uow);
        }

        public virtual IContentRepository CreateContentRepository(IDatabaseUnitOfWork uow)
        {
            return _container.GetInstance<IDatabaseUnitOfWork, IContentRepository>(uow);
        }

        public virtual IContentTypeRepository CreateContentTypeRepository(IDatabaseUnitOfWork uow)
        {
            return _container.GetInstance<IDatabaseUnitOfWork, IContentTypeRepository>(uow);
        }

        public virtual IDataTypeDefinitionRepository CreateDataTypeDefinitionRepository(IDatabaseUnitOfWork uow)
        {
            return _container.GetInstance<IDatabaseUnitOfWork, IDataTypeDefinitionRepository>(uow);
        }

        public virtual IDictionaryRepository CreateDictionaryRepository(IDatabaseUnitOfWork uow)
        {
            return _container.GetInstance<IDatabaseUnitOfWork, IDictionaryRepository>(uow);
        }

        public virtual ILanguageRepository CreateLanguageRepository(IDatabaseUnitOfWork uow)
        {
            return _container.GetInstance<IDatabaseUnitOfWork, ILanguageRepository>(uow);
        }

        public virtual IMediaRepository CreateMediaRepository(IDatabaseUnitOfWork uow)
        {
            return _container.GetInstance<IDatabaseUnitOfWork, IMediaRepository>(uow);
        }

        public virtual IMediaTypeRepository CreateMediaTypeRepository(IDatabaseUnitOfWork uow)
        {
            return _container.GetInstance<IDatabaseUnitOfWork, IMediaTypeRepository>(uow);
        }

        public virtual IRelationRepository CreateRelationRepository(IDatabaseUnitOfWork uow)
        {
            return _container.GetInstance<IDatabaseUnitOfWork, IRelationRepository>(uow);
        }

        public virtual IRelationTypeRepository CreateRelationTypeRepository(IDatabaseUnitOfWork uow)
        {
            return _container.GetInstance<IDatabaseUnitOfWork, IRelationTypeRepository>(uow);
        }

        public virtual IScriptRepository CreateScriptRepository(IUnitOfWork uow)
        {
            return _container.GetInstance<IUnitOfWork, IScriptRepository>(uow);
        }

        internal virtual IPartialViewRepository CreatePartialViewRepository(IUnitOfWork uow)
        {
            return _container.GetInstance<IUnitOfWork, IPartialViewRepository>(uow, "PartialViewRepository");
        }

        internal virtual IPartialViewRepository CreatePartialViewMacroRepository(IUnitOfWork uow)
        {
            return _container.GetInstance<IUnitOfWork, IPartialViewRepository>(uow, "PartialViewMacroRepository");
        }

        public virtual IStylesheetRepository CreateStylesheetRepository(IUnitOfWork uow, IDatabaseUnitOfWork db)
        {
            return _container.GetInstance<IUnitOfWork, IStylesheetRepository>(uow);
        }

        public virtual ITemplateRepository CreateTemplateRepository(IDatabaseUnitOfWork uow)
        {
            return _container.GetInstance<IDatabaseUnitOfWork, ITemplateRepository>(uow);
        }

        public virtual IMigrationEntryRepository CreateMigrationEntryRepository(IDatabaseUnitOfWork uow)
        {
            return _container.GetInstance<IDatabaseUnitOfWork, IMigrationEntryRepository>(uow);
        }

        public virtual IServerRegistrationRepository CreateServerRegistrationRepository(IDatabaseUnitOfWork uow)
        {
            return _container.GetInstance<IDatabaseUnitOfWork, IServerRegistrationRepository>(uow);
        }

        public virtual IUserTypeRepository CreateUserTypeRepository(IDatabaseUnitOfWork uow)
        {
            return _container.GetInstance<IDatabaseUnitOfWork, IUserTypeRepository>(uow);
        }

        public virtual IUserRepository CreateUserRepository(IDatabaseUnitOfWork uow)
        {
            return _container.GetInstance<IDatabaseUnitOfWork, IUserRepository>(uow);
        }

        internal virtual IMacroRepository CreateMacroRepository(IDatabaseUnitOfWork uow)
        {
            return _container.GetInstance<IDatabaseUnitOfWork, IMacroRepository>(uow);
        }

        public virtual IMemberRepository CreateMemberRepository(IDatabaseUnitOfWork uow)
        {
            return _container.GetInstance<IDatabaseUnitOfWork, IMemberRepository>(uow);
        }

        public virtual IMemberTypeRepository CreateMemberTypeRepository(IDatabaseUnitOfWork uow)
        {
            return _container.GetInstance<IDatabaseUnitOfWork, IMemberTypeRepository>(uow);
        }

        public virtual IMemberGroupRepository CreateMemberGroupRepository(IDatabaseUnitOfWork uow)
        {
            return _container.GetInstance<IDatabaseUnitOfWork, IMemberGroupRepository>(uow);
        }

        public virtual IEntityRepository CreateEntityRepository(IDatabaseUnitOfWork uow)
        {
            return _container.GetInstance<IDatabaseUnitOfWork, IEntityRepository>(uow);
        }

        public virtual IDomainRepository CreateDomainRepository(IDatabaseUnitOfWork uow)
        {
            return _container.GetInstance<IDatabaseUnitOfWork, IDomainRepository>(uow);
        }

        public virtual ITaskTypeRepository CreateTaskTypeRepository(IDatabaseUnitOfWork uow)
        {
            return _container.GetInstance<IDatabaseUnitOfWork, ITaskTypeRepository>(uow);
        }

        internal virtual EntityContainerRepository CreateEntityContainerRepository(IDatabaseUnitOfWork uow, Guid containerObjectType)
        {
            return _container.GetInstance<IDatabaseUnitOfWork, EntityContainerRepository>(uow);
        }
    }
}