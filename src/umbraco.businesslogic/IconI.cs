using System;

namespace umbraco.BusinessLogic.console
{
	[Obsolete("REMOVE THIS ASAP!! what is it??")]
	public interface IconI
	{
		Guid UniqueId{get;}
		int Id{get;}
		
		IconI[] Children {get;}
		string DefaultEditorURL{get;}
		string Text{get;set;}
		string OpenImage {get;}
		string Image {get;}
	}
}