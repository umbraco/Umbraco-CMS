using System;

namespace umbraco.interfaces
{
	/// <summary>
	/// Summary description for IDataFieldWithButtons.
	/// </summary>
	public interface IDataFieldWithButtons : IDataEditor
	{
		object[] MenuIcons {get;}
	}
}
