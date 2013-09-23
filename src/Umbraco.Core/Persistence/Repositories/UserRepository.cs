using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Factories;
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

        public UserRepository(IDatabaseUnitOfWork work, IUserTypeRepository userTypeRepository)
            : base(work)
        {
            _userTypeRepository = userTypeRepository;
        }

        public UserRepository(IDatabaseUnitOfWork work, IRepositoryCacheProvider cache, IUserTypeRepository userTypeRepository)
            : base(work, cache)
        {
            _userTypeRepository = userTypeRepository;
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
        /// Returns permissions for a given user for any number of nodes
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="entityIds"></param>
        /// <returns></returns>        
        public IEnumerable<EntityPermission> GetUserPermissionsForEntities(object userId, params int[] entityIds)
        {
            var repo = new PermissionRepository<IContent>(UnitOfWork);
            return repo.GetUserPermissionsForEntities(userId, entityIds);
        }

        public IProfile GetProfileByUserName(string username)
        {
            var sql = GetBaseQuery(false);
            sql.Where("umbracoUser.userLogin = @Username", new { Username = username });

            var dto = Database.FirstOrDefault<UserDto>(sql);

            if (dto == null)
                return null;

            return new Profile(dto.Id, dto.UserName);
        }

        public IUser GetUserByUserName(string username)
        {
            var sql = GetBaseQuery(false);
            sql.Where("umbracoUser.userLogin = @Username", new { Username = username });

            var dto = Database.FirstOrDefault<UserDto>(sql);
            
            if (dto == null)
                return null;

            return new User(_userTypeRepository.Get(dto.Type))
                {
                    Id = dto.Id,
                    Email = dto.Email,
                    Language = dto.UserLanguage,
                    Name = dto.UserName,
                    NoConsole = dto.NoConsole,
                    IsLockedOut = dto.Disabled
                };

        }

        public IUser GetUserById(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id });

            var dto = Database.FirstOrDefault<UserDto>(sql);

            if (dto == null)
                return null;

            return new User(_userTypeRepository.Get(dto.Type))
            {
                Id = dto.Id,
                Email = dto.Email,
                Language = dto.UserLanguage,
                Name = dto.UserName,
                NoConsole = dto.NoConsole,
                IsLockedOut = dto.Disabled
            };

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