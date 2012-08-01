using System;

namespace umbraco.interfaces
{
	/// <summary>
	/// Summary description for ITask.
	/// </summary>
	public interface ITask
	{
		int ParentID {set; get;}
		int TypeID {set; get;}
		string Alias {set; get;}
		bool Save();
		bool Delete();
		int UserId {set;}
	}

	public interface ITaskReturnUrl : ITask
	{
		string ReturnUrl {get;}
	}
}
