using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    internal interface IPartialViewMacroRepository : IPartialViewRepository
    {
        /// <summary>
        /// Adds or Updates an associated macro
        /// </summary>
        /// <param name="macro"></param>
        void AddOrUpdate(IMacro macro);
    }
}