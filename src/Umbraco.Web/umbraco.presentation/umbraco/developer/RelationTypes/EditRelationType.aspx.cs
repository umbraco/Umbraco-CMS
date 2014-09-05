using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using Umbraco.Core;
using Umbraco.Core.Models;
using RelationType = umbraco.cms.businesslogic.relation.RelationType;

namespace umbraco.cms.presentation.developer.RelationTypes
{
	/// <summary>
	/// Edit an existing RelationType
	/// </summary>
    [WebformsPageTreeAuthorize(Constants.Trees.RelationTypes)]
	public partial class EditRelationType : UmbracoEnsuredPage
	{

		/// <summary>
		/// Class scope reference to the current RelationType being edited
		/// </summary>
		private IRelationType _relationType;

		/// <summary>
		/// Class scope reference to the relations associated with the current RelationType
		/// </summary>
		private List<ReadOnlyRelation> _relations;

		/// <summary>
		/// Umbraco ObjectType used to represent all parent items in this relation type
		/// </summary>
		/// 
		private UmbracoObjectTypes _parentObjectType;

		/// <summary>
		/// Umbraco ObjectType used to represent all child items in this relation type
		/// </summary>
		private UmbracoObjectTypes _childObjectType;

		/// <summary>
		/// Gets the name of the parent object type for all relations in this relation type 
		/// </summary>
		protected string ParentObjectType
		{
			get
			{
				return this._parentObjectType.GetName();  //UmbracoHelper.GetName(this.parentObjectType);
			}
		}

		/// <summary>
		/// Gets the name of the child object type for all relations in this relation type
		/// </summary>
		protected string ChildObjectType
		{
			get
			{
				return this._childObjectType.GetName(); //UmbracoHelper.GetName(this.childObjectType);
			}
		}

		/// <summary>
		/// Gets a string representing the current relation type direction
		/// </summary>
		protected string RelationTypeDirection
		{
			get
			{
				return this._relationType.IsBidirectional ? "bidirectional" : "parentToChild";
			}
		}

		/// <summary>
		/// Gets the Relations for this RelationType, via lazy load
		/// </summary>
		private List<ReadOnlyRelation> Relations
		{
			get
			{
				if (this._relations == null)
				{
					this._relations = new List<ReadOnlyRelation>();

				    using (var reader = uQuery.SqlHelper.ExecuteReader(@"
                        SELECT  A.id, 
                                A.parentId,
		                        B.[text] AS parentText,
		                        A.childId,
		                        C.[text] AS childText,
                                A.relType,
		                        A.[datetime], 
		                        A.comment
                        FROM umbracoRelation A 
	                        LEFT OUTER JOIN umbracoNode B ON A.parentId = B.id
	                        LEFT OUTER JOIN umbracoNode C ON A.childId = C.id					
                        WHERE A.relType = " + this._relationType.Id.ToString()))
					{
						while (reader.Read())
						{
							var readOnlyRelation = new ReadOnlyRelation();

							readOnlyRelation.Id = reader.GetInt("id");
							readOnlyRelation.ParentId = reader.GetInt("parentId");
							readOnlyRelation.ParentText = reader.GetString("parentText");
							readOnlyRelation.ChildId = reader.GetInt("childId");
							readOnlyRelation.ChildText = reader.GetString("childText");
							readOnlyRelation.RelType = reader.GetInt("relType");
							readOnlyRelation.DateTime = reader.GetDateTime("datetime");
							readOnlyRelation.Comment = reader.GetString("comment");

							this._relations.Add(readOnlyRelation);
						}
					}
				}

				return this._relations;
			}
		}

		/// <summary>
		/// On Load event
		/// </summary>
		/// <param name="sender">this aspx page</param>
		/// <param name="e">EventArgs (expect empty)</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			int id;
			if (int.TryParse(Request.QueryString["id"], out id))
			{
                var relationService = Services.RelationService;

			    this._relationType = relationService.GetRelationTypeById(id);
				if (this._relationType != null)
				{
				    this._parentObjectType = UmbracoObjectTypesExtensions.GetUmbracoObjectType(this._relationType.ParentObjectType);
				    this._childObjectType = UmbracoObjectTypesExtensions.GetUmbracoObjectType(this._relationType.ChildObjectType);

					// -----------

				    if (!this.IsPostBack)
				    {
				        this.EnsureChildControls();

				        this.idLiteral.Text = this._relationType.Id.ToString();
				        this.nameTextBox.Text = this._relationType.Name;
				        this.aliasTextBox.Text = this._relationType.Alias;

				        if (this._relationType.IsBidirectional)
				        {
				            this.dualRadioButtonList.Items.FindByValue("1").Selected = true;
				        }
				        else
				        {
				            this.dualRadioButtonList.Items.FindByValue("0").Selected = true;
				        }

				        this.parentLiteral.Text = this._parentObjectType.GetFriendlyName();
				            // UmbracoHelper.GetFriendlyName(this.parentObjectType);
				        this.childLiteral.Text = this._childObjectType.GetFriendlyName();
				            // UmbracoHelper.GetFriendlyName(this.childObjectType);

				        this.relationsCountLiteral.Text = this.Relations.Count.ToString();

				        this.relationsRepeater.DataSource = this.Relations;
				        this.relationsRepeater.DataBind();
				    }
				}
				else
				{
					throw new Exception("Unable to get RelationType where ID = " + id);
				}
			}
			else
			{
				throw new Exception("Invalid RelationType ID");
			}
		}

		/// <summary>
		/// Creates the child controls used in this page
		/// </summary>
		protected override void CreateChildControls()
		{
			base.CreateChildControls();

			var relationTypeTabPage = this.tabControl.NewTabPage("Relation Type");
			relationTypeTabPage.Controls.Add(this.idPane);
			relationTypeTabPage.Controls.Add(this.nameAliasPane);
			relationTypeTabPage.Controls.Add(this.directionPane);
			relationTypeTabPage.Controls.Add(this.objectTypePane);

			var saveMenuImageButton =  tabControl.Menu.NewButton();
			saveMenuImageButton.ToolTip = "save relation type";
			saveMenuImageButton.Click +=saveMenuImageButton_Click;
			saveMenuImageButton.CausesValidation = true;
            saveMenuImageButton.Text = ui.Text("save");
			saveMenuImageButton.ValidationGroup = "RelationType";

			var relationsTabPage = this.tabControl.NewTabPage("Relations");
			relationsTabPage.Controls.Add(this.relationsCountPane);
			relationsTabPage.Controls.Add(this.relationsPane);

            /*
			var refreshMenuImageButton = relationsTabPage.Menu.NewImageButton();
			refreshMenuImageButton.AlternateText = "refresh relations";
			refreshMenuImageButton.Click += this.RefreshMenuImageButton_Click;
			refreshMenuImageButton.ImageUrl = "/umbraco/developer/RelationTypes/Images/Refresh.gif";
			refreshMenuImageButton.CausesValidation = false;*/
		}

       /// <summary>
		/// check that alias hasn't been changed to clash with another (except itself)
		/// </summary>
		/// <param name="source">the aliasCustomValidator control</param>
		/// <param name="args">to set validation respose</param>
		protected void AliasCustomValidator_ServerValidate(object source, ServerValidateEventArgs args)
		{
			args.IsValid = (RelationType.GetByAlias(this.aliasTextBox.Text.Trim()) == null) ||
				(this.aliasTextBox.Text.Trim() == this._relationType.Alias);
		}

		/// <summary>
		/// Reload the relations, in case they have changed
		/// </summary>
		/// <param name="sender">expects refreshMenuImageButton</param>
		/// <param name="e">expects ImageClickEventArgs</param>
		private void RefreshMenuImageButton_Click(object sender, ImageClickEventArgs e)
		{
		}

		/// <summary>
		/// Save button in Umbraco menu
		/// </summary>
		/// <param name="sender">expects saveMenuImageButton object</param>
		/// <param name="e">expects ImageClickEventArgs</param>
        void saveMenuImageButton_Click(object sender, EventArgs e)
		{
			if (this.Page.IsValid)
			{
				var nameChanged = this._relationType.Name != this.nameTextBox.Text.Trim();
				var aliasChanged = this._relationType.Alias != this.aliasTextBox.Text.Trim();
				var directionChanged = this._relationType.IsBidirectional != (this.dualRadioButtonList.SelectedValue == "1");

				if (nameChanged || aliasChanged || directionChanged)
				{
					string bubbleBody = string.Empty;

					if (nameChanged)
					{
						bubbleBody += "Name, ";

						this._relationType.Name = this.nameTextBox.Text.Trim();

						// Refresh tree, as the name as changed
						ClientTools.SyncTree(this._relationType.Id.ToString(), true);
					}

					if (directionChanged)
					{
						bubbleBody += "Direction, ";
						this._relationType.IsBidirectional = this.dualRadioButtonList.SelectedValue == "1";
					}

					if (aliasChanged)
					{
						bubbleBody += "Alias, ";
						this._relationType.Alias = this.aliasTextBox.Text.Trim();
					}

					bubbleBody = bubbleBody.Remove(bubbleBody.LastIndexOf(','), 1);
					bubbleBody = bubbleBody + "Changed";

				    var relationService = Services.RelationService;
                    relationService.Save(this._relationType);
                    
					ClientTools.ShowSpeechBubble(speechBubbleIcon.save, "Relation Type Updated", bubbleBody);
				}
			}
		}
	}
}
