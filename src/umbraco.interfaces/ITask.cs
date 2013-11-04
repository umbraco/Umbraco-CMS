using System;

namespace umbraco.interfaces
{
	/// <summary>
	/// Summary description for ITask.
	/// </summary>
    [Obsolete("ITask is used for legacy webforms back office editors, change to using the v7 angular approach")]
	public interface ITask
	{
		int ParentID {set; get;}
		int TypeID {set; get;}
		string Alias {set; get;}
		bool Save();
		bool Delete();
		int UserId {set;}
	}
}
