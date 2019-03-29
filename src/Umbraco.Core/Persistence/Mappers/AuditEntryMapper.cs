using System;
using System.Collections.Concurrent;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Mappers
{
    /// <summary>
    /// Represents a mapper for audit entry entities.
    /// </summary>
    [MapperFor(typeof(IAuditEntry))]
    [MapperFor(typeof(AuditEntry))]
    public sealed class AuditEntryMapper : BaseMapper
    {
        public AuditEntryMapper(ISqlContext sqlContext, ConcurrentDictionary<Type, ConcurrentDictionary<string, string>> maps)
            : base(sqlContext, maps)
        {
            DefineMap<AuditEntry, AuditEntryDto>(nameof(AuditEntry.Id), nameof(AuditEntryDto.Id));
            DefineMap<AuditEntry, AuditEntryDto>(nameof(AuditEntry.PerformingUserId), nameof(AuditEntryDto.PerformingUserId));
            DefineMap<AuditEntry, AuditEntryDto>(nameof(AuditEntry.PerformingDetails), nameof(AuditEntryDto.PerformingDetails));
            DefineMap<AuditEntry, AuditEntryDto>(nameof(AuditEntry.PerformingIp), nameof(AuditEntryDto.PerformingIp));
            DefineMap<AuditEntry, AuditEntryDto>(nameof(AuditEntry.EventDateUtc), nameof(AuditEntryDto.EventDateUtc));
            DefineMap<AuditEntry, AuditEntryDto>(nameof(AuditEntry.AffectedUserId), nameof(AuditEntryDto.AffectedUserId));
            DefineMap<AuditEntry, AuditEntryDto>(nameof(AuditEntry.AffectedDetails), nameof(AuditEntryDto.AffectedDetails));
            DefineMap<AuditEntry, AuditEntryDto>(nameof(AuditEntry.EventType), nameof(AuditEntryDto.EventType));
            DefineMap<AuditEntry, AuditEntryDto>(nameof(AuditEntry.EventDetails), nameof(AuditEntryDto.EventDetails));
        }
    }
}
