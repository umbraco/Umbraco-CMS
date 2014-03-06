using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Relators;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents the UserRepository for doing CRUD operations for <see cref="IUser"/>
    /// </summary>
    internal class UserRepository : PetaPocoRepositoryBase<int, IUser>, IUserRepository
    {
        private readonly IUserTypeRepository _userTypeRepository;
        private readonly CacheHelper _cacheHelper;

        public UserRepository(IDatabaseUnitOfWork work, IUserTypeRepository userTypeRepository, CacheHelper cacheHelper)
            : base(work)
        {
            _userTypeRepository = userTypeRepository;
            _cacheHelper = cacheHelper;
        }

        public UserRepository(IDatabaseUnitOfWork work, IRepositoryCacheProvider cache, IUserTypeRepository userTypeRepository, CacheHelper cacheHelper)
            : base(work, cache)
        {
            _userTypeRepository = userTypeRepository;
            _cacheHelper = cacheHelper;
        }

        #region Overrides of RepositoryBase<int,IUser>

        protected override IUser PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id });

            var dto = Database.Fetch<UserDto, User2AppDto, UserDto>(new UserSectionRelator().Map, sql).FirstOrDefault();
            
            if (dto == null)
                return null;

            var userType = _userTypeRepository.Get(dto.Type);
            var userFactory = new UserFactory(userType);
            var user = userFactory.BuildEntity(dto);

            return user;
        }

        protected override IEnumerable<IUser> PerformGetAll(params int[] ids)
        {
            if (ids.Any())
            {
                return PerformGetAllOnIds(ids);
            }

            var sql = GetBaseQuery(false);

            return ConvertFromDtos(Database.Fetch<UserDto, User2AppDto, UserDto>(new UserSectionRelator().Map, sql))
                .ToArray(); // important so we don't iterate twice, if we don't do thsi we can end up with null vals in cache if we were caching.
        }

        private IEnumerable<IUser> PerformGetAllOnIds(params int[] ids)
        {
            if (ids.Any() == false) yield break;
            foreach (var id in ids)
            {
                yield return Get(id);
            }
        }

        protected override IEnumerable<IUser> PerformGetByQuery(IQuery<IUser> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IUser>(sqlClause, query);
            var sql = translator.Translate();

            var dtos = Database.Fetch<UserDto, User2AppDto, UserDto>(new UserSectionRelator().Map, sql);

            foreach (var dto in dtos.DistinctBy(x => x.Id))
            {
                yield return Get(dto.Id);
            }
        }
        
        #endregion

        #region Overrides of PetaPocoRepositoryBase<int,IUser>
        
        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            if (isCount)
            {
                sql.Select("COUNT(*)").From<UserDto>();
            }
            else
            {
                return GetBaseQuery("*");
            }
            return sql;
        }

        private static Sql GetBaseQuery(string columns)
        {
            var sql = new Sql();
            sql.Select(columns)
                      .From<UserDto>()
                      .LeftJoin<User2AppDto>()
                      .On<UserDto, User2AppDto>(left => left.Id, right => right.UserId);
            return sql;
        }


        protected override string GetBaseWhereClause()
        {
            return "umbracoUser.id = @Id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {            
            var list = new List<string>
                           {
                               "DELETE FROM cmsTask WHERE userId = @Id",
                               "DELETE FROM cmsTask WHERE parentUserId = @Id",
                               "DELETE FROM umbracoUser2NodePermission WHERE userId = @Id",
                               "DELETE FROM umbracoUser2NodeNotify WHERE userId = @Id",
                               "DELETE FROM umbracoUserLogins WHERE userId = @Id",
                               "DELETE FROM umbracoUser2app WHERE " + SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("user") + "=@Id",
                               "DELETE FROM umbracoUser WHERE id = @Id"
                           };
            return list;
        }

        protected override Guid NodeObjectTypeId
        {
            get { throw new NotImplementedException(); }
        }
        
        protected override void PersistNewItem(IUser entity)
        {
            var userFactory = new UserFactory(entity.UserType);
            var userDto = userFactory.BuildDto(entity);

            var id = Convert.ToInt32(Database.Insert(userDto));
            entity.Id = id;

            foreach (var sectionDto in userDto.User2AppDtos)
            {
                //need to set the id explicitly here
                sectionDto.UserId = id;
                Database.Insert(sectionDto);
            }

            ((ICanBeDirty)entity).ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(IUser entity)
        {
            var userFactory = new UserFactory(entity.UserType);
            var userDto = userFactory.BuildDto(entity);

            Database.Update(userDto);

            //update the sections if they've changed
            var user = (User)entity;
            if (user.IsPropertyDirty("AllowedSections"))
            {
                //for any that exist on the object, we need to determine if we need to update or insert
                foreach (var sectionDto in userDto.User2AppDtos)
                {
                    if (user.AddedSections.Contains(sectionDto.AppAlias))
                    {
                        //we need to insert since this was added  
                        Database.Insert(sectionDto);
                    }
                    else
                    {
                        //we need to manually update this record because it has a composite key
                        Database.Update<User2AppDto>("SET app=@Section WHERE app=@Section AND " + SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("user") + "=@UserId",
                                                     new { Section = sectionDto.AppAlias, UserId = sectionDto.UserId });
                    }
                }

                //now we need to delete any applications that have been removed
                foreach (var section in user.RemovedSections)
                {
                    //we need to manually delete thsi record because it has a composite key
                    Database.Delete<User2AppDto>("WHERE app=@Section AND " + SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("user") + "=@UserId",
                        new { Section = section, UserId = (int)user.Id });
                }
            }

            ((ICanBeDirty)entity).ResetDirtyProperties();
        }

        #endregion

        #region Implementation of IUserRepository

        public int GetCountByQuery(IQuery<IUser> query)
        {
            var sqlClause = GetBaseQuery("umbracoUser.id");
            var translator = new SqlTranslator<IUser>(sqlClause, query);
            var subquery = translator.Translate();
            //get the COUNT base query
            var sql = GetBaseQuery(true)
                .Append(new Sql("WHERE umbracoUser.id IN (" + subquery.SQL + ")", subquery.Arguments));

            return Database.ExecuteScalar<int>(sql);
        }

        public bool Exists(string username)
        {
            var sql = new Sql();
            var escapedUserName = PetaPocoExtensions.EscapeAtSymbols(username);
            sql.Select("COUNT(*)")
                .From<UserDto>()
                .Where<UserDto>(x => x.UserName == escapedUserName);

            return Database.ExecuteScalar<int>(sql) > 0;
        }

        public IEnumerable<IUser> GetUsersAssignedToSection(string sectionAlias)
        {
            //Here we're building up a query that looks like this, a sub query is required because the resulting structure
            // needs to still contain all of the section rows per user.

            //SELECT *
            //FROM [umbracoUser]
            //LEFT JOIN [umbracoUser2app]
            //ON [umbracoUser].[id] = [umbracoUser2app].[user]
            //WHERE umbracoUser.id IN (SELECT umbracoUser.id
            //    FROM [umbracoUser]
            //    LEFT JOIN [umbracoUser2app]
            //    ON [umbracoUser].[id] = [umbracoUser2app].[user]
            //    WHERE umbracoUser2app.app = 'content')

            var sql = GetBaseQuery(false);
            var innerSql = GetBaseQuery("umbracoUser.id");
            innerSql.Where("umbracoUser2app.app = " + SqlSyntaxContext.SqlSyntaxProvider.GetQuotedValue(sectionAlias));
            sql.Where(string.Format("umbracoUser.id IN ({0})", innerSql.SQL));

            return ConvertFromDtos(Database.Fetch<UserDto, User2AppDto, UserDto>(new UserSectionRelator().Map, sql));
        }

        /// <summary>
        /// Gets paged user results
        /// </summary>
        /// <param name="query">
        /// The where clause, if this is null all records are queried
        /// </param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        /// <remarks>
        /// The query supplied will ONLY work with data specifically on the umbracoUser table because we are using PetaPoco paging (SQL paging)
        /// </remarks>
        public IEnumerable<IUser> GetPagedResultsByQuery(IQuery<IUser> query, int pageIndex, int pageSize, out int totalRecords, Expression<Func<IUser, string>> orderBy)
        {
            if (orderBy == null) throw new ArgumentNullException("orderBy");

            var sql = new Sql();
            sql.Select("*").From<UserDto>();

            Sql resultQuery;
            if (query != null)
            {
                var translator = new SqlTranslator<IUser>(sql, query);
                resultQuery = translator.Translate();
            }
            else
            {
                resultQuery = sql;
            }

            //get the referenced column name
            var expressionMember = ExpressionHelper.GetMemberInfo(orderBy);
            //now find the mapped column name
            var mapper = MappingResolver.Current.ResolveMapperByType(typeof(IUser));
            var mappedField = mapper.Map(expressionMember.Name);
            if (mappedField.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("Could not find a mapping for the column specified in the orderBy clause");
            }
            //need to ensure the order by is in brackets, see: https://github.com/toptensoftware/PetaPoco/issues/177
            resultQuery.OrderBy(string.Format("({0})", mappedField));

            var pagedResult = Database.Page<UserDto>(pageIndex + 1, pageSize, resultQuery);

            totalRecords = Convert.ToInt32(pagedResult.TotalItems);

            //now that we have the user dto's we need to construct true members from the list.
            if (totalRecords == 0)
            {
                return Enumerable.Empty<IUser>();
            }

            var result = GetAll(pagedResult.Items.Select(x => x.Id).ToArray());

            //now we need to ensure this result is also ordered by the same order by clause
            return result.OrderBy(orderBy.Compile());
        }
        
        /// <summary>
        /// Returns permissions for a given user for any number of nodes
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="entityIds"></param>
        /// <returns></returns>        
        public IEnumerable<EntityPermission> GetUserPermissionsForEntities(int userId, params int[] entityIds)
        {
            var repo = new PermissionRepository<IContent>(UnitOfWork, _cacheHelper);
            return repo.GetUserPermissionsForEntities(userId, entityIds);
        }

        /// <summary>
        /// Replaces the same permission set for a single user to any number of entities
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="permissions"></param>
        /// <param name="entityIds"></param>
        public void ReplaceUserPermissions(int userId, IEnumerable<char> permissions, params int[] entityIds)
        {
            var repo = new PermissionRepository<IContent>(UnitOfWork, _cacheHelper);
            repo.ReplaceUserPermissions(userId, permissions, entityIds);
        }

        #endregion

        private IEnumerable<IUser> ConvertFromDtos(IEnumerable<UserDto> dtos)
        {
            var foundUserTypes = new Dictionary<short, IUserType>();
            return dtos.Select(dto =>
                {
                    //first we need to get the user type
                    IUserType userType;
                    if (foundUserTypes.ContainsKey(dto.Type))
                    {
                        userType = foundUserTypes[dto.Type];
                    }
                    else
                    {
                        userType = _userTypeRepository.Get(dto.Type);
                        //put it in the local cache
                        foundUserTypes.Add(dto.Type, userType);
                    }

                    var userFactory = new UserFactory(userType);
                    return userFactory.BuildEntity(dto);
                });
        } 
    }
}