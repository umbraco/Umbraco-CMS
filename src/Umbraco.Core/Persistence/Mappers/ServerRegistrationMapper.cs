﻿using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Mappers
{
    [MapperFor(typeof(ServerRegistration))]
    [MapperFor(typeof(IServerRegistration))]
    internal sealed class ServerRegistrationMapper : BaseMapper
    {
        private static readonly ConcurrentDictionary<string, DtoMapModel> PropertyInfoCacheInstance = new ConcurrentDictionary<string, DtoMapModel>();

        #region Overrides of BaseMapper

        public ServerRegistrationMapper(ISqlSyntaxProvider sqlSyntax)
            : base(sqlSyntax)
        {
        }

        internal override ConcurrentDictionary<string, DtoMapModel> PropertyInfoCache
        {
            get { return PropertyInfoCacheInstance; }
        }

        protected override void BuildMap()
        {
            CacheMap<ServerRegistration, ServerRegistrationDto>(src => src.Id, dto => dto.Id);
            CacheMap<ServerRegistration, ServerRegistrationDto>(src => src.IsActive, dto => dto.IsActive);
            CacheMap<ServerRegistration, ServerRegistrationDto>(src => src.IsMaster, dto => dto.IsMaster);
            CacheMap<ServerRegistration, ServerRegistrationDto>(src => src.ServerAddress, dto => dto.ServerAddress);
            CacheMap<ServerRegistration, ServerRegistrationDto>(src => src.CreateDate, dto => dto.DateRegistered);
            CacheMap<ServerRegistration, ServerRegistrationDto>(src => src.UpdateDate, dto => dto.DateAccessed);
            CacheMap<ServerRegistration, ServerRegistrationDto>(src => src.ServerIdentity, dto => dto.ServerIdentity);
        }

        #endregion
    }
}