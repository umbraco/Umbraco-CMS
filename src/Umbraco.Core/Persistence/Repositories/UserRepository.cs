using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
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

            var dto = Database.FirstOrDefault<UserDto>(sql);

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
                foreach (var id in ids)
                {
                    yield return Get(id);
                }
            }
            else
            {
                var userDtos = Database.Fetch<UserDto>("WHERE id >= 0");
                foreach (var userDto in userDtos)
                {
                    yield return Get(userDto.Id);
                }
            }
        }

        protected override IEnumerable<IUser> PerformGetByQuery(IQuery<IUser> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IUser>(sqlClause, query);
            var sql = translator.Translate();

            var dtos = Database.Fetch<UserDto>(sql);

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
            sql.Select(isCount ? "COUNT(*)" : "*")
               .From<UserDto>();
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

        }

        protected override void PersistUpdatedItem(IUser entity)
        {
            var userFactory = new UserFactory(entity.UserType);
            var userDto = userFactory.BuildDto(entity);

            Database.Update(userDto);
        }

        #endregion

        #region Implementation of IUserRepository

        public IProfile GetProfileById(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id });

            var dto = Database.FirstOrDefault<UserDto>(sql);

            if (dto == null)
                return null;

            return new Profile(dto.Id, dto.UserName);
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
    }
}