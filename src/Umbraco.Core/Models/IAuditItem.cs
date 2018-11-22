using System;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    public interface IAuditItem : IAggregateRoot
    {
        string Comment { get; }
        AuditType AuditType { get; }
        int UserId { get; }
    }
}