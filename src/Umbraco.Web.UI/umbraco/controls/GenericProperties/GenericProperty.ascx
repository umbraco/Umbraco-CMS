<%@ Control Language="c#" AutoEventWireup="True" Codebehind="GenericProperty.ascx.cs" Inherits="umbraco.controls.GenericProperties.GenericProperty" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<li id="<%=this.FullId%>" onMouseDown="activeDragId = this.id;">
	<div class="propertyForm" id="<%=this.FullId%>_form">

			<div id="desc<%=this.ClientID%>" ondblclick="expandCollapse('<%=this.ClientID%>'); document.getElementById('<%=this.ClientID%>_tbName').focus();" 
			style="padding: 0px; display: block; margin: 0px;">
	
			<h3 style="padding: 0px; margin: 0px;">
			<asp:ImageButton ID="DeleteButton2" Runat="server"></asp:ImageButton>
			 
			<a href="javascript:expandCollapse('<%=this.ClientID%>');"><img src="<%= Umbraco.Core.IO.IOHelper.ResolveUrl( Umbraco.Core.IO.SystemDirectories.Umbraco )%>/images/expand.png" style="FLOAT: right"/>
			  <asp:Literal ID="FullHeader" Runat="server"></asp:Literal>
			</a>
			
			</h3>
			</div>
			
			<div id="edit<%=this.ClientID%>"  style="DISPLAY: none;">
			
			<h3 style="padding: 0px; margin: 0px;">
			  <asp:ImageButton ID="DeleteButton" Runat="server"></asp:ImageButton>
			  <a href="javascript:expandCollapse('<%=this.ClientID%>');"><img src="<%= Umbraco.Core.IO.IOHelper.ResolveUrl(  Umbraco.Core.IO.SystemDirectories.Umbraco )%>/images/collapse.png" id="<%=this.ClientID%>_fold" style="FLOAT: right"  />
			  Edit "<asp:Literal ID="Header" Runat="server"></asp:Literal>"</a>
			</h3>
			
			<cc1:Pane ID="Pane1" runat="server">
        <cc1:PropertyPanel ID="PropertyPanel1" runat="server" Text="Name">
          <asp:TextBox id="tbName" runat="server" CssClass="propertyFormInput"></asp:TextBox>
        </cc1:PropertyPanel>
        
        <cc1:PropertyPanel ID="PropertyPanel2" runat="server" Text="Alias">
         <asp:TextBox id="tbAlias" runat="server" CssClass="propertyFormInput"></asp:TextBox>
        </cc1:PropertyPanel>
        
        <cc1:PropertyPanel ID="PropertyPanel3" runat="server" Text="Type">
          <asp:DropDownList id="ddlTypes" runat="server" CssClass="propertyFormInput"></asp:DropDownList>
        </cc1:PropertyPanel>
        
        <cc1:PropertyPanel ID="PropertyPanel4" runat="server" Text="Tab">
          <asp:DropDownList id="ddlTab" runat="server" CssClass="propertyFormInput"></asp:DropDownList>
        </cc1:PropertyPanel>
        
        <cc1:PropertyPanel ID="PropertyPanel5" runat="server" Text="Mandatory">
          <asp:CheckBox id="checkMandatory" runat="server"></asp:CheckBox>
        </cc1:PropertyPanel>
        
        <cc1:PropertyPanel ID="PropertyPanel6" runat="server" Text="Validation">
          <asp:TextBox id="tbValidation" runat="server" TextMode="MultiLine" CssClass="propertyFormInput"></asp:TextBox><br />
           <asp:CustomValidator runat="server" ID="cvValidation" ControlToValidate="tbValidation" ErrorMessage="Invalid expression" ClientValidationFunction="ValidateValidation" /><br />
          <small><asp:HyperLink ID="validationLink" runat="server">Search for a regular expression</asp:HyperLink></small>
        </cc1:PropertyPanel>
        
        <cc1:PropertyPanel ID="PropertyPanel7" runat="server" Text="Description">
          <asp:TextBox id="tbDescription" runat="server" CssClass="propertyFormInput" TextMode="MultiLine"></asp:TextBox>
        </cc1:PropertyPanel>
    </cc1:Pane>
		</div>
		
			
		</div>
</li>
<script type="text/javascript">
    $(function () {
        checkAlias('<%=tbAlias.ClientID%>');
        var textbox = $('#<%=tbAlias.ClientID%>');
        var ul = textbox.closest('ul');
        if (ul.hasClass('addNewProperty')) {
            duplicatePropertyNameAsSafeAlias('<%=tbName.ClientID%>', '<%=tbAlias.ClientID%>');
        }
    });
    function ValidateValidation(sender, args) {
        try {
            var patt = new RegExp(args.Value);
            args.IsValid = true;

        } catch (e) {
            args.IsValid = false;
        }
    }
</script>