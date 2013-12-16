<%@ Page Language="c#" MasterPageFile="../masterpages/umbracoPage.Master" ValidateRequest="false" Codebehind="insertMacro.aspx.cs" AutoEventWireup="True"
  Inherits="umbraco.dialogs.insertMacro" Trace="false" %>
<%@ Import Namespace="Umbraco.Web" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<asp:Content ContentPlaceHolderID="head" runat="server">
<script type="text/javascript">
  function saveTreepickerValue(appAlias, macroAlias) {
    var treePicker = window.showModalDialog('treePicker.aspx?app=' + appAlias + '&treeType=' + appAlias, 'treePicker', 'dialogWidth=350px;dialogHeight=300px;scrollbars=no;center=yes;border=thin;help=no;status=no')
    document.forms[0][macroAlias].value = treePicker;
    document.getElementById("label" + macroAlias).innerHTML = "</b><i>updated with id: " + treePicker + "</i><b><br/>";
  }

  var macroAliases = new Array();

  function registerAlias(alias) {
    macroAliases[macroAliases.length] = alias;
  }

  function updateMacro() {
    var macroString = '';

    for (i = 0; i < macroAliases.length; i++) {
      var propertyName = macroAliases[i]
      // Vi opdaterer macroStringen
      if (document.forms[0][macroAliases[i]].type == 'checkbox') {
        if (document.forms[0][macroAliases[i]].checked)
          macroString += propertyName + "=\"1\" ";
        else
          macroString += propertyName + "=\"0\" ";

      } else if (document.forms[0][macroAliases[i]].length) {
        var tempValue = '';
        for (var j = 0; j < document.forms[0][macroAliases[i]].length; j++) {
          if (document.forms[0][macroAliases[i]][j].selected)
            tempValue += document.forms[0][macroAliases[i]][j].value + ', ';
        }
        if (tempValue.length > 2)
          tempValue = tempValue.substring(0, tempValue.length - 2)
        macroString += propertyName + "=\"" + tempValue + "\" ";
      } else {
        macroString += propertyName + "=\"" + pseudoHtmlEncode(document.forms[0][macroAliases[i]].value) + "\" ";
      }
    }

    if (macroString.length > 1)
      macroString = macroString.substr(0, macroString.length - 1);

    if (document.forms[0].macroMode.value == 'edit') {
      var idAliasRef = "";
      if (document.forms[0]["macroAlias"].value != '')
        idAliasRef = " macroAlias=\"" + document.forms[0]["macroAlias"].value;
      else
        idAliasRef = " macroID=\"" + document.forms[0]["macroID"].value;

      top.right.umbracoEditMacroDo("<?UMBRACO_MACRO" + idAliasRef + "\" " + macroString + ">");
    } else {
      top.right.umbracoInsertMacroDo("<?UMBRACO_MACRO macroAlias=\"" + document.forms[0]["macroAlias"].value + "\" " + macroString + ">");
    }


    UmbClientMgr.closeModalWindow();
  }

  function pseudoHtmlEncode(text) {
    return text.replace(/\"/gi, "&amp;quot;").replace(/\</gi, "&amp;lt;").replace(/\>/gi, "&amp;gt;");
  }
  </script>
</asp:Content>

<asp:Content ContentPlaceHolderID="body" runat="server">
 <input type="hidden" name="macroMode" value="<%=Request["mode"]%>" />
    
    <%if (Request["macroID"] != null || Request["macroAlias"] != null) {%>
    
    <input type="hidden" name="macroID" value="<%=Request.CleanForXss("macroID")%>" />
    <input type="hidden" name="macroAlias" value="<%=Request.CleanForXss("macroAlias")%>" />
    
    <div class="macroProperties">
      <cc1:Pane id="pane_edit" runat="server">
        <asp:PlaceHolder ID="macroProperties" runat="server" />
      </cc1:Pane>
    </div>
    <p>
    <input type="button" value="<%=umbraco.ui.Text("general", "ok", UmbracoUser)%>" onclick="updateMacro()" />
    &nbsp; <em> or </em> &nbsp;
     <a href="#" style="color: blue"  onclick="UmbClientMgr.closeModalWindow()"><%=umbraco.ui.Text("general", "cancel", UmbracoUser)%></a>
    </p>
    <%} else {%>
    
    <cc1:Pane id="pane_insert" runat="server">
      <cc1:PropertyPanel runat="server">
          <asp:ListBox Rows="1" ID="macroAlias" runat="server"></asp:ListBox>
      </cc1:PropertyPanel>
    </cc1:Pane>
    <p>
    <input type="submit" value="<%=umbraco.ui.Text("general", "ok", UmbracoUser)%>" />
    &nbsp; <em> or </em> &nbsp;
     <a href="#" style="color: blue"  onclick="UmbClientMgr.closeModalWindow()"><%=umbraco.ui.Text("general", "cancel", UmbracoUser)%></a>
    </p>
    
    <%}%>
</asp:Content>