using System;

namespace umbraco.cms.businesslogic.stat
{
	/// <summary>
	/// Summary description for Entry.
	/// </summary>
	public class Entry
	{
		private int _currentPage;
		private int _lastPage;
		private System.DateTime _entryTime;

		public Entry(int CurrentPage, int LastPage)
		{
			_entryTime = System.DateTime.Now;
			_currentPage = CurrentPage;
			_lastPage = LastPage;
		}

		public int CurrentPage 
		{
			get {return _currentPage;}
			set {_currentPage = value;}
		}
		public int LastPage 
		{
			get {return _lastPage;}
			set {_lastPage = value;}
		}
		public DateTime EntryTime
		{
			get {return _entryTime;}
			set {_entryTime = value;}
		}

	}
}
