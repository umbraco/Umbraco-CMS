<%@ Page Language="c#" MasterPageFile="../masterpages/umbracoDialog.Master" Codebehind="create.aspx.cs" AutoEventWireup="True" Inherits="umbraco.dialogs.create" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>


<asp:Content ContentPlaceHolderID="head" runat="server">
  <script language="javascript" type="text/javascript">

			// Hack needed for the way the content picker iframe works
			var tempOpener = "";

			function handleOpener() {
				tempOpener = window.opener;
				window.opener = null;
			}
			
			function revertOpener() {
				window.opener = tempOpener;
			}

			function dialogHandler(id) {
				document.getElementById("nodeId").value = id;
				document.getElementById("ok").disabled = false;
				// Get node name by xmlrequest
				if (id > 0) {
				    umbraco.presentation.webservices.CMSNode.GetNodeName('<%=umbraco.BasePages.BasePage.umbracoUserContextID%>', id, updateName);
					}
				else			
					jQuery("#pageName").html("<p><strong><%=umbraco.ui.Text(umbraco.helper.Request("app"))%></strong> <%= umbraco.ui.Text("moveOrCopy","nodeSelected") %></p>");
					jQuery("#pageNameHolder").attr("class","success");
			}
			
			function updateName(result) {			  
			  jQuery("#pageName").html("<p><strong>" + result + "</strong> <%= umbraco.ui.Text("moveOrCopy","nodeSelected") %></p>");
				jQuery("#pageNameHolder").attr("class","success");
			}
			

function doSubmit() {document.Form1["ok"].click()}

		function execCreate()
		{
			var nodeType;
			var rename;
			for (var i=0;i<document.forms[0].length;i++) {
				if (document.forms[0][i].name.indexOf('nodeType') > -1)
					nodeType = document.forms[0][i];
				else if (document.forms[0][i].name.indexOf('rename') > -1)
					rename = document.forms[0][i];
			}
			
			if (rename.value != '') {
			  parent.window.returnValue = document.getElementById("path").value + "|" + document.getElementById("nodeId").value + "|" + nodeType.value + '--- ' + rename.value;
			  parent.window.close()
			} else 
				if (nodeType.value == '') 
					alert('<%=umbraco.ui.Text("errors", "missingType", this.getUser())%>');
				else
					alert('<%=umbraco.ui.Text("errors", "missingTitle", this.getUser())%>');
		}

	var functionsFrame = this;
	var tabFrame = this;
	var isDialog = true;
	var submitOnEnter = true;
	</script>
</asp:Content>

<asp:Content ContentPlaceHolderID="body" runat="server">
    <input type="hidden" id="nodeId" name="nodeId" value="<%=umbraco.helper.Request("nodeId")%>" />
    <input type="hidden" id="path" name="path" value="" runat="server" />
    
    <asp:Literal ID="FeedBackMessage" runat="server" />
    
    <cc1:Pane ID="pane_chooseNode" runat="server">
      <cc1:PropertyPanel runat="server">
      <iframe frameborder="0" src="../TreeInit.aspx?app=<%=umbraco.helper.Request("app")%>&amp;isDialog=true&amp;dialogMode=id&amp;contextMenu=false"
                style="overflow: auto; width: 100%; border: none; position: relative; height: 250px;
                background-color: white"></iframe>
      </cc1:PropertyPanel>
    </cc1:Pane>
    
    <asp:Panel runat="server" ID="panel_buttons">
      <div class="notice" id="pageNameHolder" style="margin-top: 10px;"><p id="pageName"><%= umbraco.ui.Text("moveOrCopy","noNodeSelected") %></p></div>
      
      <div style="padding-top: 10px;" class="guiDialogNormal">
      <input type="button" id="ok" value="<%=umbraco.ui.Text("ok")%>" onclick="revertOpener(); document.location.href = 'create.aspx?nodeType=<%=umbraco.helper.Request("nodeType")%>&app=<%=umbraco.helper.Request("app")%>&nodeId=' + document.getElementById('nodeId').value"
        disabled="true" style="width: 100px" />
        &nbsp; <em><%= umbraco.ui.Text("or") %></em>&nbsp; <a href="#" style="color: blue" onclick="UmbClientMgr.mainWindow().closeModal()"><%=umbraco.ui.Text("cancel")%></a>
      </div>
    </asp:Panel>
    
    
    <cc1:Pane ID="pane_chooseName" Visible="false" runat="server">
      <cc1:PropertyPanel runat="server">
        <asp:PlaceHolder ID="phCreate" runat="server"></asp:PlaceHolder>   
      </cc1:PropertyPanel>
    </cc1:Pane>

    
    <script type="text/javascript">
		  <%if (umbraco.helper.Request("nodeId") == "")
			  Response.Write("handleOpener();");
		  %>
    </script>
</asp:Content>