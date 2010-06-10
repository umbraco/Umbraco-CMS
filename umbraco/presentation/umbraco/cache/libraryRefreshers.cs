using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace umbraco.presentation.cache
{
    public class MediaLibraryRefreshers : interfaces.ICacheRefresher
    {
        public MediaLibraryRefreshers()
        {

        }

        public Guid UniqueIdentifier
        {
            get { return new Guid("B29286DD-2D40-4DDB-B325-681226589FEC"); }
        }

        public string Name
        {
            get { return "Clears Media Cache from umbraco.library"; }
        }

        public void RefreshAll()
        {
        }

        public void Refresh(int Id)
        {
            library.ClearLibraryCacheForMedia(Id);
        }

        public void Remove(int Id)
        {
        }

        public void Refresh(Guid Id)
        {
        }

    }

    public class MemberLibraryRefreshers : interfaces.ICacheRefresher
    {
        public MemberLibraryRefreshers()
        {

        }

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

        public void Refresh(int Id)
        {
            library.ClearLibraryCacheForMember(Id);
        }

        public void Remove(int Id)
        {
        }

        public void Refresh(Guid Id)
        {
        }

    }
}