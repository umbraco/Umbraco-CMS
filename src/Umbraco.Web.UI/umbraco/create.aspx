<%@ Page Language="c#" MasterPageFile="masterpages/umbracoDialog.Master" Codebehind="CreateDialog.aspx.cs" AutoEventWireup="True" Inherits="Umbraco.Web.UI.Umbraco.CreateDialog" %>
<%@ Register Namespace="umbraco" TagPrefix="umb" Assembly="umbraco" %>


<asp:Content ContentPlaceHolderID="head" runat="server">
  <script type="text/javascript">
    var preExecute;
    function doSubmit() { document.forms[0].submit(); }

    var functionsFrame = this;
    var tabFrame = this;
    var isDialog = true;
    var submitOnEnter = true;
  </script>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="body">
    <asp:PlaceHolder ID="UI" runat="server"></asp:PlaceHolder>
    <asp:PlaceHolder runat="server" Visible="False" ID="AccessError">
        <div class="error">
            <p>
                The current user does not have access to create this type of object
            </p>
        </div>
    </asp:PlaceHolder>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="footer">
  <script type="text/javascript">
    function setFocusOnText() {
          for (var i = 0; i < document.forms[0].length; i++) {
            if (document.forms[0][i].type == 'text') {
              document.forms[0][i].focus();
              break;
            }
          }
    }
    
    <%if (!IsPostBack) { %>
    setTimeout("setFocusOnText()", 100);
    <%} %>
  </script>
</asp:Content>