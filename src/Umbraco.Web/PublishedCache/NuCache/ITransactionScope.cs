using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.PublishedCache.NuCache
{
    /// <summary>
    /// Transaction Scope for NuCache Operations
    /// </summary>
    public interface ITransactionScope : IDisposable
    {
        /// <summary>
        /// Commit transaction
        /// </summary>
        void Commit();
        /// <summary>
        /// Rollback transaction
        /// </summary>
        void Rollback();
    }
}
