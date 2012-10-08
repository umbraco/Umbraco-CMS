using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    internal class MacroRepository : RepositoryBase<string, IMacro>, IMacroRepository
    {
        public MacroRepository(IUnitOfWork work) : base(work)
        {
        }

        public MacroRepository(IUnitOfWork work, IRepositoryCacheProvider cache) : base(work, cache)
        {
        }

        #region Overrides of RepositoryBase<string,IMacro>

        protected override IMacro PerformGet(string id)
        {
            throw new System.NotImplementedException();
        }

        protected override IEnumerable<IMacro> PerformGetAll(params string[] ids)
        {
            throw new System.NotImplementedException();
        }

        protected override IEnumerable<IMacro> PerformGetByQuery(IQuery<IMacro> query)
        {
            throw new System.NotImplementedException();
        }

        protected override bool PerformExists(string id)
        {
            throw new System.NotImplementedException();
        }

        protected override int PerformCount(IQuery<IMacro> query)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region Overrides of IUnitOfWorkRepository

        protected override void PersistNewItem(IMacro item)
        {
            throw new System.NotImplementedException();
        }

        protected override void PersistUpdatedItem(IMacro item)
        {
            throw new System.NotImplementedException();
        }

        protected override void PersistDeletedItem(IMacro item)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}