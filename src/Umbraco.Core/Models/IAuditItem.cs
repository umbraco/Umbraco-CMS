using System;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    public interface IAuditItem : IEntity
    {
        string Comment { get; }
        AuditType AuditType { get; }
        int UserId { get; }
    }
}
