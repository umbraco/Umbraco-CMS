using System;
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
using umbraco.cms.businesslogic.member;
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
		private businesslogic.member.MemberType _memberType;

	    protected ContentTypeControlNew ContentTypeControlNew1;

	    protected override void OnInit(EventArgs e)
	    {
	        base.OnInit(e);

            ContentTypeControlNew1.SavingContentType += ContentTypeControlNew1_SavingContentType;
	    }

	    protected override void OnLoad(EventArgs e)
	    {
	        base.OnLoad(e);

            _memberType = new businesslogic.member.MemberType(int.Parse(Request.QueryString["id"]));
            
            ContentTypeControlNew1.InfoTabPage.Controls.Add(Pane1andmore);

            if (!IsPostBack)
            {
                SetupExtraEditorControls();

                ClientTools
                    .SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadMemberTypes>().Tree.Alias)
                    .SyncTree(_memberType.Id.ToString(CultureInfo.InvariantCulture), false);
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
                    SetupExtraEditorControls();
				
                    ClientTools
                        .ShowSpeechBubble(speechBubbleIcon.save, "Membertype saved", "")
                        .SyncTree(_memberType.Id.ToString(CultureInfo.InvariantCulture), true);					
                    
				} 
				else 
				{
                    ClientTools
                        .ShowSpeechBubble(e.IconType, e.Message, "")
                        .SyncTree(_memberType.Id.ToString(CultureInfo.InvariantCulture), true);
                    
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
	        var propTypes = _memberType.PropertyTypes.Where(x => builtIns.Contains(x.Alias) == false);

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

        /// <summary>
        /// Executes some code before the member type is saved, this allows us to save the member can edit/member can view information
        /// before the Save() command is executed.
        /// </summary>
        /// <param name="e"></param>
        void ContentTypeControlNew1_SavingContentType(businesslogic.ContentType e)
        {
            var mt = e as MemberType;
            if (mt == null) return; //This should not happen!
            foreach (DataGridItem dgi in dgEditExtras.Items)
            {
                if (dgi.ItemType == ListItemType.Item || dgi.ItemType == ListItemType.AlternatingItem)
                {
                    var pt = cms.businesslogic.propertytype.PropertyType.GetPropertyType(int.Parse(dgi.Cells[0].Text));
                    mt.setMemberCanEdit(pt, ((CheckBox)dgi.FindControl("ckbMemberCanEdit")).Checked);
                    mt.setMemberViewOnProfile(pt, ((CheckBox)dgi.FindControl("ckbMemberCanView")).Checked);
                }
            }
        }

	    protected void dgEditExtras_itemdatabound(object sender,DataGridItemEventArgs e) 
		{
			if(e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem) 
			{
				var pt = cms.businesslogic.propertytype.PropertyType.GetPropertyType(int.Parse(((DataRowView)e.Item.DataItem).Row["id"].ToString()));
				((CheckBox)e.Item.FindControl("ckbMemberCanEdit")).Checked = _memberType.MemberCanEdit(pt);
				((CheckBox)e.Item.FindControl("ckbMemberCanView")).Checked = _memberType.ViewOnProfile(pt);
			}
		}

	}


}
