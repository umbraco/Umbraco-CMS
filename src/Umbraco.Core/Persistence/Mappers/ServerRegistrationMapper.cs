using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Mappers
{
    internal sealed class ServerRegistrationMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache = new ConcurrentDictionary<string, DtoMapModel>();

        internal static readonly ServerRegistrationMapper Instance = new ServerRegistrationMapper();

        private ServerRegistrationMapper()
        {
            BuildMap();
        }

        #region Overrides of BaseMapper

        internal override void BuildMap()
        {
            CacheMap<ServerRegistration, ServerRegistrationDto>(src => src.Id, dto => dto.Id);
            CacheMap<ServerRegistration, ServerRegistrationDto>(src => src.IsActive, dto => dto.IsActive);
            CacheMap<ServerRegistration, ServerRegistrationDto>(src => src.ServerAddress, dto => dto.Address);
            CacheMap<ServerRegistration, ServerRegistrationDto>(src => src.CreateDate, dto => dto.DateRegistered);
            CacheMap<ServerRegistration, ServerRegistrationDto>(src => src.UpdateDate, dto => dto.LastNotified);
        }

        internal override string Map(string propertyName)
        {
            if (!PropertyInfoCache.ContainsKey(propertyName))
                return string.Empty;

            var dtoTypeProperty = PropertyInfoCache[propertyName];

            return base.GetColumnName(dtoTypeProperty.Type, dtoTypeProperty.PropertyInfo);
        }

        internal override void CacheMap<TSource, TDestination>(Expression<Func<TSource, object>> sourceMember, Expression<Func<TDestination, object>> destinationMember)
        {
            var property = base.ResolveMapping(sourceMember, destinationMember);
            PropertyInfoCache.AddOrUpdate(property.SourcePropertyName, property, (x, y) => property);
        }

        #endregion
    }
}