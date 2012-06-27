<%@ Page Language="c#" MasterPageFile="../masterpages/umbracoPage.Master" CodeBehind="EditMemberGroup.aspx.cs"
  AutoEventWireup="True" Inherits="umbraco.presentation.members.EditMemberGroup" %>

<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<asp:Content runat="server" ContentPlaceHolderID="body">
<input type="hidden" id="memberGroupName" runat="server" />
  <cc1:UmbracoPanel ID="Panel1" runat="server" hasMenu="true">
    <cc1:Pane ID="Pane7" Style="padding-right: 10px; padding-left: 10px; padding-bottom: 10px;
      padding-top: 10px; text-align: left" runat="server" Height="44px" Width="528px">
      <table id="Table1" width="100%">
        <tr>
          <th width="15%">
            <%=umbraco.ui.Text("name", base.getUser())%>
          </th>
          <td>
            <asp:TextBox ID="NameTxt" Width="200px" runat="server"></asp:TextBox>
          </td>
        </tr>
      </table>
    </cc1:Pane>
  </cc1:UmbracoPanel>
</asp:Content>
