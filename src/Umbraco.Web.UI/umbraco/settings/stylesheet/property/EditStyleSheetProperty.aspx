<%@ Page Language="c#" MasterPageFile="../../../masterpages/umbracoPage.Master" CodeBehind="EditStyleSheetProperty.aspx.cs"
  AutoEventWireup="True" Inherits="Umbraco.Web.UI.Umbraco.Settings.Stylesheet.Property.EditStyleSheetProperty"
  ValidateRequest="False" %>

<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<asp:Content ContentPlaceHolderID="body" runat="server">
  <cc1:UmbracoPanel ID="Panel1" runat="server" Text="Edit stylesheet property" Width="432px" Height="176px" hasMenu="true">
    <cc1:Pane ID="Pane7" CssClass="pane" runat="server">
      <table cellspacing="0" cellpadding="4" border="0">
        <tr>
          <th width="30%">
            <%=umbraco.ui.Text("name", UmbracoUser)%>:
          </th>
          <td class="propertyContent">
            <asp:HiddenField runat="server" ID="OriginalName"/>
            <asp:TextBox ID="NameTxt" Width="350px" runat="server" /><br />
            <small><%=umbraco.ui.Text("stylesheet", "nameHelp", UmbracoUser)%></small>
          </td>
        </tr>
        <tr>
          <th width="30%">
            <%=umbraco.ui.Text("alias", UmbracoUser)%>:
          </th>
          <td class="propertyContent">
            <asp:TextBox ID="AliasTxt" Width="350px" runat="server" /><br />
            <small><%=umbraco.ui.Text("stylesheet", "aliasHelp", UmbracoUser)%></small>
          </td>
        </tr>
        <tr>
          <th width="30%">
            <%=umbraco.ui.Text("styles", UmbracoUser)%>:
          </th>
          <td class="propertyContent">
            <asp:TextBox ID="Content" Style="width: 350px" TextMode="MultiLine" runat="server" />
            <br />
            <br />
          </td>
        </tr>
        <tr>
          <th width="30%">
            <%=umbraco.ui.Text("preview", UmbracoUser)%>:
          </th>
          <td class="propertyContent">
            <div id="preview" style="padding: 10px; border: 1px solid #ccc; width: 330px;">
              <div runat="server" id="prStyles">
                a b c d e f g h i j k l m n o p q r s t u v w x t z
                <br />
                A B C D E F G H I J K L M N O P Q R S T U V W X Y Z<br />
                1 2 3 4 5 6 7 8 9 0 € £ $ % & (.,;:'\"!?)
                <br />
                <br />
                Just keep examining every bid quoted for zinc etchings.
              </div>
            </div>
          </td>
        </tr>
      </table>
    </cc1:Pane>
  </cc1:UmbracoPanel>

    <script type="text/javascript">
        jQuery(document).ready(function () {
            UmbClientMgr.appActions().bindSaveShortCut();
        });
    </script>

</asp:Content>
