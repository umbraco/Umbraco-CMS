using System;

namespace umbraco.BusinessLogic.console
{
	/// <summary>
	/// Summary description for MenuItemI.
	/// </summary>
	public interface MenuItemI
	{
		EditorBehavior behavior {get;}
		string EditorURL {get;}
		string Text {get;}
	}
	
	public enum EditorBehavior {
		modal, external, standard, command	
	}
}
