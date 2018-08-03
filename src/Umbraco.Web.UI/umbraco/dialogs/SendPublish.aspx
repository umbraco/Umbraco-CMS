<%@ Page language="c#" MasterPageFile="../masterpages/umbracoDialog.Master" AutoEventWireup="True" Inherits="umbraco.dialogs.SendPublish" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<asp:Content ContentPlaceHolderID="body" runat="server">
    <div class="umb-dialog-body form-horizontal">
    
        <cc1:Pane ID="pane_form" runat="server">

            <h5><%=umbraco.ui.Text("editContentSendToPublishText")%></h5>

        </cc1:Pane>
    </div>
    <div class="umb-dialog-footer btn-toolbar umb-btn-toolbar">
        <a href="#" class="btn btn-link" onclick="UmbClientMgr.closeModalWindow()"><%=umbraco.ui.Text("closewindow")%></a>
        
    </div>
		
</asp:Content>
