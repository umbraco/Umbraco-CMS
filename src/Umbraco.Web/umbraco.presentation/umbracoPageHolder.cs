using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Text;
using System.Collections;

namespace umbraco.layoutControls
{
	/// <summary>
	/// Summary description for umbracoPageHolder.
	/// </summary>
	[DefaultProperty("Text"), 
		ToolboxData("<{0}:umbracoPageHolder runat=server></{0}:umbracoPageHolder>")]
	public class umbracoPageHolder : System.Web.UI.WebControls.PlaceHolder
	{
		#region private properties

		private String _pageName;
		private String _writerName;
		private DateTime _createDate;
		private DateTime _updateDate;
		private int _pageID;
		private int _pageVersion;
		private int _templateID;
		private Hashtable _elements = new Hashtable();
		private StringBuilder _pageContent = new StringBuilder();

		#endregion

		[Bindable(true), Category("umbraco"), DefaultValue("")] 
		public string PageName 
		{
			get {return _pageName;}
			set {_pageName = value;}
		}

		[Bindable(true), Category("umbraco"), DefaultValue("")] 
		public string WriterName 
		{
			get {return _writerName;}
			set {_writerName = value;}
		}
		[Bindable(true), Category("umbraco"), DefaultValue("")] 
		public DateTime CreateDate
		{
			get {return _createDate;}
			set {_createDate = value;}
		}
		[Bindable(true), Category("umbraco"), DefaultValue("")] 
		public DateTime updateDate 
		{
			get {return _updateDate;}
			set {_updateDate = value;}
		}
		[Bindable(true), Category("umbraco"), DefaultValue("")] 
		public int pageID
		{
			get {return _pageID;}
			set {_pageID = value;}
		}
		[Bindable(true), Category("umbraco"), DefaultValue("")] 
		public int pageVersion
		{
			get {return _pageVersion;}
			set {_pageVersion = value;}
		}
		[Bindable(true), Category("umbraco"), DefaultValue("")] 
		public int templateID
		{
			get {return _templateID;}
			set {_templateID = value;}
		}
		[Bindable(true), Category("umbraco"), DefaultValue("")] 
		public Hashtable Elements
		{
			get {return _elements;}
			set {_elements = value;}
		}
		[Bindable(true), Category("umbraco"), DefaultValue("")] 
		public StringBuilder PageContent
		{
			get {return _pageContent;}
			set {_pageContent = value;}
		}

		public void Populate(page Page) {
			if (Page != null) 
			{
				_pageID = Page.PageID;
				_templateID = Page.Template;

				_createDate = Page.CreateDate;
				_updateDate = Page.UpdateDate;
				
				_writerName = Page.WriterName;
				_pageName = Page.PageName;
				_elements = Page.Elements;

				_pageContent.Append(Page.PageContent);
				this.Controls.Add(Page.PageContentControl);
			}
		}

		/// <summary> 
		/// Render this control to the output parameter specified.
		/// </summary>
		/// <param name="output"> The HTML writer to write out to </param>
		protected override void Render(HtmlTextWriter output)
		{
			base.Render(output);
		}
	}
}
