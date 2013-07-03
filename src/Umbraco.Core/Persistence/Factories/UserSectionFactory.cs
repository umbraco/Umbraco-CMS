using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class UserSectionFactory : IEntityFactory<UserSection, User2AppDto>
    {
        #region Implementation of IEntityFactory<Language,LanguageDto>

        public UserSection BuildEntity(User2AppDto dto)
        {
            return Mapper.Map<User2AppDto, UserSection>(dto);
        }

        public User2AppDto BuildDto(UserSection entity)
        {
            return Mapper.Map<UserSection, User2AppDto>(entity);
        }

        #endregion

        /// <summary>
        /// Sets up the automapper mappings
        /// </summary>
        /// <param name="config"></param>
        internal static void ConfigureMappings(IConfiguration config)
        {
            config.CreateMap<User2AppDto, UserSection>()
                  .ForMember(section => section.UserId, expression => expression.MapFrom(dto => dto.UserId))
                  .ForMember(section => section.SectionAlias, expression => expression.MapFrom(dto => dto.AppAlias));

            config.CreateMap<UserSection, User2AppDto>()
                  .ForMember(dto => dto.UserId, expression => expression.MapFrom(section => section.UserId))
                  .ForMember(dto => dto.AppAlias, expression => expression.MapFrom(section => section.SectionAlias));
        }
    }
}