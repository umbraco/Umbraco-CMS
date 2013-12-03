<%@ Page Language="C#" MasterPageFile="../masterpages/umbracoDialog.Master" AutoEventWireup="true" Inherits="umbraco.presentation.dialogs.emptyTrashcan" %>
<%@ Import Namespace="System.Globalization" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<asp:Content ContentPlaceHolderID="head" runat="server">
		<script type="text/javascript">
		
		    var recycleBinType = '<%=BinType.ToString()%>';
		    var emptyTotal = '<%= umbraco.cms.businesslogic.RecycleBin.Count(BinType).ToString(CultureInfo.InvariantCulture)%>';
		    
		    function emptyRecycleBin() {
    			jQuery('#formDiv').hide();
    			jQuery('#buttons').hide(); 
	    		jQuery('#animation').show(); 
		    	jQuery('#anim').attr("src","<%=umbraco.GlobalSettings.ClientPath%>/images/progressBar.gif");
		    	
		    	// call the empty trashcan webservice
		    	umbraco.presentation.webservices.trashcan.EmptyTrashcan(recycleBinType);

         // wait one second to start the status update
         setTimeout('updateStatus();', 1000);
		    }
		    
		    function updateStatus() {
		        umbraco.presentation.webservices.trashcan.GetTrashStatus(updateStatusLabel, failure);
		    }
		    
		    function failure(retVal) {
		        alert('error: ' + retVal);
		    }
		    
		    function updateStatusLabel(retVal) {
                jQuery('#statusLabel').html("<strong>" + retVal + " <%=umbraco.ui.Text("remaining")%></strong>");            

                if (retVal != '' && retVal != '0') {
                    setTimeout('updateStatus();', 500);
                } else {
                    jQuery('#div_form').hide();
                    jQuery('#notification').show();
                    jQuery('#notification').html("<p><%=umbraco.ui.Text("defaultdialogs", "recycleBinIsEmpty")%> </p> <p><a href='#' onclick='UmbClientMgr.closeModalWindow()'><%= umbraco.ui.Text("defaultdialogs", "closeThisWindow")%></a></p>");
                    UmbClientMgr.mainTree().reloadActionNode();
                }
                
		    }
		</script>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="body">
    
 		
  	<div class="success" id="notification" style="display: none;"></div>
		
		<div id="div_form">
		<cc1:Pane id="pane_form" runat="server">
		<cc1:PropertyPanel runat="server">
		
		
		
		<div id="animation" align="center" style="display: none;">
		<p><%= umbraco.ui.Text("defaultdialogs", "recycleBinDeleting")%></p>
		
		<cc1:ProgressBar ID="progbar" runat="server" Title="Please wait..." />
		<br />
		<span class="guiDialogTiny" id="statusLabel"><%=umbraco.ui.Text("deleting", UmbracoUser)%></span>
		</div>
	  	  	  
	  <div id="formDiv">
	    <p><%= umbraco.ui.Text("defaultdialogs", "recycleBinWarning")%></p>
		   <input type="checkbox" id="confirmDelete" onclick="$get('ok').disabled = !this.checked;" /> <label for="confirmDelete"><%=umbraco.ui.Text("defaultdialogs", "confirmEmptyTrashcan", umbraco.cms.businesslogic.RecycleBin.Count(BinType).ToString(CultureInfo.InvariantCulture), UmbracoUser)%></label>
		</div>
	  </cc1:PropertyPanel>
	  </cc1:Pane>
	  
		<br />
		<div id="buttons">
		<input type="button" ID="ok" value="<%=umbraco.ui.Text("actions", "emptyTrashcan", UmbracoUser) %>" class="guiInputButton" onclick="if ($get('confirmDelete').checked) {emptyRecycleBin();}" disabled="true" />  
		<em><%= umbraco.ui.Text("or") %></em> 
    <a href="#" onclick="UmbClientMgr.closeModalWindow();">
      <%=umbraco.ui.Text("cancel")%>
    </a>
		</div>
		</div>
</asp:Content>