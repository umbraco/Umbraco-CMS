using System;

namespace umbraco.presentation.Web.UI
{
	/// <summary>
	/// Summary description for ContentPage.
	/// </summary>
	public class ContentPage : System.Web.UI.Page
	{
		private int _id = 0;

		public int UmbracoNodeId 
		{
			set 
			{
				_id = value;
				System.Web.HttpContext.Current.Items["pageID"] = _id;
			}
			get {return _id;}
		}
		public ContentPage()
		{
		}
	}
}
