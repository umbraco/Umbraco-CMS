<%@ Page Language="c#" Codebehind="xsltChooseExtension.aspx.cs" MasterPageFile="../../masterpages/umbracoDialog.Master"  AutoEventWireup="True"
  Inherits="umbraco.developer.xsltChooseExtension" %>
<%@ Import Namespace="Umbraco.Web" %>  <%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
  
<asp:Content ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        function returnResult() {
            var result = document.getElementById('<%= assemblies.ClientID %>').value + ":" + document.getElementById('selectedMethod').value + "(";
            for (var i = 0; i < document.forms[0].length - 1; i++) {
                if (document.forms[0][i].name.indexOf('param') > -1)
                    result = result + "'" + document.forms[0][i].value + "', ";
            }
            if (result.substring(result.length - 1, result.length) == " ")
                result = result.substring(0, result.length - 2);
            result = result + ")";
            
            document.location = 'xsltInsertValueOf.aspx?objectId=<%=Request.CleanForXss("objectId")%>&value=' + result;
        }
    </script>

<style type="text/css">
div.code{padding: 7px 0px 7px 0px;  font-family: Consolas,courier;}
div.code input{border: none; background:#F6F6F9; color: #000; padding: 5px; font-family: Consolas,courier;}
</style>

</asp:Content> 

<asp:Content ContentPlaceHolderID="body" runat="server">
<cc1:Pane runat="server" Text="Choose xslt extension">
  <cc1:PropertyPanel runat="server">
      <asp:DropDownList ID="assemblies" runat="server" Width="200px" />
      <asp:DropDownList ID="methods" runat="server" Width="400px"/>
  </cc1:PropertyPanel>
  <cc1:PropertyPanel runat="server">
          <asp:PlaceHolder ID="PlaceHolderParamters" runat="server"></asp:PlaceHolder>
  </cc1:PropertyPanel>
</cc1:Pane>

<p>
  <asp:Button ID="bt_insert" OnClientClick="returnResult(); return false;" Enabled="false" runat="server" Text="Insert" /> <em><%= umbraco.ui.Text("or") %></em> <a href="xsltInsertValueOf.aspx"><%= umbraco.ui.Text("cancel") %></a>
</p> 

</asp:Content>