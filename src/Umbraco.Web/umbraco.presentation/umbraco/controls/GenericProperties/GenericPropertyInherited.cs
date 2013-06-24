using System;
using System.Collections.Generic;
using umbraco.IO;
using umbraco.cms.businesslogic.propertytype;
using ClientDependency.Core;

namespace umbraco.controls.GenericProperties
{
	/// <summary>
	/// Summary description for GenericPropertyWrapper.
    /// </summary>
    [ClientDependency(ClientDependencyType.Css, "GenericProperty/genericproperty.css", "UmbracoClient")]
	public class GenericPropertyInherited : System.Web.UI.WebControls.PlaceHolder
	{
        private cms.businesslogic.propertytype.PropertyType _pt;
        private cms.businesslogic.ContentType _ct;
		private string _fullId = "";

		public cms.businesslogic.propertytype.PropertyType PropertyType 
		{
			set {_pt = value;}
			get {return _pt;}
		}

        public cms.businesslogic.ContentType ContentType
        {
            set { _ct = value; }
        }

		public string FullId 
		{
			set 
			{
				_fullId = value;
			}
		}

        public GenericPropertyInherited()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit (e);
            var FullHeader = _pt.GetRawName() + " (" + _pt.Alias + "), Type: " + _pt.DataTypeDefinition.Text + " (inherited from " + _ct.Text + ")";
            System.Web.UI.Control u = new System.Web.UI.LiteralControl(String.Format(@"
<li id='{0}' class='inherited'>
	<div class='propertyForm' id='{0}_form'>
		<div id='desc{0}' style='padding: 0px; display: block; margin: 0px;'>
			<h3 style='padding: 0px; margin: 0px;'>{1}</h3>
        </div>
    </div>
</li>
            ", this._fullId, FullHeader));
			u.ID = this.ID + "_control";
			this.Controls.Add(u);
			
		}
    }
}
