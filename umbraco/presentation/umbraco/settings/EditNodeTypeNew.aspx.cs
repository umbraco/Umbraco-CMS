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
using umbraco.cms.businesslogic.web;
using System.Linq;
using umbraco.cms.helpers;

namespace umbraco.settings
{
    public partial class EditContentTypeNew : BasePages.UmbracoEnsuredPage
    {
        public EditContentTypeNew()
        {
            CurrentApp = BusinessLogic.DefaultApps.settings.ToString();

        }

        protected controls.ContentTypeControlNew ContentTypeControlNew1;
        cms.businesslogic.web.DocumentType dt;


        private DataTable dtTemplates = new DataTable();

        override protected void OnInit(EventArgs e)
        {
            ContentTypeControlNew1.InfoTabPage.Controls.Add(tmpPane);
            base.OnInit(e);
        }

        protected void Page_Load(object sender, System.EventArgs e)
        {
            dt = new DocumentType(int.Parse(Request.QueryString["id"]));
            if (!Page.IsPostBack)
            {
                bindTemplates();

                ClientTools
                    .SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadNodeTypes>().Tree.Alias)
                     .SyncTree("-1,init," + dt.Path.Replace("-1,", ""), false);

            }


        }


        private void bindTemplates()
        {
            var templates = (from t in cms.businesslogic.template.Template.GetAllAsList()
                             join at in dt.allowedTemplates on t.Id equals at.Id into at_l
                             from at in at_l.DefaultIfEmpty()
                             select new
                             {
                                 Id = t.Id,
                                 Name = t.Text,
                                 Selected = at != null
                             }).ToList();

            templateList.Items.Clear();
            templateList.Items.AddRange(templates.ConvertAll(item =>
            {
                string anchor = DeepLink.GetAnchor(DeepLinkType.Template, item.Id.ToString(), true);
                ListItem li = new ListItem();
                if (!string.IsNullOrEmpty(anchor))
                {
                    li.Text = string.Format("{0} {1}", item.Name, anchor);
                }
                else
                {
                    li.Text = item.Name;
                }
                li.Value = item.Id.ToString();
                li.Selected = item.Selected;
                return li;
            }).ToArray());


            ddlTemplates.Enabled = templates.Any();
            ddlTemplates.Items.Clear();
            ddlTemplates.Items.Insert(0, new ListItem(ui.Text("choose") + "...", "0"));
            ddlTemplates.Items.AddRange(templates.ConvertAll(item =>
            {
                ListItem li = new ListItem();
                li.Text = item.Name;
                li.Value = item.Id.ToString();
                return li;
            }).ToArray());

            var ddlTemplatesSelect = ddlTemplates.Items.FindByValue(dt.DefaultTemplate.ToString());
            if (ddlTemplatesSelect != null) ddlTemplatesSelect.Selected = true;

        }

        protected override bool OnBubbleEvent(object source, EventArgs args)
        {
            bool handled = false;
            if (args is controls.SaveClickEventArgs)
            {
                controls.SaveClickEventArgs e = (controls.SaveClickEventArgs)args;
                if (e.Message == "Saved")
                {
                    int dtid = 0;
                    if (int.TryParse(Request.QueryString["id"], out dtid))
                        new cms.businesslogic.web.DocumentType(dtid).Save();

                    base.speechBubble(e.IconType, ui.Text("contentTypeSavedHeader"), "");

                    ArrayList tmp = new ArrayList();

                    foreach (ListItem li in templateList.Items)
                    {
                        if (li.Selected) tmp.Add(new cms.businesslogic.template.Template(int.Parse(li.Value)));
                    }

                    cms.businesslogic.template.Template[] tt = new cms.businesslogic.template.Template[tmp.Count];
                    for (int i = 0; i < tt.Length; i++)
                    {
                        tt[i] = (cms.businesslogic.template.Template)tmp[i];
                    }

                    dt.allowedTemplates = tt;


                    if (dt.allowedTemplates.Length > 0 && ddlTemplates.SelectedIndex >= 0)
                    {
                        dt.DefaultTemplate = int.Parse(ddlTemplates.SelectedValue);
                    }
                    else
                        dt.RemoveDefaultTemplate();

                    bindTemplates();
                }
                else
                {
                    base.speechBubble(e.IconType, e.Message, "");
                }
                handled = true;
            }
            return handled;
        }

        protected void dgTemplate_itemdatabound(object sender, DataGridItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                ((CheckBox)e.Item.FindControl("ckbAllowTemplate")).Checked = true;
            }
        }


    }
}
