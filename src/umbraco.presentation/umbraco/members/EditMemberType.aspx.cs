using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using umbraco.cms.presentation.Trees;

namespace umbraco.cms.presentation.members
{
	public partial class EditMemberType : BasePages.UmbracoEnsuredPage
	{

	    public EditMemberType()
	    {
            CurrentApp = BusinessLogic.DefaultApps.member.ToString();

	    }
		protected System.Web.UI.WebControls.PlaceHolder plc;
		private cms.businesslogic.member.MemberType dt;

		private System.Collections.ArrayList ExtraPropertyTypeInfos = new System.Collections.ArrayList();
		protected controls.ContentTypeControlNew ContentTypeControlNew1;

		protected void Page_Load(object sender, System.EventArgs e)
		{
			dt = new cms.businesslogic.member.MemberType(int.Parse(Request.QueryString["id"]));
			setupExtraEditorControls();
			ContentTypeControlNew1.InfoTabPage.Controls.Add(Pane1andmore);

			if (!IsPostBack)
			{
				ClientTools
					.SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadMemberTypes>().Tree.Alias)
					.SyncTree(dt.Id.ToString(), false);
			}

		}
		protected override bool OnBubbleEvent(object source, EventArgs args)
		{
			bool handled = false;
			if (args is controls.SaveClickEventArgs) 
			{
				controls.SaveClickEventArgs e = (controls.SaveClickEventArgs) args;
				if (e.Message == "Saved") 
				{
					saveExtras();
				
                    ClientTools
                        .ShowSpeechBubble(speechBubbleIcon.save, "Memebertype saved", "")
                        .SyncTree(dt.Id.ToString(), true);					
                    
				} 
				else 
				{
                    ClientTools
                        .ShowSpeechBubble(e.IconType, e.Message, "")
                        .SyncTree(dt.Id.ToString(), true);
                    
				}
				handled = true;
			}
			setupExtraEditorControls();
			return handled;
		}
		private void setupExtraEditorControls(){
			DataTable dt1 = new DataTable();
			dt1.Columns.Add("id");
			dt1.Columns.Add("name");
			dt1.Columns.Add("canedit");
			dt1.Columns.Add("canview");

			foreach (cms.businesslogic.propertytype.PropertyType pt in dt.PropertyTypes) 
			{
				DataRow dr = dt1.NewRow();
				dr["name"] = pt.Name;
				dr["id"] = pt.Id;
				dt1.Rows.Add(dr);
			}
			dgEditExtras.DataSource = dt1;
			dgEditExtras.DataBind();			
		}

		protected void saveExtras() {
			foreach (DataGridItem dgi in dgEditExtras.Items) {
				if(dgi.ItemType == ListItemType.Item || dgi.ItemType == ListItemType.AlternatingItem) 
				{
					cms.businesslogic.propertytype.PropertyType pt = cms.businesslogic.propertytype.PropertyType.GetPropertyType(int.Parse(dgi.Cells[0].Text));
					dt.setMemberCanEdit(pt,((CheckBox)dgi.FindControl("ckbMemberCanEdit")).Checked);
					dt.setMemberViewOnProfile(pt,((CheckBox)dgi.FindControl("ckbMemberCanView")).Checked);
                    dt.Save();
				}
			}
		
		}
		protected void dgEditExtras_itemdatabound(object sender,DataGridItemEventArgs e) 
		{
			if(e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem) 
			{
				cms.businesslogic.propertytype.PropertyType pt = cms.businesslogic.propertytype.PropertyType.GetPropertyType(int.Parse(((DataRowView)e.Item.DataItem).Row["id"].ToString()));
				((CheckBox)e.Item.FindControl("ckbMemberCanEdit")).Checked = dt.MemberCanEdit(pt);
				((CheckBox)e.Item.FindControl("ckbMemberCanView")).Checked = dt.ViewOnProfile(pt);
			}
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//

            InitializeComponent();
            base.OnInit(e);

			
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{   

		}
		#endregion

	}


}
