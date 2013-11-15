using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using ClientDependency.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using umbraco.DataLayer;

namespace Umbraco.Web.UI.Controls
{
	/// <summary>
	/// Represents the 'insert macro' button when editing a template which includes the drop down list selector
	/// </summary>
	/// <remarks>
	///	Though this would be nicer to do in a UserControl, unfortunatley the way that the ScrollingMenu control is designed it seems that 
	/// we have to insert all items via code and loading a UserControl in dynamically is equally ugly. 
	/// </remarks>
	[ClientDependency(ClientDependencyType.Css, "splitbutton/splitbutton.css", "UmbracoClient")]
	[ClientDependency(ClientDependencyType.Javascript, "splitbutton/jquery.splitbutton.js", "UmbracoClient", Priority = 100)]
	[ClientDependency(ClientDependencyType.Javascript, "splitbutton/InsertMacroSplitButton.js", "UmbracoClient", Priority = 101)] 
	internal class InsertMacroSplitButton : UmbracoControl
	{
		protected LiteralControl ListContainer;

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			EnsureChildControls();
		}

		/// <summary>
		/// The JS callback to display the dialog modal screen to customize the macro to be inserted into the editor if the
		/// macro has parameters.
		/// </summary>
		public string ClientCallbackOpenMacroModel { get; set; }

		/// <summary>
		/// The JS callback method which accepts an 'alias' parameter that is invoked when clicking the macro button
		/// to insert a macro that has no parameters.
		/// </summary>
		public string ClientCallbackInsertMacroMarkup { get; set; }

		protected override void CreateChildControls()
		{
			base.CreateChildControls();

			//var menuItemsId = ClientID + "menuitems";
			//var placeHolderId = ClientID + "sbPlaceholder";

			/*create the list, similar to this, but swap the repeater for real html:
			 
			<div id="macroMenu" style="width: 285px">
				<asp:Repeater ID="rpt_macros" runat="server">
					<ItemTemplate>
						<div class="macro" rel="<%# DataBinder.Eval(Container, "DataItem.macroAlias")%>"
							data-has-params="<%#  DoesMacroHaveSettings(DataBinder.Eval(Container, "DataItem.id").ToString()) %>">
							<%# DataBinder.Eval(Container, "DataItem.macroName")%>
						</div>
					</ItemTemplate>
				</asp:Repeater>
			</div>
			*/

			//Create the drop down menu list first (it is hidden)
			var divMacroItemContainer = new TagBuilder("div");
			divMacroItemContainer.Attributes.Add("style", "width: 285px;display:none;");
			divMacroItemContainer.Attributes.Add("class", "sbMenu");
			var macros = ApplicationContext.DatabaseContext.Database.Fetch<MacroDto>("select id, macroAlias, macroName from cmsMacro order by macroName");
			foreach (var macro in macros)
			{
				var divMacro = new TagBuilder("div");
				divMacro.AddCssClass("macro-item");
				divMacro.Attributes.Add("rel", macro.Alias);
				divMacro.Attributes.Add("data-has-params", DoesMacroHaveParameters(macro.Id).ToString().ToLower());
				divMacro.SetInnerText(macro.Name);
				divMacroItemContainer.InnerHtml += divMacro.ToString();
			}
			
			/*create the button itself, similar to this:
			
			<div id="splitButtonMacro" style="display: inline; height: 23px; vertical-align: top;">
				<a href="javascript:openMacroModal();" class="sbLink">
					<img alt="Insert Macro" src="../images/editor/insMacroSB.png" title="Insert Macro"
						style="vertical-align: top;">
				</a>
			</div>
			
			*/

			var divSplitButtonWrapper = new TagBuilder("div");
			divSplitButtonWrapper.AddCssClass("sbPlaceHolder");
			divSplitButtonWrapper.Attributes.Add("id", ClientID + "sbPlaceholder");
			var divButton = new TagBuilder("div");
			divButton.Attributes.Add("style", "display: inline; height: 23px; vertical-align: top;");
			var aButton = new TagBuilder("a");
			aButton.Attributes.Add("href", "#"); //will be bound with jquery
			aButton.AddCssClass("sbLink");
			var imgButton = new TagBuilder("img");
			imgButton.Attributes.Add("alt", "Insert Macro");
			imgButton.Attributes.Add("src", this.ResolveUrl(SystemDirectories.Umbraco + "/images/editor/insMacroSB.png"));
			imgButton.Attributes.Add("title", "Insert Macro");
			imgButton.Attributes.Add("style", "vertical-align: top;");
			aButton.InnerHtml = imgButton.ToString();
			divButton.InnerHtml = aButton.ToString();
			divSplitButtonWrapper.InnerHtml = divButton.ToString();

			ListContainer = new LiteralControl(divMacroItemContainer.ToString() + divSplitButtonWrapper.ToString());
			Controls.Add(ListContainer);

//			Page.ClientScript.RegisterStartupScript(
//				typeof(InsertMacroSplitButton), 
//				ClientID,
//				@"jQuery(document).ready(function() {
//					jQuery('#" + placeHolderId + " a.sbLink').splitbutton({menu:'#" + menuItemsId + "'}); " +
//				"});",
//				true);

			Page.ClientScript.RegisterStartupScript(
				typeof(InsertMacroSplitButton),
				typeof(InsertMacroSplitButton).Name, //same key for all instancees, we should only render once
				@"jQuery(document).ready(function() {
						var splitButton = new Umbraco.Controls.InsertMacroSplitButton({
							openMacroModel: " + ClientCallbackOpenMacroModel + @",
							insertMacroMarkup: " + ClientCallbackInsertMacroMarkup + @"
						});
						splitButton.init();
				});",
				true);
		}


		private bool DoesMacroHaveParameters(int macroId)
		{
            return ApplicationContext.DatabaseContext.Database.ExecuteScalar<int>(string.Format("SELECT COUNT(*) from cmsMacroProperty where macro = {0}", macroId)) > 0;
		}
	}
}
