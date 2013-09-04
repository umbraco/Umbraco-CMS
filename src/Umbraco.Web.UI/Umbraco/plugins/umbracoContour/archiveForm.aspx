<%@ Page MasterPageFile="../../masterpages/umbracoDialog.Master" Language="C#" AutoEventWireup="true" CodeBehind="archiveForm.aspx.cs" Inherits="Umbraco.Forms.UI.Dialogs.archiveForm" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="body">


    <asp:Literal ID="FeedBackMessage" runat="server" />

    <table class="propertyPane" id="Table1" cellspacing="0" cellpadding="4" width="360" border="0" runat="server">
      <tr>
        <td class="propertyContent" colspan="2">
          <asp:Panel ID="archive" runat="server" Visible="True">
            <p>
            <span class="guiDialogNormal">
              <% if (Request["unarchive"] == null)
                 { %>
              Archiving your form will remove the form from various lists (form picker, ...) and will place the designer in read only mode.
              <%}
                 else
                 { %>
              Unarchiving your form will make it possible to edit the form again and the form will also appear in various list (form picker, ...) again.
              <% } %>
            </span>
            </p>

            <asp:Button ID="submit" runat="server" Text="Archive" onclick="submit_Click"></asp:Button> <em><%= umbraco.ui.Text("or") %></em> <a href="#" onclick="<% if (Umbraco.Forms.Core.CompatibilityHelper.IsVersion4dot5OrNewer){%>UmbClientMgr.closeModalWindow()<%}else{%>top.closeModal()<%}%>; return false;"><%= umbraco.ui.Text("cancel") %></a>
          </asp:Panel>
          

          <asp:Panel ID="done" runat="server" Visible="False">
           <p>The form '<asp:Literal ID="nameConfirm" runat="server"></asp:Literal>'
            <% if (Request["unarchive"] == null)
                 { %>
              has been archived.
              <%}
                 else
                 { %>
              has been unarchived.
              <% } %></p> 
          </asp:Panel>
        </td>
      </tr>
    </table>
</asp:Content>
