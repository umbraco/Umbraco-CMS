using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using ClientDependency.Core;
using Umbraco.Core;
using Umbraco.Core.IO;
using umbraco.BusinessLogic;
using umbraco.cms.presentation.Trees;
using umbraco.controls.Images;
using umbraco.controls.Tree;

[assembly: WebResource("umbraco.editorControls.MultiNodeTreePicker.MultiNodePickerStyles.css", "text/css")]
[assembly: WebResource("umbraco.editorControls.MultiNodeTreePicker.MultiNodePickerScripts.js", "application/x-javascript")]

namespace umbraco.editorControls.MultiNodeTreePicker
{
	/// <summary>
	/// The user interface to display to the content editor
	/// </summary>
	[ClientDependency(ClientDependencyType.Javascript, "ui/jqueryui.js", "UmbracoClient")]
	[ClientDependency(ClientDependencyType.Javascript, "ui/jquery.tooltip.min.js", "UmbracoClient")]
	[ClientDependency(ClientDependencyType.Javascript, "controls/Images/ImageViewer.js", "UmbracoRoot")]
    [ValidationProperty("Value")]
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
	public class MNTP_DataEditor : Control, INamingContainer
	{
		#region Static Constructor

		/// <summary>
		/// This adds our filtered tree definition to the TreeDefinitionCollection at runtime
		/// instead of having to declare it in the database 
		/// </summary>
		static MNTP_DataEditor()
		{
			//NOTE: Before we had locking here but static ctors are always threadsafe

			if (TreeDefinitionCollection.Instance.Any(x => x.TreeType == typeof (FilteredContentTree))) 
				return;
			
			
			//need to add our tree definitions to the collection.

			//find the content tree to duplicate
			var contentTree = TreeDefinitionCollection.Instance.SingleOrDefault(x => string.Equals(x.Tree.Alias, Umbraco.Core.Constants.Applications.Content, StringComparison.OrdinalIgnoreCase));
            //have put this here because the legacy content tree no longer exists as a tree def
            if (contentTree == null)
            {
                contentTree = new TreeDefinition(
                    typeof (loadContent),
                    new ApplicationTree(false, true, 0, "content", "content", "Content", ".sprTreeFolder", ".sprTreeFolder_o", "", typeof (loadContent).GetFullNameWithAssembly(), ""),
                    new Application("Content", "content", "content"));
            }
			var filteredContentTree = new TreeDefinition(typeof(FilteredContentTree),
			                                             new umbraco.BusinessLogic.ApplicationTree(true, false, 0,
			                                                                                       contentTree.Tree.ApplicationAlias,
			                                                                                       "FilteredContentTree",
			                                                                                       contentTree.Tree.Title,
			                                                                                       contentTree.Tree.IconClosed,
			                                                                                       contentTree.Tree.IconOpened,
			                                                                                       "umbraco.editorControls",
			                                                                                       "MultiNodeTreePicker.FilteredContentTree",
			                                                                                       contentTree.Tree.Action),
                                                                                                   contentTree.App);

			//find the media tree to duplicate
			var mediaTree = TreeDefinitionCollection.Instance.SingleOrDefault(x => string.Equals(x.Tree.Alias, Umbraco.Core.Constants.Applications.Media, StringComparison.OrdinalIgnoreCase));
            //have put this here because the legacy content tree no longer exists as a tree def
            if (mediaTree == null)
            {
                mediaTree = new TreeDefinition(
                    typeof(loadMedia),
                    new ApplicationTree(false, true, 0, "media", "media", "Media", ".sprTreeFolder", ".sprTreeFolder_o", "", typeof(loadMedia).GetFullNameWithAssembly(), ""),
                    new Application("Media", "media", "media"));
            }
			var filteredMediaTree = new TreeDefinition(typeof(FilteredMediaTree),
			                                           new umbraco.BusinessLogic.ApplicationTree(true, false, 0,
			                                                                                     mediaTree.Tree.ApplicationAlias,
			                                                                                     "FilteredMediaTree",
                                                                                                 mediaTree.Tree.Title,
                                                                                                 mediaTree.Tree.IconClosed,
                                                                                                 mediaTree.Tree.IconOpened,
			                                                                                     "umbraco.editorControls",
			                                                                                     "MultiNodeTreePicker.FilteredMediaTree",
                                                                                                 mediaTree.Tree.Action),
                                                                                                 mediaTree.App);

			//add it to the collection at runtime
			TreeDefinitionCollection.Instance.Add(filteredContentTree);
			TreeDefinitionCollection.Instance.Add(filteredMediaTree);
		}

		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="MNTP_DataEditor"/> class.
		/// </summary>
		public MNTP_DataEditor()
		{
			this.MediaTypesWithThumbnails = new string[] { Umbraco.Core.Constants.Conventions.MediaTypes.Image };
			ShowThumbnailsForMedia = true;
			TreeToRender = Umbraco.Core.Constants.Applications.Content;
			MaxNodeCount = -1;
			MinNodeCount = 0;
			StartNodeId = uQuery.RootNodeId;
			ShowToolTips = true;
			ControlHeight = 200;
		}

        /// <summary>
        /// This is used for validation purposes only, see the [ValidationProperty("Value")] attribute above.
        /// </summary>
	    public string Value
	    {
	        get { return string.Join(",", SelectedIds); }
	    }

	    #region Protected members

		/// <summary>
		/// 
		/// </summary>
		protected CustomValidator MinItemsValidator;

		/// <summary>
		/// 
		/// </summary>
		protected CustomTreeControl TreePickerControl;

		/// <summary>
		/// 
		/// </summary>
		protected Repeater SelectedValues;

		/// <summary>
		/// 
		/// </summary>
		protected HiddenField PickedValue;

		/// <summary>
		/// 
		/// </summary>
		protected HtmlGenericControl RightColumn;
		#endregion

		#region public Properties

		/// <summary>
		/// gets/sets the value based on an array of IDs selected
		/// </summary>
		public string[] SelectedIds
		{
			get
			{
				List<string> val = new List<string>();
				var splitVals = PickedValue.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				//this will make sure only the node count specified is saved
				//into umbraco, even if people try to hack the front end for whatever reason.
				if (MaxNodeCount >= 0)
				{
					for (var i = 0; i < splitVals.Length; i++)
					{
						if (i < MaxNodeCount)
						{
							val.Add(splitVals[i]);
						}
						else break;
					}
				}
				else
				{
					val = splitVals.ToList();
				}
				return val.ToArray();
			}
			set
			{
				XmlValue = ConvertToXDocument(value);
			}
		}

		/// <summary>
		/// get/set the value for the selected nodes in xml format
		/// </summary>
		public XDocument XmlValue
		{
			get
			{
				return ConvertToXDocument(SelectedIds);
			}
			set
			{
				if (value == null)
				{
					SelectedValues.DataSource = null;
					PickedValue.Value = "";
				}
				else
				{
					//set the data source for the repeater and hidden field
					var nodes = value.Descendants("nodeId");
					SelectedValues.DataSource = nodes;
					PickedValue.Value = string.Join(",", nodes.Select(x => x.Value).ToArray());
				}
			}
		}

		/// <summary>
		/// The property name being edited with the current data editor. This is used for the min items validation statement.
		/// </summary>
		public string PropertyName { get; set; }

		/// <summary>
		/// The tree type alias to render
		/// </summary>
		public string TreeToRender { get; set; }

		/// <summary>
		///  An xpath filter to match nodes that will be disabled from being clicked
		/// </summary>
		public string XPathFilter { get; set; }

		/// <summary>
		/// The minimum amount of nodes that can be selected
		/// </summary>
		public int MinNodeCount { get; set; }

		/// <summary>
		/// The maximum amount of nodes that can be selected
		/// </summary>
		public int MaxNodeCount { get; set; }

		/// <summary>
		/// The start node id
		/// </summary>
		public int StartNodeId { get; set; }

		/// <summary>
		/// The start node selection type
		/// </summary>
		public NodeSelectionType StartNodeSelectionType { get; set; }

		/// <summary>
		/// The xpath expression type to select the start node when the StartNodeSelectionType is XPath
		/// </summary>
		public XPathExpressionType StartNodeXPathExpressionType { get; set; }

		/// <summary>
		/// The XPath expression to use to determine the start node when the StartNodeSelectionType is XPath
		/// </summary>
		public string StartNodeXPathExpression { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether [show tool tips].
		/// </summary>
		/// <value><c>true</c> if [show tool tips]; otherwise, <c>false</c>.</value>
		/// <remarks>Shows/Hides the tooltip info bubble.</remarks>
		public bool ShowToolTips { get; set; }

		/// <summary>
		/// The XPathFilterType to match
		/// </summary>
		public XPathFilterType XPathFilterMatchType { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether [show thumbnails for media].
		/// </summary>
		/// <value>
		/// 	<c>true</c> if [show thumbnails for media]; otherwise, <c>false</c>.
		/// </value>
		/// <remarks>Whether or not to show thumbnails for media</remarks>
		public bool ShowThumbnailsForMedia { get; set; }

		/// <summary>
		/// A list of media type names that can have thumbnails (i.e. 'image')
		/// </summary>
		public string[] MediaTypesWithThumbnails { get; set; }

		/// <summary>
		/// This is set by the data type and allows us to save a cookie value
		/// for persistence for the data type.
		/// </summary>
		public int DataTypeDefinitionId { get; set; }

		/// <summary>
		/// The height of the tree control box in pixels
		/// </summary>
		public int ControlHeight { get; set; }

		#endregion

		/// <summary>
		/// Initialize the control, make sure children are created
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			EnsureChildControls();
		}

		/// <summary>
		/// Add the resources (sytles/scripts)
		/// </summary>
		/// <param name="e"></param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			//add the js/css required
			this.RegisterEmbeddedClientResource("umbraco.editorControls.MultiNodeTreePicker.MultiNodePickerStyles.css", ClientDependencyType.Css);
			this.RegisterEmbeddedClientResource("umbraco.editorControls.MultiNodeTreePicker.MultiNodePickerScripts.js", ClientDependencyType.Javascript);

			//update the tree type (we need to do this each time because i don't think view state works with these controls)
			switch (TreeToRender)
			{
				case Umbraco.Core.Constants.Applications.Media:
					TreePickerControl.TreeType = "FilteredMediaTree";
					TreePickerControl.App = Umbraco.Core.Constants.Applications.Media;
					break;
				case Umbraco.Core.Constants.Applications.Content:
				default:
					TreePickerControl.TreeType = "FilteredContentTree";
					TreePickerControl.App = Umbraco.Core.Constants.Applications.Content;
					break;
			}

			if (Page.IsPostBack)
			{
				//since it is a post back, bind the data source to the view state values
				XmlValue = ConvertToXDocument(SelectedIds);
			}

			//bind the repeater if theres a data source, or if there's no datasource but this is a postback (i.e. nodes deleted)
			if (SelectedValues.DataSource != null || Page.IsPostBack)
			{
				SelectedValues.DataBind();
			}

		}

		/// <summary>
		/// Creates the child controls for this control
		/// </summary>
		protected override void CreateChildControls()
		{
			base.CreateChildControls();

			EnsureChildControls();

			//create the tree control
			TreePickerControl = new CustomTreeControl
				{
					ID = "TreePicker",
					IsDialog = true,
					ShowContextMenu = false,
					DialogMode = TreeDialogModes.id,
					Height = Unit.Pixel(ControlHeight),
					StartNodeID = StartNodeId
				};

			//create the hidden field
			PickedValue = new HiddenField { ID = "PickedValue" };

			//create the right column
			RightColumn = new HtmlGenericControl("div") { ID = "RightColumn" };
			RightColumn.Attributes.Add("class", "right propertypane");

			//create the repeater
			SelectedValues = new Repeater
				{
					//EnableViewState = false,
					ID = "SelectedValues",
					ItemTemplate = new SelectedItemsTemplate()
				};

			SelectedValues.ItemDataBound += SelectedValues_ItemDataBound;

			//add the repeater to the right column
			RightColumn.Controls.Add(SelectedValues);

			MinItemsValidator = new CustomValidator()
									{
										ID = "MinItemsValidator",
										ErrorMessage =
											string.Format(MNTPResources.Val_MinItemsInvalid, MinNodeCount)
									};
			MinItemsValidator.ServerValidate += new ServerValidateEventHandler(MinItemsValidator_ServerValidate);

			//add the controls
			this.Controls.Add(MinItemsValidator);
			this.Controls.Add(TreePickerControl);
			this.Controls.Add(PickedValue);
			this.Controls.Add(RightColumn);
		}
        
		/// <summary>
		/// Ensure the repeater is data bound
		/// </summary>
		public override void DataBind()
		{
			base.DataBind();
			SelectedValues.DataBind();
		}

		void MinItemsValidator_ServerValidate(object source, ServerValidateEventArgs args)
		{
			args.IsValid = true;
			if (MinNodeCount > 0 && SelectedIds.Length < MinNodeCount)
			{
				args.IsValid = false;
			}
		}

		/// <summary>
		/// Event handler for the selected node repeater. 
		/// This will fill in all of the text values, icons, etc.. for nodes based on their ID.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void SelectedValues_ItemDataBound(object sender, RepeaterItemEventArgs e)
		{
			var liSelectNode = (HtmlGenericControl)e.Item.FindControl("SelectedNodeListItem");
			var lnkSelectNode = (HtmlAnchor)e.Item.FindControl("SelectedNodeLink");
			var litSelectNodeName = (Literal)e.Item.FindControl("SelectedNodeText");
			var infoButton = (HtmlAnchor)e.Item.FindControl("InfoButton");

			//hide the info button if tooltips are hidden
			if (!ShowToolTips)
			{
				infoButton.Style.Add(HtmlTextWriterStyle.Display, "none");
			}

			var thisNode = (XElement)e.Item.DataItem;
			int thisNodeId;
			if (int.TryParse(thisNode.Value, out thisNodeId))
			{
				umbraco.cms.businesslogic.Content loadedNode;

				try
				{
					loadedNode = new umbraco.cms.businesslogic.Content(thisNodeId);

					//add the node id
					liSelectNode.Attributes["rel"] = thisNodeId.ToString();
					//add the path to be referenced
					liSelectNode.Attributes["umb:nodedata"] = loadedNode.Path;
					lnkSelectNode.HRef = "javascript:void(0);";
					litSelectNodeName.Text = loadedNode.Text;

					if (loadedNode.IsTrashed)
					{
						//need to flag this to be removed which will be done after all items are data bound
						liSelectNode.Attributes["rel"] = "trashed";
					}
					else
					{
						//we need to set the icon
						if (loadedNode.ContentTypeIcon.StartsWith(".spr"))
							lnkSelectNode.Attributes["class"] += " " + loadedNode.ContentTypeIcon.TrimStart('.');
						else
						{
							//it's a real icon, so make it a background image
							lnkSelectNode.Style.Add(HtmlTextWriterStyle.BackgroundImage,
								string.Format("url('{0}')", IconPath + loadedNode.ContentTypeIcon));
							//set the nospr class since it's not a sprite
							lnkSelectNode.Attributes["class"] += " noSpr";
						}

						//show the media preview if media and allowed
						if (TreeToRender == Umbraco.Core.Constants.Applications.Media && ShowThumbnailsForMedia)
						{
							var imgPreview = (ImageViewer)e.Item.FindControl("ImgPreview");
							//show the thubmnail controls
							imgPreview.Visible = true;

							//add the item class
							var item = (HtmlGenericControl)e.Item.FindControl("Item");
							item.Attributes["class"] += " thumb-item";

							//item.Style.Add(HtmlTextWriterStyle.Height, "50px");
							////make the content sit beside the item
							//var inner = (HtmlGenericControl)e.Item.FindControl("InnerItem");
							//inner.Style.Add(HtmlTextWriterStyle.Width, "224px");

							//check if it's a thumbnail type element, we need to check both schemas
							if (MediaTypesWithThumbnails.Select(x => x.ToUpper())
								.Contains(loadedNode.ContentType.Alias.ToUpper()))
							{
								imgPreview.MediaId = thisNodeId;
								imgPreview.DataBind();
							}
						}
					}

				}
				catch (ArgumentException)
				{
					//the node no longer exists, so we display a msg
					litSelectNodeName.Text = "<i>NODE NO LONGER EXISTS</i>";
				}
			}
		}

		/// <summary>
		/// set the nodekey to the id of this datatype
		/// </summary>
		/// <remarks>
		/// this is how get the xpath out of the cookie to know how the tree knows how to filter things.
		/// generally the nodekey is used for a string id, but we'll use it for something different.
		/// </remarks>
		/// <param name="e"></param>
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			TreePickerControl.NodeKey = this.DataTypeDefinitionId.ToString();

			SavePersistentValuesForTree(XPathFilter);
		}

		/// <summary>
		/// Override render to control the exact output of what is rendered this includes instantiating the jquery plugin
		/// </summary>
		/// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> object that receives the server control content.</param>
		/// <remarks>
		/// Generally i don't like to do this but there's a few div's, etc... to render so this makes more sense.
		/// </remarks>
		protected override void Render(HtmlTextWriter writer)
		{
			//<div class="multiTreePicker">
			//    <div class="header propertypane">
			//        <div>Select items</div>
			//    </div>
			//    <div class="left propertypane">        
			//        <umb:tree runat="server" ID="TreePickerControl" 
			//            CssClass="myTreePicker" Mode="Standard" 
			//            DialogMode="id" ShowContextMenu="false" 
			//            IsDialog="true" TreeType="content" />
			//    </div>
			//    <div class="right propertypane">
			//    </div>
			//</div>

			RenderTooltip(writer);

			writer.AddAttribute("class", (!MinItemsValidator.IsValid ? "error " : "") + "multiNodePicker clearfix");
			writer.AddAttribute("id", this.ClientID);
			writer.RenderBeginTag(HtmlTextWriterTag.Div);

			writer.AddAttribute("class", "header propertypane");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			writer.Write("Select Items");
			writer.RenderEndTag();
			writer.RenderEndTag();

			writer.AddAttribute("class", "left propertypane");
			writer.AddStyleAttribute(HtmlTextWriterStyle.Height, ((ControlHeight + 10).ToString() + "px"));
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			//add the tree control here
			TreePickerControl.RenderControl(writer);
			writer.RenderEndTag();

			RightColumn.RenderControl(writer);

			//render the hidden field
			PickedValue.RenderControl(writer);

			writer.RenderEndTag(); //end multiNodePicker div

			var tooltipAjaxUrl = IOHelper.ResolveUrl(SystemDirectories.Umbraco) + @"/controls/Tree/CustomTreeService.asmx/GetNodeInfo";

			//add jquery window load event to create the js tree picker
			var jsMethod = string.Format("jQuery('#{0}').MultiNodeTreePicker('{1}', {2}, '{3}', {4}, {5}, '{6}', '{7}');",
				TreePickerControl.ClientID,
				this.ClientID,
				MaxNodeCount,
				tooltipAjaxUrl,
				ShowToolTips.ToString().ToLower(),
				(TreeToRender == Umbraco.Core.Constants.Applications.Media && ShowThumbnailsForMedia).ToString().ToLower(),
				IOHelper.ResolveUrl(SystemDirectories.Umbraco),
				TreeToRender);
			var js = "jQuery(window).load(function() { " + jsMethod + " });";

			writer.WriteLine("<script type='text/javascript'>" + js + "</script>");

		}

		/// <summary>
		/// converts a list of Ids to the XDocument structure
		/// </summary>
		/// <param name="val">The value.</param>
		/// <returns></returns>
		private XDocument ConvertToXDocument(IEnumerable<string> val)
		{
			if (val.Count() > 0)
			{
				return new XDocument(new XElement("MultiNodePicker",
					new XAttribute("type", TreeToRender),
					val.Select(x => new XElement("nodeId", x.ToString()))));
			}
			else
			{
				//return null to support recursive values
				return null;

				//return an empty node set
				//return new XDocument(new XElement("MultiNodePicker"));
			}
		}

		/// <summary>
		/// this will render the tooltip object on the page so long as another 
		/// one hasn't already been registered. There should only be one tooltip.
		/// </summary>
		private void RenderTooltip(HtmlTextWriter writer)
		{
			if (this.Page.Items.Contains("MNTPTooltip"))
			{
				return;
			}

			//render the tooltip holder
			//<div class="tooltip">
			//  <div class="throbber"></div>
			//  <div class="tooltipInfo"></div>
			//</div>
			//this.Page.Controls.AddAt(0, new LiteralControl("<div id='MNTPTooltip'><div class='throbber'></div><div class='tooltipInfo'></div></div>"));
			writer.AddAttribute("id", "MNTPTooltip");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			writer.AddAttribute("class", "throbber");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			writer.RenderEndTag(); //end throbber
			writer.AddAttribute("class", "tooltipInfo");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			writer.RenderEndTag(); //end tooltipInfo
			writer.RenderEndTag(); //end tooltipo

			//ensure we add this to our page items so it's not duplicated
			this.Page.Items.Add("MNTPTooltip", true);
		}

		/// <summary>
		/// This will update the multi-node tree picker data which is used to store
		/// the xpath data and xpath match type for this control id.
		/// </summary>
		/// <param name="xpath">The xpath.</param>
		/// <remarks>
		/// This will save the data into a cookie and also into the request cookie. It must save
		/// it to both locations in case the request cookie has been changed and the request cookie
		/// is different than the response cookie.
		/// </remarks>
		private void SavePersistentValuesForTree(string xpath)
		{

			//create the output cookie with all of the values of the request cookie

			var newCookie = HttpContext.Current.Response.Cookies[MNTP_DataType.PersistenceCookieName] ?? new HttpCookie(MNTP_DataType.PersistenceCookieName);

			//store the xpath for this data type definition
			newCookie.MntpAddXPathFilter(this.DataTypeDefinitionId, xpath);
			//store the match type
			newCookie.MntpAddXPathFilterType(this.DataTypeDefinitionId, XPathFilterMatchType);
			//store the start node id
			newCookie.MntpAddStartNodeId(this.DataTypeDefinitionId, StartNodeId);
			//store the start node selection type
			newCookie.MntpAddStartNodeSelectionType(this.DataTypeDefinitionId, StartNodeSelectionType);
			//store the start node xpath expression type
			newCookie.MntpAddStartNodeXPathExpressionType(this.DataTypeDefinitionId, StartNodeXPathExpressionType);
			//store the start node xpath expression
			newCookie.MntpAddStartNodeXPathExpression(this.DataTypeDefinitionId, StartNodeXPathExpression);
			//store the current editing node if found
			if (!string.IsNullOrEmpty(HttpContext.Current.Request["id"]))
			{
				var id = 0;
				if (int.TryParse(HttpContext.Current.Request["id"], out id))
				{
					newCookie.MntpAddCurrentEditingNode(this.DataTypeDefinitionId, id);
				}
			}

			HttpContext.Current.Response.Cookies.Add(newCookie);

			//add it to the request cookies too, thus overriding any old data
			if (HttpContext.Current.Request.Cookies[MNTP_DataType.PersistenceCookieName] != null && HttpContext.Current.Request.Cookies[MNTP_DataType.PersistenceCookieName].Values.Count > 0)
			{
				//remove the incoming one and replace with new one
				HttpContext.Current.Request.Cookies.Remove(MNTP_DataType.PersistenceCookieName);
			}
			HttpContext.Current.Request.Cookies.Add(newCookie);

		}

		/// <summary>
		/// A reference path to where the icons are actually stored as compared to where the tree themes folder is
		/// </summary>
		private static readonly string IconPath = IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "/images/umbraco/";
	}
}
