using System;
using System.Web.UI.WebControls;
using umbraco.BasePages;
using umbraco.cms.businesslogic.relation;

namespace umbraco.cms.presentation.developer.RelationTypes
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
		/// Add a new relation type into the database, and redirects to it's editing page.
		/// </summary>
		/// <param name="sender">expects the addButton control</param>
		/// <param name="e">expects EventArgs for addButton</param>
		protected void AddButton_Click(object sender, EventArgs e)
		{
			if (this.Page.IsValid)
			{
                string newRelationTypeAlias = this.aliasTextBox.Text.Trim();

                uQuery.SqlHelper.ExecuteNonQuery(
                    string.Format("INSERT INTO umbracoRelationType ([dual], parentObjectType, childObjectType, name, alias) VALUES ({0}, '{1}', '{2}', '{3}', '{4}')",
                        this.dualRadioButtonList.SelectedValue,
                        uQuery.GetUmbracoObjectType(this.parentDropDownList.SelectedValue).GetGuid().ToString(),
                        uQuery.GetUmbracoObjectType(this.childDropDownList.SelectedValue).GetGuid().ToString(),
                        this.descriptionTextBox.Text,
                        newRelationTypeAlias));

                int newRelationTypeId = uQuery.SqlHelper.ExecuteScalar<int>("SELECT id FROM umbracoRelationType WHERE alias = '" + newRelationTypeAlias + "'");

				// base.speechBubble(BasePage.speechBubbleIcon.success, "New Relation Type", "relation type created");

				BasePage.Current.ClientTools.ChangeContentFrameUrl("/umbraco/developer/RelationTypes/EditRelationType.aspx?id=" + newRelationTypeId.ToString()).CloseModalWindow().ChildNodeCreated();
			}
		}

		/// <summary>
		/// Adds the Umbraco Object types to a drop down list
		/// </summary>
		/// <param name="dropDownList">control for which to add the Umbraco object types</param>
		private void AppendUmbracoObjectTypes(DropDownList dropDownList)
		{
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