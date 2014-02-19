using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using ClientDependency.Core;
using umbraco.cms.presentation.Trees;
using umbraco.controls;

namespace umbraco.cms.presentation.members
{
	public partial class EditMemberType : BasePages.UmbracoEnsuredPage
	{

	    public EditMemberType()
	    {
            CurrentApp = BusinessLogic.DefaultApps.member.ToString();

	    }
		protected PlaceHolder plc;
		private businesslogic.member.MemberType _dt;

		private ArrayList _extraPropertyTypeInfos = new ArrayList();
		protected controls.ContentTypeControlNew ContentTypeControlNew1;

		protected void Page_Load(object sender, System.EventArgs e)
		{
			_dt = new cms.businesslogic.member.MemberType(int.Parse(Request.QueryString["id"]));
			SetupExtraEditorControls();
			ContentTypeControlNew1.InfoTabPage.Controls.Add(Pane1andmore);

			if (!IsPostBack)
			{
				ClientTools
					.SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadMemberTypes>().Tree.Alias)
					.SyncTree(_dt.Id.ToString(), false);
			}

		}
		protected override bool OnBubbleEvent(object source, EventArgs args)
		{
			var handled = false;
		    var eventArgs = args as SaveClickEventArgs;
		    if (eventArgs != null) 
			{
				var e = eventArgs;
				if (e.Message == "Saved") 
				{
					saveExtras();
				
                    ClientTools
                        .ShowSpeechBubble(speechBubbleIcon.save, "Membertype saved", "")
                        .SyncTree(_dt.Id.ToString(CultureInfo.InvariantCulture), true);					
                    
				} 
				else 
				{
                    ClientTools
                        .ShowSpeechBubble(e.IconType, e.Message, "")
                        .SyncTree(_dt.Id.ToString(CultureInfo.InvariantCulture), true);
                    
				}
				handled = true;
			}
			
			return handled;
		}

	    private void SetupExtraEditorControls()
	    {
	        var dt1 = new DataTable();
	        dt1.Columns.Add("id");
	        dt1.Columns.Add("name");
	        dt1.Columns.Add("canedit");
	        dt1.Columns.Add("canview");

            //filter out the 'built-in' property types as we don't want to display these options for them
	        var builtIns = Umbraco.Core.Constants.Conventions.Member.GetStandardPropertyTypeStubs().Select(x => x.Key).ToArray();
	        var propTypes = _dt.PropertyTypes.Where(x => builtIns.Contains(x.Alias) == false);

            foreach (var pt in propTypes)
	        {
	            var dr = dt1.NewRow();
	            dr["name"] = pt.Name;
	            dr["id"] = pt.Id;
	            dt1.Rows.Add(dr);
	        }
	        dgEditExtras.DataSource = dt1;
	        dgEditExtras.DataBind();
	    }

	    protected void saveExtras()
	    {
	        foreach (DataGridItem dgi in dgEditExtras.Items)
	        {
	            if (dgi.ItemType == ListItemType.Item || dgi.ItemType == ListItemType.AlternatingItem)
	            {
	                var pt = cms.businesslogic.propertytype.PropertyType.GetPropertyType(int.Parse(dgi.Cells[0].Text));
	                _dt.setMemberCanEdit(pt, ((CheckBox) dgi.FindControl("ckbMemberCanEdit")).Checked);
	                _dt.setMemberViewOnProfile(pt, ((CheckBox) dgi.FindControl("ckbMemberCanView")).Checked);
	                _dt.Save();
	            }
	        }
	    }

	    protected void dgEditExtras_itemdatabound(object sender,DataGridItemEventArgs e) 
		{
			if(e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem) 
			{
				var pt = cms.businesslogic.propertytype.PropertyType.GetPropertyType(int.Parse(((DataRowView)e.Item.DataItem).Row["id"].ToString()));
				((CheckBox)e.Item.FindControl("ckbMemberCanEdit")).Checked = _dt.MemberCanEdit(pt);
				((CheckBox)e.Item.FindControl("ckbMemberCanView")).Checked = _dt.ViewOnProfile(pt);
			}
		}

	}


}
