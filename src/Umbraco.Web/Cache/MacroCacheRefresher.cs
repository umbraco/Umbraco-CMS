using System;
using umbraco;
using umbraco.interfaces;

namespace Umbraco.Web.Cache
{
    public class MacroCacheRefresher : ICacheRefresher
    {
        public string Name
        {
            get
            {
                return "Macro cache refresher";
            }
        }

        public Guid UniqueIdentifier
        {
            get
            {
                return new Guid("7B1E683C-5F34-43dd-803D-9699EA1E98CA");
            }
        }

        public void RefreshAll()
        {
        }

        public void Refresh(Guid id)
        {
        }

        void ICacheRefresher.Refresh(int id)
        {
            macro.GetMacro(id).removeFromCache();
        }

        void ICacheRefresher.Remove(int id)
        {
            macro.GetMacro(id).removeFromCache();
        }

    }
}