<%@ Page MasterPageFile="../../masterpages/umbracoDialog.Master" Language="C#" AutoEventWireup="true" CodeBehind="importForm.aspx.cs" Inherits="Umbraco.Forms.UI.Dialogs.importForm" %>



<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="body">
    <input id="tempFile" type="hidden" name="tempFile" runat="server" />

    <asp:Literal ID="FeedBackMessage" runat="server" />

    <table class="propertyPane" id="Table1" cellspacing="0" cellpadding="4" width="360" border="0" runat="server">
      <tr>
        <td class="propertyContent" colspan="2">
          <asp:Panel ID="Wizard" runat="server" Visible="True">
            <p>
            <span class="guiDialogNormal">
              To import a Contour form, find the ".ucf" file on your computer by clicking the "Browse" button and click "Import" (you'll be asked for confirmation on the next screen) 
            </span>
            </p>
            
            <p>
            <input id="documentTypeFile" type="file" runat="server" />
            </p>
            
            
            <asp:Button ID="submit" runat="server" Text="Import" onclick="submit_Click"></asp:Button> <em><%= umbraco.ui.Text("or") %></em> <a href="#" onclick="<% if (Umbraco.Forms.Core.CompatibilityHelper.IsVersion4dot5OrNewer){%>UmbClientMgr.closeModalWindow()<%}else{%>top.closeModal()<%}%>; return false;"><%= umbraco.ui.Text("cancel") %></a>
          </asp:Panel>
          
          
          <asp:Panel ID="Confirm" runat="server" Visible="False">
            <strong>
              Name
              :</strong>
            <asp:Literal ID="name" runat="server"></asp:Literal>
            <br />
            <strong>
              Fields
              :</strong>
            <asp:Literal ID="fields" runat="server"></asp:Literal>
            <br />
            <br />
            <asp:Button ID="import" runat="server" Text="Import" onclick="import_Click"></asp:Button>
          </asp:Panel>
          <asp:Panel ID="done" runat="server" Visible="False">
            <asp:Literal ID="nameConfirm" runat="server"></asp:Literal>
            has been imported!
          </asp:Panel>
        </td>
      </tr>
    </table>
    </asp:Content>
