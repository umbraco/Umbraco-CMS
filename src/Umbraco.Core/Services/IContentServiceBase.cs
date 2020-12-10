using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public interface IContentServiceBase<TItem> : IContentServiceBase
        where TItem: class, IContentBase
    {
        TItem GetById(Guid key);
        Attempt<OperationResult> Save(IEnumerable<TItem> contents, int userId = Constants.Security.SuperUserId, bool raiseEvents = true);
    }

    /// <summary>
    /// Placeholder for sharing logic between the content, media (and member) services
    /// TODO: Start sharing the logic!
    /// </summary>
    public interface IContentServiceBase : IService
    {
        /// <summary>
        /// Checks/fixes the data integrity of node paths/levels stored in the database
        /// </summary>
        ContentDataIntegrityReport CheckDataIntegrity(ContentDataIntegrityReportOptions options);
    }
}
