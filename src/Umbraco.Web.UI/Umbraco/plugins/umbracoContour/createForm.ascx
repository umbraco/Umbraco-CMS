<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="createForm.ascx.cs" Inherits="Umbraco.Forms.UI.Dialogs.createForm" %>

<p style="margin-top:0px;padding-top:0px;margin-bottom:4px;padding-bottom:0px;"><small>To create a new form, simply give it a name and click the Create button. By Choosing a template, some relevant fields are automaticly added to your form (they can be edited afterwards). <a href="http://www.umbraco.tv" target="_blank">How does this work (video)?</a></small></p>

Name: <asp:RequiredFieldValidator id="RequiredFieldValidator1" ErrorMessage="*" ControlToValidate="rename" runat="server">*</asp:RequiredFieldValidator><br />
<input type="hidden" name="nodeType" value="-1">
<asp:TextBox id="rename" Runat="server" CssClass="bigInput" Width="350"></asp:TextBox>


<div style="MARGIN-TOP: 10px">Choose a template (optional):<br />
<asp:ListBox id="formTemplate" Runat="server" Width="350" CssClass="bigInput" Rows="1" SelectionMode="Single">
	<asp:ListItem Value="">None</asp:ListItem>
</asp:ListBox>
</div>


<!-- added to support missing postback on enter in IE -->
<asp:TextBox runat="server" style="visibility:hidden;display:none;" ID="Textbox1"/>


<div style="MARGIN-TOP: 15px;">
<asp:Button id="sbmt" Runat="server" style="MARGIN-TOP: 14px" Width="90" onclick="sbmt_Click"></asp:Button>
&nbsp; <em><%= umbraco.ui.Text("or") %></em> &nbsp;
<a href="#" style="color: blue"  onClick="<% if (Umbraco.Forms.Core.CompatibilityHelper.IsVersion4dot5OrNewer){%>UmbClientMgr.closeModalWindow()<%}else{%>top.closeModal()<%}%>"><%=umbraco.ui.Text("cancel")%></a>
</div>