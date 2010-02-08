<%@ Page Language="c#" MasterPageFile="../../masterpages/umbracoPage.Master" Title="Assembly Browser" Codebehind="assemblyBrowser.aspx.cs" AutoEventWireup="True"
  Inherits="umbraco.developer.assemblyBrowser" %>
<%@ Register TagPrefix="wc1" Namespace="umbraco.controls" Assembly="umbraco" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>



<asp:Content ContentPlaceHolderID="body" runat="server">
<h3 style="MARGIN-LEFT: 0px"><asp:Label id="AssemblyName" runat="server"></asp:Label></h3>
<asp:Panel id="ChooseProperties" runat="server">
					<p class="guiDialogTiny">The following list shows the Public Properties from the 
						Control. By checking the Properties and click the "Save Properties" button at 

						the bottom, umbraco will create the corresponding Macro Elements.</p>
					<asp:CheckBoxList id="MacroProperties" runat="server"></asp:CheckBoxList>
					<p>
						<asp:Button id="Button1" runat="server" Text="Save Properties" onclick="Button1_Click"></asp:Button>
					</p>
</asp:Panel>

<asp:Panel id="ConfigProperties" runat="server" Visible="False">
				    <p class="guiDialogNormal"><strong>The following Macro Parameters was added:</strong><br /></p>
				    <ul class="guiDialogNormal"><asp:Literal ID="resultLiteral" runat="server"></asp:Literal></ul>
				    <p class="guiDialogNormal">
				    <span style="color: Green"><strong>Important:</strong> You might need to reload the macro to see the changes.</span><br /><br />

				    </p>


</asp:Panel>

</asp:Content>
