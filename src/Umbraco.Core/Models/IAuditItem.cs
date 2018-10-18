using System;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    public interface IAuditItem : IEntity
    {
        string Comment { get; }

        /// <summary>
        /// The entity type for the log entry
        /// </summary>
        string EntityType { get; }

        /// <summary>
        /// Optional additional data parameters for the log
        /// </summary>
        string Parameters { get; }
        AuditType AuditType { get; }
        int UserId { get; }
    }
}
