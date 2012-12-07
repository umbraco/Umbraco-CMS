using System;
using umbraco.interfaces;

namespace umbraco
{
	public class macroCacheRefresh : ICacheRefresher
	{
		#region ICacheRefresher Members

		public string Name
		{
			get
			{
				// TODO:  Add templateCacheRefresh.Name getter implementation
				return "Macro cache refresher";
			}
		}

		public Guid UniqueIdentifier
		{
			get
			{
				// TODO:  Add templateCacheRefresh.UniqueIdentifier getter implementation
				return new Guid("7B1E683C-5F34-43dd-803D-9699EA1E98CA");
			}
		}

		public void RefreshAll()
		{
		}

		public void Refresh(Guid Id)
		{
			// Doesn't do anything
		}

		void ICacheRefresher.Refresh(int Id)
		{
			macro.GetMacro(Id).removeFromCache();
		}

		void ICacheRefresher.Remove(int Id)
		{
			macro.GetMacro(Id).removeFromCache();
		}

		#endregion
	}
}