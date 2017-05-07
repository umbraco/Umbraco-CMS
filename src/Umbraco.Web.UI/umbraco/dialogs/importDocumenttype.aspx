<%@ Page MasterPageFile="../masterpages/umbracoDialog.Master" Language="c#" Codebehind="importDocumenttype.aspx.cs" AutoEventWireup="false"
  Inherits="umbraco.presentation.umbraco.dialogs.importDocumentType" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="body">
    <input id="tempFile" type="hidden" name="tempFile" runat="server" />

    <asp:Literal ID="FeedBackMessage" runat="server" />

    <table class="propertyPane" id="Table1" cellspacing="0" cellpadding="4" border="0" runat="server">
      <tr>
        <td class="propertyContent" colspan="2">
          <asp:Panel ID="Wizard" runat="server" Visible="True">
            <p>
            <span class="guiDialogNormal">
              <%=umbraco.ui.Text("importDocumentTypeHelp")%>
            </span>
            </p>
            
            <p>
            <input id="documentTypeFile" type="file" runat="server" />
            </p>

            <div class="btn-toolbar umb-btn-toolbar">
                <a href="#" class="btn btn-link" onclick="UmbClientMgr.closeModalWindow(); return false;"><%= umbraco.ui.Text("cancel") %></a>
                <asp:Button ID="submit" runat="server" CssClass="btn btn-primary"></asp:Button>
            </div>
          </asp:Panel>
          
          
          <asp:Panel ID="Confirm" runat="server" Visible="False">
            <strong>
              <%=umbraco.ui.Text("name")%>
              :</strong>
            <asp:Literal ID="dtName" runat="server"></asp:Literal>
            <br />
            <strong>
              <%=umbraco.ui.Text("alias")%>
              :</strong>
            <asp:Literal ID="dtAlias" runat="server"></asp:Literal>
            <br />
            <br />
            <asp:Button ID="import" runat="server" CssClass="btn btn-primary"></asp:Button>
          </asp:Panel>
          <asp:Panel ID="done" runat="server" Visible="False">
            <asp:Literal ID="dtNameConfirm" runat="server"></asp:Literal>
            has been imported!
          </asp:Panel>
        </td>
      </tr>
    </table>
    </asp:Content>
