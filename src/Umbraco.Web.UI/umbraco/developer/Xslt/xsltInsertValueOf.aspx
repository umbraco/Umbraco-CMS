<%@ Page Language="c#" MasterPageFile="../../masterpages/umbracoDialog.Master" Codebehind="xsltInsertValueOf.aspx.cs" AutoEventWireup="True"  Inherits="umbraco.developer.xsltInsertValueOf" %>
<%@ Import Namespace="Umbraco.Web" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>

<asp:Content ContentPlaceHolderID="head" runat="server">
<script type="text/javascript">
  function doSubmit() {

    var checked = "";
    if (document.getElementById('<%= disableOutputEscaping.ClientID %>').checked)
      checked = ' disable-output-escaping="yes"';

    result = '<xsl:value-of select="' + document.getElementById('<%= valueOf.ClientID %>').value + '"' + checked + '/>';

    UmbClientMgr.contentFrame().focus();
    UmbClientMgr.contentFrame().UmbEditor.Insert(result, '', '<%=Request.CleanForXss("objectId")%>');

    UmbClientMgr.closeModalWindow();
  }

  function getExtensionMethod() {
    document.location = 'xsltChooseExtension.aspx?objectId=<%=Request.CleanForXss("objectId")%>';
  }

  function recieveExtensionMethod(theValue) {
    document.getElementById('<%= valueOf.ClientID %>').value = theValue;
  }

  var functionsFrame = this;
  var tabFrame = this;
  var isDialog = true;
  var submitOnEnter = true;

  this.focus();
  </script>

<umb:JsInclude ID="JsInclude1" runat="server" FilePath="js/umbracoCheckKeys.js" PathNameAlias="UmbracoRoot"  />

<style type="text/css">
body{margin: 0px; padding: 0px;}
.propertyItemheader{width: 200px !Important;}
</style>

</asp:Content>

<asp:Content ContentPlaceHolderID="body" runat="server">
<cc1:Pane runat="server" Text="Insert value">
<cc1:PropertyPanel runat="server" Text="Value">
            <asp:TextBox runat="server" ID="valueOf" Width="250px"></asp:TextBox>
            <asp:DropDownList ID="preValues" runat="server" Width="150px"></asp:DropDownList>
            <input type="button" value="Get Extension" onclick="getExtensionMethod();" style="font-size: xx-small">
</cc1:PropertyPanel>
<cc1:PropertyPanel runat="server" Text="Disable output escaping">
            <asp:CheckBox runat="server" ID="disableOutputEscaping"></asp:CheckBox>
</cc1:PropertyPanel>
</cc1:Pane>

<p>
          <input type="button" value="Insert value" onclick="doSubmit();" /> <em><%= umbraco.ui.Text("or") %></em> <a href="#" onclick="UmbClientMgr.closeModalWindow(); return false;"><%= umbraco.ui.Text("cancel") %></a>
</p>
</asp:Content>