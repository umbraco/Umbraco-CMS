using System;
using System.Web.UI.WebControls;
using Umbraco.Core;
using Umbraco.Web.UI.Pages;
using umbraco.BusinessLogic;
using Umbraco.Core.Models;

namespace umbraco.cms.presentation.developer.RelationTypes
{
	/// <summary>
	/// Add a new Relation Type
	/// </summary>
    [WebformsPageTreeAuthorize(Constants.Trees.RelationTypes)]
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
		    var relationService = Services.RelationService;
			args.IsValid = relationService.GetRelationTypeByAlias(this.aliasTextBox.Text.Trim()) == null;
		}

		/// <summary>
		/// Add a new relation type into the database, and redirects to it's editing page.
		/// </summary>
		/// <param name="sender">expects the addButton control</param>
		/// <param name="e">expects EventArgs for addButton</param>
		protected void AddButton_Click(object sender, EventArgs e)
		{
			if (Page.IsValid)
			{
                var newRelationTypeAlias = this.aliasTextBox.Text.Trim();

			    var relationService = Services.RelationService;
			    var relationType = new RelationType(new Guid(this.childDropDownList.SelectedValue),
			        new Guid(this.parentDropDownList.SelectedValue), newRelationTypeAlias, this.descriptionTextBox.Text)
			                       {
			                           IsBidirectional = this.dualRadioButtonList.SelectedValue == "1"
			                       };

                relationService.Save(relationType);

			    var newRelationTypeId = relationService.GetRelationTypeByAlias(newRelationTypeAlias).Id;

				ClientTools.ChangeContentFrameUrl("developer/RelationTypes/EditRelationType.aspx?id=" + newRelationTypeId).CloseModalWindow().ChildNodeCreated();
			}
		}

		/// <summary>
		/// Adds the Umbraco Object types to a drop down list
		/// </summary>
		/// <param name="dropDownList">control for which to add the Umbraco object types</param>
		private void AppendUmbracoObjectTypes(ListControl dropDownList)
		{
            dropDownList.Items.Add(new ListItem(UmbracoObjectTypes.Document.GetFriendlyName(), Umbraco.Core.Constants.ObjectTypes.Document));
		    dropDownList.Items.Add(new ListItem(UmbracoObjectTypes.Media.GetFriendlyName(), Umbraco.Core.Constants.ObjectTypes.Media));
		    dropDownList.Items.Add(new ListItem(UmbracoObjectTypes.Member.GetFriendlyName(), Umbraco.Core.Constants.ObjectTypes.Member));
		    dropDownList.Items.Add(new ListItem(UmbracoObjectTypes.MediaType.GetFriendlyName(), Umbraco.Core.Constants.ObjectTypes.MediaType));
		    dropDownList.Items.Add(new ListItem(UmbracoObjectTypes.DocumentType.GetFriendlyName(), Umbraco.Core.Constants.ObjectTypes.DocumentType));
		    dropDownList.Items.Add(new ListItem(UmbracoObjectTypes.MemberType.GetFriendlyName(), Umbraco.Core.Constants.ObjectTypes.MemberType));
		    dropDownList.Items.Add(new ListItem(UmbracoObjectTypes.DataType.GetFriendlyName(), Umbraco.Core.Constants.ObjectTypes.DataType));
		    dropDownList.Items.Add(new ListItem(UmbracoObjectTypes.MemberGroup.GetFriendlyName(), Umbraco.Core.Constants.ObjectTypes.MemberGroup));
		    dropDownList.Items.Add(new ListItem(UmbracoObjectTypes.Stylesheet.GetFriendlyName(), Umbraco.Core.Constants.ObjectTypes.Stylesheet));
		    dropDownList.Items.Add(new ListItem(UmbracoObjectTypes.Template.GetFriendlyName(), Umbraco.Core.Constants.ObjectTypes.Template));
		    dropDownList.Items.Add(new ListItem(UmbracoObjectTypes.ROOT.GetFriendlyName(), Umbraco.Core.Constants.ObjectTypes.SystemRoot));
		    dropDownList.Items.Add(new ListItem(UmbracoObjectTypes.RecycleBin.GetFriendlyName(), Umbraco.Core.Constants.ObjectTypes.ContentRecycleBin));
		}
	}
}
