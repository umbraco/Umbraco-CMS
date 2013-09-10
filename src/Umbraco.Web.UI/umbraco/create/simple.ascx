<%@ Control Language="c#" AutoEventWireup="True" Codebehind="simple.ascx.cs" Inherits="umbraco.cms.presentation.create.controls.simple" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>


<cc1:Pane runat="server">
        <cc1:PropertyPanel runat="server" Text="Name">
             <asp:TextBox id="rename" CssClass="bigInput input-large-type input-block-level" Runat="server"></asp:TextBox><asp:RequiredFieldValidator id="RequiredFieldValidator1" ErrorMessage="*" ControlToValidate="rename" runat="server">*</asp:RequiredFieldValidator>   
        </cc1:PropertyPanel> 
</cc1:Pane>

<cc1:Pane runat="server" CssClass="btn-toolbar umb-btn-toolbar">
     <a href="#" class="btn btn-link" onclick="UmbClientMgr.closeModalWindow()"><%=umbraco.ui.Text("cancel")%></a>
     <asp:Button id="sbmt" Runat="server" CssClass="btn btn-primary" onclick="sbmt_Click"></asp:Button>
</cc1:Pane>


<!-- added to support missing postback on enter in IE -->
<asp:TextBox runat="server" style="visibility:hidden;display:none;" ID="Textbox1"/>
<input type="hidden" name="nodeType">


