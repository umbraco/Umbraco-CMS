using System;

namespace umbraco.controls.GenericProperties
{
	/// <summary>
	/// Summary description for GenericPropertyWrapper.
	/// </summary>
	public class GenericPropertyWrapper : System.Web.UI.WebControls.PlaceHolder
	{

		private GenericProperty _gp; 
		private cms.businesslogic.propertytype.PropertyType _pt;
		private cms.businesslogic.ContentType.TabI[] _tabs;
		private cms.businesslogic.datatype.DataTypeDefinition[] _dtds;
		private int _tabId;
		private string _fullId = "";

		public event System.EventHandler Delete;

		public cms.businesslogic.propertytype.PropertyType PropertyType 
		{
			set {_pt = value;}
			get {return _pt;}
		}
		public int TabId 
		{
			set {_tabId = value;}
		}

		public cms.businesslogic.datatype.DataTypeDefinition[] DataTypeDefinitions 
		{
			set 
			{
				_dtds = value;
			}
		}
		public cms.businesslogic.web.DocumentType.TabI[] Tabs 
		{
			set 
			{
				_tabs = value;
			}
		}

		public string FullId 
		{
			set 
			{
				_fullId = value;
			}
		}

		public GenericProperty GenricPropertyControl 
		{
			get 
			{
				return _gp;
			}
		}

		public GenericPropertyWrapper()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public void UpdateEditControl() 
		{
			if (this.Controls.Count == 1) 
			{
				System.Web.UI.Control u = this.Controls[0];
				u.ID = this.ID + "_control";
				_gp = (GenericProperty) u;
				_gp.PropertyType = _pt;
				_gp.DataTypeDefinitions = _dtds;
				_gp.Tabs = _tabs;
				_gp.TabId = _tabId;
				_gp.FullId = _fullId;
			}
		}

		protected void GenericPropertyWrapper_Delete(object sender, System.EventArgs e) 
		{
			Delete(this,new System.EventArgs());
		}
		
		protected override void OnInit(EventArgs e)
		{
			base.OnInit (e);
			System.Web.UI.Control u = new System.Web.UI.UserControl().LoadControl(GlobalSettings.Path + "/controls/genericProperties/GenericProperty.ascx");
			u.ID = this.ID + "_control";
			((GenericProperty) u).Delete += new EventHandler(GenericPropertyWrapper_Delete);
			((GenericProperty) u).FullId = _fullId;
			this.Controls.Add(u);
			UpdateEditControl();
		}


	}
}
