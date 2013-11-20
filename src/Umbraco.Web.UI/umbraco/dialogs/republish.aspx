<%@ Page Language="c#" Codebehind="republish.aspx.cs" MasterPageFile="../masterpages/umbracoDialog.Master" AutoEventWireup="True" Inherits="umbraco.cms.presentation.republish" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<asp:Content ContentPlaceHolderID="head" runat="server">
   <script type="text/javascript">
     function showProgress(button, elementId) {
       var img = document.getElementById(elementId);

       img.style.visibility = "visible";
       button.style.display = "none";
     }
		</script>
</asp:Content>

<asp:Content ContentPlaceHolderID="body" runat="server">
<asp:Panel ID="p_republish" runat="server">
    <div class="propertyDiv">      
          <p><%= umbraco.ui.Text("defaultdialogs", "siterepublishHelp")%> </p>
    </div>
      
    <div id="buttons" class="btn-toolbar umb-btn-toolbar">
        <a href="#" class="btn btn-link" onclick="UmbClientMgr.closeModalWindow();"><%=umbraco.ui.Text("cancel")%></a>
        <asp:Button ID="bt_go" cssclass="btn btn-primary" OnClick="go" OnClientClick="showProgress(document.getElementById('buttons'),'progress'); return true;" runat="server" Text="Republish" />
    </div>     
      
    <div id="progress" style="visibility: hidden;">
		<cc1:ProgressBar ID="progbar" runat="server" Title="Please wait..." />
    </div>
      
    </asp:Panel>
    
    <asp:Panel ID="p_done" Visible="false" runat="server">
     <div class="success">
      <p><%= umbraco.ui.Text("defaultdialogs", "siterepublished")%></p>
      
     </div>
      <input type="button" class="btn btn-primary" onclick="UmbClientMgr.closeModalWindow();" value="Ok" />
    </asp:Panel>
</asp:Content>