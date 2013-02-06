using System;
using Umbraco.Core;
using umbraco.interfaces;

namespace Umbraco.Web.Cache
{
    public class MemberLibraryRefreshers : ICacheRefresher
    {

        public Guid UniqueIdentifier
        {
            get { return new Guid("E285DF34-ACDC-4226-AE32-C0CB5CF388DA"); }
        }

        public string Name
        {
            get { return "Clears Member Cache from umbraco.library"; }
        }

        public void RefreshAll()
        {
        }

        public void Refresh(int id)
        {
            ApplicationContext.Current.ApplicationCache.ClearLibraryCacheForMember(id, false);
        }

        public void Remove(int id)
        {
        }

        public void Refresh(Guid id)
        {
        }

    }
}