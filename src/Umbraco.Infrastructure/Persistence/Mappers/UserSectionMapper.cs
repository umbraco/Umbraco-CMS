namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;


// [MapperFor(typeof(UserSection))]
// public sealed class UserSectionMapper : BaseMapper
// {
//    private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();

// //NOTE: its an internal class but the ctor must be public since we're using Activator.CreateInstance to create it
//    // otherwise that would fail because there is no public constructor.
//    public UserSectionMapper()
//    {
//        BuildMap();
//    }

// #region Overrides of BaseMapper

// internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache
//    {
//        get { return PropertyInfoCacheInstance; }
//    }

// internal override void BuildMap()
//    {
//        CacheMap<UserSection, User2AppDto>(src => src.UserId, dto => dto.UserId);
//        CacheMap<UserSection, User2AppDto>(src => src.SectionAlias, dto => dto.AppAlias);
//    }

// #endregion
// }
