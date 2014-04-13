<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="DlrScripting.ascx.cs" Inherits="Umbraco.Web.UI.Umbraco.Create.DlrScripting" %>
<%@ Import Namespace="umbraco" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<cc1:Pane runat="server">
    <cc1:PropertyPanel runat="server" text="Filename (without extension)">
        <asp:TextBox id="rename" Runat="server" CssClass="bigInput input-large-type input-block-level"></asp:TextBox>
        <asp:RequiredFieldValidator id="RequiredFieldValidator1" ErrorMessage="*" ControlToValidate="rename" runat="server">*</asp:RequiredFieldValidator>
    </cc1:PropertyPanel>

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <cc1:PropertyPanel runat="server" text="Choose a language">
                <asp:ListBox id="filetype" Runat="server" CssClass="bigInput input-large-type input-block-level" Rows="1" 
                    SelectionMode="Single" AutoPostBack="true" OnSelectedIndexChanged="loadTemplates">
                </asp:ListBox>
            </cc1:PropertyPanel>
           <cc1:PropertyPanel runat="server" text="Choose a snippet:">
                <asp:ListBox id="template" Runat="server" CssClass="bigInput input-large-type input-block-level" Rows="1" SelectionMode="Single">
                </asp:ListBox>
           </cc1:PropertyPanel>
        </ContentTemplate>
    </asp:UpdatePanel>

     <cc1:PropertyPanel runat="server">
        <asp:CheckBox ID="createMacro" Runat="server" Checked="true" Text="Create Macro"></asp:CheckBox>
         <asp:CustomValidator ErrorMessage="<br/>A macro already exists with the specified name" ID="MacroExistsValidator" 
            Display="Dynamic" ForeColor="red"
            runat="server" OnServerValidate="MacroExistsValidator_OnServerValidate"/>
    </cc1:PropertyPanel>
</cc1:Pane>

<cc1:Pane runat="server" CssClass="btn-toolbar umb-btn-toolbar">
     <a href="#" class="btn btn-link" onclick="UmbClientMgr.closeModalWindow()"><%=umbraco.ui.Text("cancel")%></a>
     <asp:Button id="sbmt" Runat="server" CssClass="btn btn-primary" Text="Save" onclick="sbmt_Click"></asp:Button>
</cc1:Pane>

<!-- added to support missing postback on enter in IE -->
<asp:TextBox runat="server" style="visibility:hidden;display:none;" ID="Textbox1"/>
<input type="hidden" name="nodeType" value="-1">