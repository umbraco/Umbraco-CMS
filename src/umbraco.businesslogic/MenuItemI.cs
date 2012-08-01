using System;

namespace umbraco.BusinessLogic.console
{
	/// <summary>
	/// Summary description for MenuItemI.
	/// </summary>
    [Obsolete("this is not used anywhere")]
	public interface MenuItemI
	{
		EditorBehavior behavior {get;}
		string EditorURL {get;}
		string Text {get;}
	}
	
    [Obsolete("this is not used anywhere")]
	public enum EditorBehavior {
		modal, external, standard, command	
	}
}
