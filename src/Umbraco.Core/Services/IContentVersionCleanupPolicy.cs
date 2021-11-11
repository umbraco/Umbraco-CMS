using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Used to filter historic content versions for cleanup.
    /// </summary>
    public interface IContentVersionCleanupPolicy
    {
        /// <summary>
        /// Filters a set of candidates historic content versions for cleanup according to policy settings.
        /// </summary>
        IEnumerable<ContentVersionMeta> Apply(DateTime asAtDate, IEnumerable<ContentVersionMeta> items);
    }
}
