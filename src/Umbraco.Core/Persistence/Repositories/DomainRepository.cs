using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    internal class DomainRepository : PetaPocoRepositoryBase<int, IDomain>, IDomainRepository
    {
        private readonly IContentRepository _contentRepository;
        private readonly ILanguageRepository _languageRepository;

        public DomainRepository(IDatabaseUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax, IContentRepository contentRepository, ILanguageRepository languageRepository)
            : base(work, cache, logger, sqlSyntax)
        {
            _contentRepository = contentRepository;
            _languageRepository = languageRepository;
        }

        /// <summary>
        /// Override the cache, this repo will not perform any cache, the caching is taken care of in the inner repository
        /// </summary>
        /// <remarks>
        /// This is required because IDomain is a deep object and we dont' want to cache it since it contains an ILanguage and an IContent, when these
        /// are deep cloned the object graph that will be cached will be huge. Instead we'll have an internal repository that caches the simple
        /// Domain structure and we'll use the other repositories to resolve the entities to attach
        /// </remarks>
        protected override IRuntimeCacheProvider RuntimeCache
        {
            get { return new NullCacheProvider(); }
        }

        protected override IDomain PerformGet(int id)
        {
            using (var repo = new CachedDomainRepository(this, UnitOfWork, RepositoryCache, Logger, SqlSyntax))
            {
                var factory = new DomainModelFactory();
                return factory.BuildDomainEntity(repo.Get(id), _contentRepository, _languageRepository);
            }
        }

        protected override IEnumerable<IDomain> PerformGetAll(params int[] ids)
        {
            using (var repo = new CachedDomainRepository(this, UnitOfWork, RepositoryCache, Logger, SqlSyntax))
            {
                var factory = new DomainModelFactory();
                return factory.BuildDomainEntities(repo.GetAll(ids).ToArray(), _contentRepository, _languageRepository);
            }
        }

        protected override IEnumerable<IDomain> PerformGetByQuery(IQuery<IDomain> query)
        {
            throw new NotSupportedException("This repository does not support this method");
        }

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            sql.Select(isCount ? "COUNT(*)" : "*").From<DomainDto>(SqlSyntax);
            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return "umbracoDomains.id = @Id";
        }

        protected override void PersistDeletedItem(IDomain entity)
        {
            using (var repo = new CachedDomainRepository(this, UnitOfWork, RepositoryCache, Logger, SqlSyntax))
            {
                var factory = new DomainModelFactory();
                repo.PersistDeletedItem(factory.BuildEntity(entity));
            }
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            throw new NotImplementedException();
        }

        protected override Guid NodeObjectTypeId
        {
            get { throw new NotImplementedException(); }
        }

        protected override void PersistNewItem(IDomain entity)
        {
            using (var repo = new CachedDomainRepository(this, UnitOfWork, RepositoryCache, Logger, SqlSyntax))
            {
                var factory = new DomainModelFactory();
                var cacheableEntity = factory.BuildEntity(entity);
                repo.PersistNewItem(cacheableEntity);
                //re-map the id
                entity.Id = cacheableEntity.Id;
                entity.ResetDirtyProperties();
            }
        }

        protected override void PersistUpdatedItem(IDomain entity)
        {
            using (var repo = new CachedDomainRepository(this, UnitOfWork, RepositoryCache, Logger, SqlSyntax))
            {
                var factory = new DomainModelFactory();
                repo.PersistUpdatedItem(factory.BuildEntity(entity));
                entity.ResetDirtyProperties();
            }
        }

        public IDomain GetByName(string domainName)
        {
            using (var repo = new CachedDomainRepository(this, UnitOfWork, RepositoryCache, Logger, SqlSyntax))
            {
                var factory = new DomainModelFactory();
                return factory.BuildDomainEntity(
                    repo.GetByQuery(new Query<CacheableDomain>().Where(x => x.DomainName.InvariantEquals(domainName))).FirstOrDefault(),
                    _contentRepository, _languageRepository);
            }
        }

        public bool Exists(string domainName)
        {
            using (var repo = new CachedDomainRepository(this, UnitOfWork, RepositoryCache, Logger, SqlSyntax))
            {
                var query = new Query<CacheableDomain>().Where(x => x.DomainName.InvariantEquals(domainName));
                return repo.GetByQuery(query).Any();
            }
        }

        public IEnumerable<IDomain> GetAll(bool includeWildcards)
        {
            using (var repo = new CachedDomainRepository(this, UnitOfWork, RepositoryCache, Logger, SqlSyntax))
            {
                var factory = new DomainModelFactory();
                return factory.BuildDomainEntities(repo.GetAll().ToArray(), _contentRepository, _languageRepository)
                    .Where(x => includeWildcards || x.IsWildcard == false);
            }
        }

        public IEnumerable<IDomain> GetAssignedDomains(int contentId, bool includeWildcards)
        {
            using (var repo = new CachedDomainRepository(this, UnitOfWork, RepositoryCache, Logger, SqlSyntax))
            {
                var factory = new DomainModelFactory();

                var query = new Query<CacheableDomain>().Where(x => x.RootContentId == contentId);

                return factory.BuildDomainEntities(repo.GetByQuery(query).ToArray(), _contentRepository, _languageRepository)
                    .Where(x => includeWildcards || x.IsWildcard == false);
            }
        }

        /// <summary>
        /// Dispose disposable properties
        /// </summary>
        /// <remarks>
        /// Ensure the unit of work is disposed
        /// </remarks>
        protected override void DisposeResources()
        {
            _contentRepository.Dispose();
            _languageRepository.Dispose();
        }

        /// <summary>
        /// A simple domain model that is cacheable without a large object graph
        /// </summary>
        internal class CacheableDomain : Entity, IAggregateRoot
        {
            public int? DefaultLanguageId { get; set; }
            public string DomainName { get; set; }
            public int? RootContentId { get; set; }
        }

        /// <summary>
        /// Inner repository responsible for CRUD for domains that allows caching simple data
        /// </summary>
        private class CachedDomainRepository : PetaPocoRepositoryBase<int, CacheableDomain>
        {
            private readonly DomainRepository _domainRepo;

            public CachedDomainRepository(DomainRepository domainRepo, IDatabaseUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax)
                : base(work, cache, logger, sqlSyntax)
            {
                _domainRepo = domainRepo;
            }

            protected override CacheableDomain PerformGet(int id)
            {
                var sql = GetBaseQuery(false);
                sql.Where(GetBaseWhereClause(), new { Id = id });

                var dto = Database.FirstOrDefault<DomainDto>(sql);
                if (dto == null)
                    return null;

                var entity = ConvertFromDto(dto);

                //on initial construction we don't want to have dirty properties tracked
                // http://issues.umbraco.org/issue/U4-1946
                ((Entity)entity).ResetDirtyProperties(false);

                return entity;
            }

            protected override IEnumerable<CacheableDomain> PerformGetAll(params int[] ids)
            {
                var sql = GetBaseQuery(false).Where("umbracoDomains.id > 0");
                if (ids.Any())
                {
                    sql.Where("umbracoDomains.id in (@ids)", new { ids = ids });
                }

                return Database.Fetch<DomainDto>(sql).Select(ConvertFromDto);
            }

            protected override IEnumerable<CacheableDomain> PerformGetByQuery(IQuery<CacheableDomain> query)
            {
                var sqlClause = GetBaseQuery(false);
                var translator = new SqlTranslator<CacheableDomain>(sqlClause, query);
                var sql = translator.Translate();
                return Database.Fetch<DomainDto>(sql).Select(ConvertFromDto);
            }

            protected override Sql GetBaseQuery(bool isCount)
            {
                return _domainRepo.GetBaseQuery(isCount);
            }

            protected override string GetBaseWhereClause()
            {
                return _domainRepo.GetBaseWhereClause();
            }

            protected override IEnumerable<string> GetDeleteClauses()
            {
                var list = new List<string>
                {
                    "DELETE FROM umbracoDomains WHERE id = @Id"
                };
                return list;
            }

            protected override Guid NodeObjectTypeId
            {
                get { throw new NotImplementedException(); }
            }

            protected override void PersistNewItem(CacheableDomain entity)
            {
                var exists = Database.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoDomains WHERE domainName = @domainName", new { domainName = entity.DomainName });
                if (exists > 0) throw new DuplicateNameException(string.Format("The domain name {0} is already assigned", entity.DomainName));

                entity.AddingEntity();

                var factory = new DomainModelFactory();
                var dto = factory.BuildDto(entity);

                var id = Convert.ToInt32(Database.Insert(dto));
                entity.Id = id;

                entity.ResetDirtyProperties();
            }

            protected override void PersistUpdatedItem(CacheableDomain entity)
            {
                entity.UpdatingEntity();

                var factory = new DomainModelFactory();
                var dto = factory.BuildDto(entity);

                Database.Update(dto);

                entity.ResetDirtyProperties();
            }

            private CacheableDomain ConvertFromDto(DomainDto dto)
            {
                var factory = new DomainModelFactory();
                var entity = factory.BuildEntity(dto);
                return entity;
            }
        }

        internal class DomainModelFactory
        {
            public IEnumerable<IDomain> BuildDomainEntities(CacheableDomain[] cacheableDomains, IContentRepository contentRepository, ILanguageRepository languageRepository)
            {
                var contentIds = cacheableDomains.Select(x => x.RootContentId).Where(x => x.HasValue).Select(x => x.Value).Distinct().ToArray();
                var langIds = cacheableDomains.Select(x => x.DefaultLanguageId).Where(x => x.HasValue).Select(x => x.Value).Distinct().ToArray();
                var contentItems = contentRepository.GetAll(contentIds);
                var langItems = languageRepository.GetAll(langIds);

                return cacheableDomains
                    .WhereNotNull()
                    .Select(cacheableDomain => new UmbracoDomain(cacheableDomain.DomainName)
                    {
                        Id = cacheableDomain.Id,
                        //lookup from repo - this will be cached
                        Language = cacheableDomain.DefaultLanguageId.HasValue ? langItems.FirstOrDefault(l => l.Id == cacheableDomain.DefaultLanguageId.Value) : null,
                        //lookup from repo - this will be cached
                        RootContent = cacheableDomain.RootContentId.HasValue ? contentItems.FirstOrDefault(l => l.Id == cacheableDomain.RootContentId.Value) : null,
                    });
            }

            public IDomain BuildDomainEntity(CacheableDomain cacheableDomain, IContentRepository contentRepository, ILanguageRepository languageRepository)
            {
                if (cacheableDomain == null) return null;

                return new UmbracoDomain(cacheableDomain.DomainName)
                {
                    Id = cacheableDomain.Id,
                    //lookup from repo - this will be cached
                    Language = cacheableDomain.DefaultLanguageId.HasValue ? languageRepository.Get(cacheableDomain.DefaultLanguageId.Value) : null,
                    //lookup from repo - this will be cached
                    RootContent = cacheableDomain.RootContentId.HasValue ? contentRepository.Get(cacheableDomain.RootContentId.Value) : null
                };
            }

            public CacheableDomain BuildEntity(IDomain entity)
            {
                var domain = new CacheableDomain
                {
                    Id = entity.Id,
                    DefaultLanguageId = entity.Language == null ? null : (int?)entity.Language.Id,
                    DomainName = entity.DomainName,
                    RootContentId = entity.RootContent == null ? null : (int?)entity.RootContent.Id
                };
                //on initial construction we don't want to have dirty properties tracked
                // http://issues.umbraco.org/issue/U4-1946
                domain.ResetDirtyProperties(false);
                return domain;
            }

            public CacheableDomain BuildEntity(DomainDto dto)
            {
                var domain = new CacheableDomain { Id = dto.Id, DefaultLanguageId = dto.DefaultLanguage, DomainName = dto.DomainName, RootContentId = dto.RootStructureId };
                //on initial construction we don't want to have dirty properties tracked
                // http://issues.umbraco.org/issue/U4-1946
                domain.ResetDirtyProperties(false);
                return domain;
            }

            public DomainDto BuildDto(CacheableDomain entity)
            {
                var dto = new DomainDto { DefaultLanguage = entity.DefaultLanguageId, DomainName = entity.DomainName, Id = entity.Id, RootStructureId = entity.RootContentId };
                return dto;
            }
        }
    }
}