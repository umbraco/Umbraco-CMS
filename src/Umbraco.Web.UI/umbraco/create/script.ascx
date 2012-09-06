<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="script.ascx.cs" Inherits="umbraco.presentation.umbraco.create.script" %>

<input type="hidden" name="nodeType" value="-1"/>

Name: <asp:RequiredFieldValidator id="RequiredFieldValidator1" ErrorMessage="*" ControlToValidate="rename" runat="server">*</asp:RequiredFieldValidator><br />
<asp:TextBox id="rename" Runat="server" Width="350" CssClass="bigInput"></asp:TextBox>

<div style="MARGIN-TOP: 10px">
Type:<br />
<asp:ListBox id="scriptType" Runat="server" Width="350" CssClass="bigInput" Rows="1" SelectionMode="Single">
	<asp:ListItem Selected="True" Value="">Folder</asp:ListItem>
</asp:ListBox>
</div>

<!-- added to support missing postback on enter in IE -->
<asp:TextBox runat="server" style="visibility:hidden;display:none;" ID="Textbox1"/>

<div style="MARGIN-TOP: 15px;">
<asp:Button id="sbmt" Runat="server" style="MARGIN-TOP: 14px" Width="90"></asp:Button>
<em> or </em>  
<a href="#" style="color: Blue; margin-left: 6px;" onclick="UmbClientMgr.closeModalWindow()"><%=umbraco.ui.Text("cancel")%></a>
</div>