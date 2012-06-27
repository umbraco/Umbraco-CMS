using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using umbraco.BasePages;
using umbraco.cms.businesslogic.relation;

namespace umbraco.cms.presentation.Trees.RelationTypes
{
    /// <summary>
    /// Add a new Relation Type
    /// </summary>
    public partial class NewRelationType : UmbracoEnsuredPage
    {
        /// <summary>
        /// On Load event
        /// </summary>
        /// <param name="sender">this aspx page</param>
        /// <param name="e">EventArgs (expect empty)</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.Page.IsPostBack)
            {
                this.Form.DefaultFocus = this.descriptionTextBox.ClientID;
            }

            this.AppendUmbracoObjectTypes(this.parentDropDownList);
            this.AppendUmbracoObjectTypes(this.childDropDownList);
        }

        /// <summary>
        /// Server side validation to ensure there are no existing relationshipTypes with the alias of
        /// the relation type being added
        /// </summary>
        /// <param name="source">the aliasCustomValidator control</param>
        /// <param name="args">to set validation respose</param>
        protected void AliasCustomValidator_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = RelationType.GetByAlias(this.aliasTextBox.Text.Trim()) == null;
        }

        /// <summary>
        /// Add a new relation type
        /// </summary>
        /// <param name="sender">expects the addButton control</param>
        /// <param name="e">expects EventArgs for addButton</param>
        protected void AddButton_Click(object sender, EventArgs e)
        {
            if (this.Page.IsValid)
            {
                int newRelationTypeId = uQuery.SqlHelper.ExecuteScalar<int>(
                    string.Format(
                        "INSERT INTO umbracoRelationType (dual, parentObjectType, childObjectType, name, alias) VALUES ({0}, '{1}', '{2}', '{3}', '{4}')",
                        this.dualRadioButtonList.SelectedValue,
                        // UmbracoHelper.GetGuid(UmbracoHelper.GetUmbracoObjectType(this.parentDropDownList.SelectedValue)).ToString(),
                        // UmbracoHelper.GetGuid(UmbracoHelper.GetUmbracoObjectType(this.childDropDownList.SelectedValue)).ToString(),
                        uQuery.GetUmbracoObjectType(this.parentDropDownList.SelectedValue).GetGuid().ToString(),
                        uQuery.GetUmbracoObjectType(this.childDropDownList.SelectedValue).GetGuid().ToString(),
                        this.descriptionTextBox.Text,
                        this.aliasTextBox.Text.Trim()) + "; SELECT MAX (id) FROM umbracoRelationType");

                // base.speechBubble(BasePage.speechBubbleIcon.success, "New Relation Type", "relation type created");
                
                BasePage.Current.ClientTools.ChangeContentFrameUrl("/umbraco/Trees/RelationTypes/EditRelationType.aspx?id=" + newRelationTypeId.ToString()).CloseModalWindow().ChildNodeCreated();                
            }
        }

        /// <summary>
        /// Adds the Umbraco Object types to a drop down list
        /// </summary>
        /// <param name="dropDownList">control for which to add the Umbraco object types</param>
        private void AppendUmbracoObjectTypes(DropDownList dropDownList)
        {
            // dropDownList.Items.Add(new ListItem(UmbracoHelper.GetFriendlyName(UmbracoHelper.UmbracoObjectType.Document), UmbracoHelper.GetName(UmbracoHelper.UmbracoObjectType.Document)));
            // dropDownList.Items.Add(new ListItem(UmbracoHelper.GetFriendlyName(UmbracoHelper.UmbracoObjectType.Media), UmbracoHelper.GetName(UmbracoHelper.UmbracoObjectType.Media)));
            // dropDownList.Items.Add(new ListItem(UmbracoHelper.GetFriendlyName(UmbracoHelper.UmbracoObjectType.Member), UmbracoHelper.GetName(UmbracoHelper.UmbracoObjectType.Member)));
            // ////dropDownList.Items.Add(new ListItem("---", "---"));
            // dropDownList.Items.Add(new ListItem(UmbracoHelper.GetFriendlyName(UmbracoHelper.UmbracoObjectType.MemberGroup), UmbracoHelper.GetName(UmbracoHelper.UmbracoObjectType.MemberGroup)));
            // dropDownList.Items.Add(new ListItem(UmbracoHelper.GetFriendlyName(UmbracoHelper.UmbracoObjectType.MemberType), UmbracoHelper.GetName(UmbracoHelper.UmbracoObjectType.MemberType)));
            // dropDownList.Items.Add(new ListItem(UmbracoHelper.GetFriendlyName(UmbracoHelper.UmbracoObjectType.DocumentType), UmbracoHelper.GetName(UmbracoHelper.UmbracoObjectType.DocumentType)));
            // dropDownList.Items.Add(new ListItem(UmbracoHelper.GetFriendlyName(UmbracoHelper.UmbracoObjectType.MediaType), UmbracoHelper.GetName(UmbracoHelper.UmbracoObjectType.MediaType)));
            // dropDownList.Items.Add(new ListItem(UmbracoHelper.GetFriendlyName(UmbracoHelper.UmbracoObjectType.ContentItem), UmbracoHelper.GetName(UmbracoHelper.UmbracoObjectType.ContentItem)));
            // dropDownList.Items.Add(new ListItem(UmbracoHelper.GetFriendlyName(UmbracoHelper.UmbracoObjectType.ContentItemType), UmbracoHelper.GetName(UmbracoHelper.UmbracoObjectType.ContentItemType)));
            // dropDownList.Items.Add(new ListItem(UmbracoHelper.GetFriendlyName(UmbracoHelper.UmbracoObjectType.DataType), UmbracoHelper.GetName(UmbracoHelper.UmbracoObjectType.DataType)));
            // dropDownList.Items.Add(new ListItem(UmbracoHelper.GetFriendlyName(UmbracoHelper.UmbracoObjectType.RecycleBin), UmbracoHelper.GetName(UmbracoHelper.UmbracoObjectType.RecycleBin)));
            // dropDownList.Items.Add(new ListItem(UmbracoHelper.GetFriendlyName(UmbracoHelper.UmbracoObjectType.ROOT), UmbracoHelper.GetName(UmbracoHelper.UmbracoObjectType.ROOT)));
            // dropDownList.Items.Add(new ListItem(UmbracoHelper.GetFriendlyName(UmbracoHelper.UmbracoObjectType.Stylesheet), UmbracoHelper.GetName(UmbracoHelper.UmbracoObjectType.Stylesheet)));
            // dropDownList.Items.Add(new ListItem(UmbracoHelper.GetFriendlyName(UmbracoHelper.UmbracoObjectType.Template), UmbracoHelper.GetName(UmbracoHelper.UmbracoObjectType.Template)));

            dropDownList.Items.Add(new ListItem(uQuery.UmbracoObjectType.Document.GetFriendlyName(), uQuery.UmbracoObjectType.Document.GetName()));
            dropDownList.Items.Add(new ListItem(uQuery.UmbracoObjectType.Media.GetFriendlyName(), uQuery.UmbracoObjectType.Media.GetName()));
            dropDownList.Items.Add(new ListItem(uQuery.UmbracoObjectType.Member.GetFriendlyName(), uQuery.UmbracoObjectType.Member.GetName()));
            //////dropDownList.Items.Add(new ListItem("---", "---"));
            dropDownList.Items.Add(new ListItem(uQuery.UmbracoObjectType.MemberGroup.GetFriendlyName(), uQuery.UmbracoObjectType.MemberGroup.GetName()));
            dropDownList.Items.Add(new ListItem(uQuery.UmbracoObjectType.MemberType.GetFriendlyName(), uQuery.UmbracoObjectType.MemberType.GetName()));
            dropDownList.Items.Add(new ListItem(uQuery.UmbracoObjectType.DocumentType.GetFriendlyName(), uQuery.UmbracoObjectType.DocumentType.GetName()));
            dropDownList.Items.Add(new ListItem(uQuery.UmbracoObjectType.MediaType.GetFriendlyName(), uQuery.UmbracoObjectType.MediaType.GetName()));
            dropDownList.Items.Add(new ListItem(uQuery.UmbracoObjectType.ContentItem.GetFriendlyName(), uQuery.UmbracoObjectType.ContentItem.GetName()));
            dropDownList.Items.Add(new ListItem(uQuery.UmbracoObjectType.ContentItemType.GetFriendlyName(), uQuery.UmbracoObjectType.ContentItemType.GetName()));
            dropDownList.Items.Add(new ListItem(uQuery.UmbracoObjectType.DataType.GetFriendlyName(), uQuery.UmbracoObjectType.DataType.GetName()));
            dropDownList.Items.Add(new ListItem(uQuery.UmbracoObjectType.RecycleBin.GetFriendlyName(), uQuery.UmbracoObjectType.RecycleBin.GetName()));
            dropDownList.Items.Add(new ListItem(uQuery.UmbracoObjectType.ROOT.GetFriendlyName(), uQuery.UmbracoObjectType.ROOT.GetName()));
            dropDownList.Items.Add(new ListItem(uQuery.UmbracoObjectType.Stylesheet.GetFriendlyName(), uQuery.UmbracoObjectType.Stylesheet.GetName()));
            dropDownList.Items.Add(new ListItem(uQuery.UmbracoObjectType.Template.GetFriendlyName(), uQuery.UmbracoObjectType.Template.GetName()));
        }
    }
}