<%@ Page Language="c#" MasterPageFile="../masterpages/umbracoDialog.Master" CodeBehind="treePicker.aspx.cs"
    AutoEventWireup="True" Inherits="umbraco.dialogs.treePicker" %>

<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="umb2" TagName="Tree" Src="../controls/Tree/TreeControl.ascx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">

    <script type="text/javascript" language="javascript">
    
			function dialogHandler(id) {
			    UmbClientMgr.closeModalWindow(id);
			}			
			
    </script>

    <umb2:Tree runat="server" ID="DialogTree" App='<%#TreeParams.App %>' TreeType='<%#TreeParams.TreeType %>'
        IsDialog="true" ShowContextMenu="false" DialogMode="id" FunctionToCall="dialogHandler" NodeKey='<%#TreeParams.NodeKey %>' />
        
</asp:Content>
