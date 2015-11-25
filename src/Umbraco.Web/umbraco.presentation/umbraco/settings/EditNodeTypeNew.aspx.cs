using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI.WebControls;
using umbraco.cms.presentation.Trees;
using umbraco.cms.businesslogic.web;
using System.Linq;
using umbraco.controls;
using Umbraco.Core;

namespace umbraco.settings
{
    public partial class EditContentTypeNew : BasePages.UmbracoEnsuredPage
    {
        public EditContentTypeNew()
        {
            CurrentApp = BusinessLogic.DefaultApps.settings.ToString();
        }

        protected controls.ContentTypeControlNew ContentTypeControlNew1;
        private DocumentType _dt;

        override protected void OnInit(EventArgs e)
        {
            ContentTypeControlNew1.DocumentTypeCallback = new Func<DocumentType, DocumentType>(UpdateAllowedTemplates);
            ContentTypeControlNew1.InfoTabPage.Controls.Add(tmpPane);
            base.OnInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            _dt = new DocumentType(int.Parse(Request.QueryString["id"]));
            if (!Page.IsPostBack)
            {
                BindTemplates();

                ClientTools
                    .SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadNodeTypes>().Tree.Alias)
                     .SyncTree("-1,init," + _dt.Path.Replace("-1,", ""), false);
            }
        }

        protected override bool OnBubbleEvent(object source, EventArgs args)
        {
            bool handled = false;
            var eventArgs = args as SaveClickEventArgs;
            if (eventArgs != null)
            {
                var e = eventArgs;
                if (e.Message == "Saved")
                {
                    ClientTools.ShowSpeechBubble(e.IconType, ui.Text("contentTypeSavedHeader"), "");

                    BindTemplates();
                }
                else
                {
                    ClientTools.ShowSpeechBubble(e.IconType, e.Message, "");
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

        private DocumentType UpdateAllowedTemplates(DocumentType documentType)
        {
            var tmp = new ArrayList();

            foreach (ListItem li in templateList.Items)
            {
                if (li.Selected)
                    tmp.Add(new cms.businesslogic.template.Template(int.Parse(li.Value)));
            }

            var tt = new cms.businesslogic.template.Template[tmp.Count];
            for (int i = 0; i < tt.Length; i++)
            {
                tt[i] = (cms.businesslogic.template.Template)tmp[i];
            }

            documentType.allowedTemplates = tt;

            if (documentType.allowedTemplates.Length > 0 && ddlTemplates.SelectedIndex >= 0)
            {
                documentType.DefaultTemplate = int.Parse(ddlTemplates.SelectedValue);
            }
            else
            {
                documentType.RemoveDefaultTemplate();
            }

            _dt = documentType;

            return documentType;
        }

        private void BindTemplates()
        {
            var templates = (from t in cms.businesslogic.template.Template.GetAllAsList()
                             join at in _dt.allowedTemplates on t.Id equals at.Id into at_l
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
                var li = new ListItem { Text = Server.HtmlEncode(item.Name), Value = item.Id.ToString(CultureInfo.InvariantCulture), Selected = item.Selected };
                return li;
            }).ToArray());


            ddlTemplates.Enabled = templates.Any();
            ddlTemplates.Items.Clear();
            ddlTemplates.Items.Insert(0, new ListItem(ui.Text("choose") + "...", "0"));
            ddlTemplates.Items.AddRange(templates.ConvertAll(item =>
            {
                var li = new ListItem { Text = Server.HtmlEncode(item.Name), Value = item.Id.ToString(CultureInfo.InvariantCulture) };
                return li;
            }).ToArray());

            var ddlTemplatesSelect = ddlTemplates.Items.FindByValue(_dt.DefaultTemplate.ToString(CultureInfo.InvariantCulture));
            if (ddlTemplatesSelect != null)
                ddlTemplatesSelect.Selected = true;
        }

        /// <summary>
        /// tmpPane control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.Pane tmpPane;

        /// <summary>
        /// templateList control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.CheckBoxList templateList;

        /// <summary>
        /// ddlTemplates control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.DropDownList ddlTemplates;
    }
}
