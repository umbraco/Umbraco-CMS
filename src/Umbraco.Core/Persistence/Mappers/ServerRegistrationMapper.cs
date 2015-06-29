using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Mappers
{
    [MapperFor(typeof(ServerRegistration))]
    public sealed class ServerRegistrationMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();

        //NOTE: its an internal class but the ctor must be public since we're using Activator.CreateInstance to create it
        // otherwise that would fail because there is no public constructor.
        public ServerRegistrationMapper()
        {
            BuildMap();
        }

        #region Overrides of BaseMapper

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache
        {
            get { return PropertyInfoCacheInstance; }
        }

        internal override void BuildMap()
        {
            CacheMap<ServerRegistration, ServerRegistrationDto>(src => src.Id, dto => dto.Id);
            CacheMap<ServerRegistration, ServerRegistrationDto>(src => src.IsActive, dto => dto.IsActive);
            CacheMap<ServerRegistration, ServerRegistrationDto>(src => src.ServerAddress, dto => dto.Address);
            CacheMap<ServerRegistration, ServerRegistrationDto>(src => src.CreateDate, dto => dto.DateRegistered);
            CacheMap<ServerRegistration, ServerRegistrationDto>(src => src.UpdateDate, dto => dto.LastNotified);
            CacheMap<ServerRegistration, ServerRegistrationDto>(src => src.ComputerName, dto => dto.ComputerName);
        }

        #endregion
    }
}