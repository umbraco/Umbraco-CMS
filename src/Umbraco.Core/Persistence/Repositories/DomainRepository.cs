using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.FaultHandling;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    //TODO: We need to get a readonly ISO code for the domain assigned

    internal class DomainRepository : PetaPocoRepositoryBase<int, IDomain>, IDomainRepository
    {
        private readonly RepositoryCacheOptions _cacheOptions;

        public DomainRepository(IDatabaseUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax)
            : base(work, cache, logger, sqlSyntax)
        {
            //Custom cache options for better performance
            _cacheOptions = new RepositoryCacheOptions
            {
                GetAllCacheAllowZeroCount = true,
                GetAllCacheValidateCount = false
            };
        }

        /// <summary>
        /// Returns the repository cache options
        /// </summary>
        protected override RepositoryCacheOptions RepositoryCacheOptions
        {
            get { return _cacheOptions; }
        }

        protected override IDomain PerformGet(int id)
        {
            //use the underlying GetAll which will force cache all domains
            return GetAll().FirstOrDefault(x => x.Id == id);
        }

        protected override IEnumerable<IDomain> PerformGetAll(params int[] ids)
        {
            var sql = GetBaseQuery(false).Where("umbracoDomains.id > 0");
            if (ids.Any())
            {
                sql.Where("umbracoDomains.id in (@ids)", new { ids = ids });
            }

            return Database.Fetch<DomainDto>(sql).Select(ConvertFromDto);
        }

        protected override IEnumerable<IDomain> PerformGetByQuery(IQuery<IDomain> query)
        {
            throw new NotSupportedException("This repository does not support this method");
        }

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            if (isCount)
            {
                sql.Select("COUNT(*)").From<DomainDto>(SqlSyntax);
            }
            else
            {
                sql.Select("umbracoDomains.*, umbracoLanguage.languageISOCode")
                    .From<DomainDto>(SqlSyntax)
                    .LeftJoin<LanguageDto>(SqlSyntax)
                    .On<DomainDto, LanguageDto>(SqlSyntax, dto => dto.DefaultLanguage, dto => dto.Id);
            }
            
            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return "umbracoDomains.id = @Id";
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

        protected override void PersistNewItem(IDomain entity)
        {
            var exists = Database.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoDomains WHERE domainName = @domainName", new { domainName = entity.DomainName });
            if (exists > 0) throw new DuplicateNameException(string.Format("The domain name {0} is already assigned", entity.DomainName));

            if (entity.RootContentId.HasValue)
            {
                var contentExists = Database.ExecuteScalar<int>("SELECT COUNT(*) FROM cmsContent WHERE nodeId = @id", new { id = entity.RootContentId.Value });
                if (contentExists == 0) throw new NullReferenceException("No content exists with id " + entity.RootContentId.Value);
            }

            if (entity.LanguageId.HasValue)
            {
                var languageExists = Database.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoLanguage WHERE id = @id", new { id = entity.LanguageId.Value });
                if (languageExists == 0) throw new NullReferenceException("No language exists with id " + entity.LanguageId.Value);
            }

            ((UmbracoDomain)entity).AddingEntity();

            var factory = new DomainModelFactory();
            var dto = factory.BuildDto(entity);

            var id = Convert.ToInt32(Database.Insert(dto));
            entity.Id = id;

            //if the language changed, we need to resolve the ISO code!
            if (entity.LanguageId.HasValue)
            {
                ((UmbracoDomain)entity).LanguageIsoCode = Database.ExecuteScalar<string>("SELECT languageISOCode FROM umbracoLanguage WHERE id=@langId", new { langId = entity.LanguageId });
            }

            entity.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(IDomain entity)
        {
            ((UmbracoDomain)entity).UpdatingEntity();

            var exists = Database.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoDomains WHERE domainName = @domainName AND umbracoDomains.id <> @id", 
                new { domainName = entity.DomainName, id = entity.Id });
            //ensure there is no other domain with the same name on another entity
            if (exists > 0) throw new DuplicateNameException(string.Format("The domain name {0} is already assigned", entity.DomainName));

            if (entity.RootContentId.HasValue)
            {
                var contentExists = Database.ExecuteScalar<int>("SELECT COUNT(*) FROM cmsContent WHERE nodeId = @id", new { id = entity.RootContentId.Value });
                if (contentExists == 0) throw new NullReferenceException("No content exists with id " + entity.RootContentId.Value);
            }

            if (entity.LanguageId.HasValue)
            {
                var languageExists = Database.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoLanguage WHERE id = @id", new { id = entity.LanguageId.Value });
                if (languageExists == 0) throw new NullReferenceException("No language exists with id " + entity.LanguageId.Value);
            }

            var factory = new DomainModelFactory();
            var dto = factory.BuildDto(entity);

            Database.Update(dto);

            //if the language changed, we need to resolve the ISO code!
            if (entity.WasPropertyDirty("LanguageId"))
            {
                ((UmbracoDomain)entity).LanguageIsoCode = Database.ExecuteScalar<string>("SELECT languageISOCode FROM umbracoLanguage WHERE id=@langId", new {langId = entity.LanguageId});
            }

            entity.ResetDirtyProperties();
        }

        public IDomain GetByName(string domainName)
        {
            return GetAll().FirstOrDefault(x => x.DomainName.InvariantEquals(domainName));
        }

        public bool Exists(string domainName)
        {
            return GetAll().Any(x => x.DomainName.InvariantEquals(domainName));
        }

        public IEnumerable<IDomain> GetAll(bool includeWildcards)
        {
            return GetAll().Where(x => includeWildcards || x.IsWildcard == false);
        }

        public IEnumerable<IDomain> GetAssignedDomains(int contentId, bool includeWildcards)
        {
            return GetAll()
                .Where(x => x.RootContentId == contentId)
                .Where(x => includeWildcards || x.IsWildcard == false);
        }

        private IDomain ConvertFromDto(DomainDto dto)
        {
            var factory = new DomainModelFactory();
            var entity = factory.BuildEntity(dto);
            return entity;
        }      

        internal class DomainModelFactory
        {
           
            public IDomain BuildEntity(DomainDto dto)
            {
                var domain = new UmbracoDomain(dto.DomainName, dto.IsoCode)
                {
                    Id = dto.Id,
                    LanguageId = dto.DefaultLanguage,
                    RootContentId = dto.RootStructureId
                };
                //on initial construction we don't want to have dirty properties tracked
                // http://issues.umbraco.org/issue/U4-1946
                domain.ResetDirtyProperties(false);
                return domain;
            }

            public DomainDto BuildDto(IDomain entity)
            {
                var dto = new DomainDto { DefaultLanguage = entity.LanguageId, DomainName = entity.DomainName, Id = entity.Id, RootStructureId = entity.RootContentId };
                return dto;
            }
        }
    }
}