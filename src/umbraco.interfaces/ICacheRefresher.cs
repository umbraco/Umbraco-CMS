using System;

namespace umbraco.interfaces
{

    /// <summary>
    /// The IcacheRefresher Interface is used for loadbalancing.
    /// 
    /// </summary>
    public interface ICacheRefresher
    {
        Guid UniqueIdentifier { get; }
        string Name { get; }
        void RefreshAll();
        void Refresh(int Id);
        void Remove(int Id);
        void Refresh(Guid Id);

        //void Notify(object payload);
    }

}
