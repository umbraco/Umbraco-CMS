<%@ Control Language="c#" AutoEventWireup="True" CodeBehind="GenericProperty.ascx.cs" Inherits="umbraco.controls.GenericProperties.GenericProperty" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<li id="<%=this.FullId%>" onmousedown="activeDragId = this.id;">
    <div class="propertyForm" id="<%=this.FullId%>_form">
        <div id="desc<%=this.ClientID%>" ondblclick="expandCollapse('<%=this.ClientID%>'); document.getElementById('<%=this.ClientID%>_tbName').focus();">
            <div class="header">
                <i class="icon-navigation handle"></i>

                <asp:LinkButton ID="DeleteButton2" runat="server">
                    <i class="btn btn-mini icon-delete"></i> 
                </asp:LinkButton>

                <a href="javascript:expandCollapse('<%=this.ClientID%>');">
			

                    <i class="toggle-button btn btn-mini icon-navigation-down"></i>
                    <asp:Literal ID="FullHeader" runat="server"></asp:Literal>
                </a>
            </div>
        </div>

        <div id="edit<%=this.ClientID%>" style="DISPLAY: none;">
            <div class="header">
                <i class="icon-navigation handle"></i>
                <asp:LinkButton ID="DeleteButton" runat="server">
                      <i class="btn btn-mini icon-delete"></i> 
                </asp:LinkButton>
                <a href="javascript:expandCollapse('<%=this.ClientID%>');">
                    <i class="toggle-button btn btn-mini icon-navigation-up"></i>
                    Edit "<asp:Literal ID="Header" runat="server"></asp:Literal>"
                </a>
            </div>

			<cc1:Pane ID="Pane1" runat="server">
        <cc1:PropertyPanel ID="PropertyPanel1" runat="server" Text="Name">
                    <asp:TextBox ID="tbName" runat="server" CssClass="propertyFormInput prop-name"></asp:TextBox>
                </cc1:PropertyPanel>

        <cc1:PropertyPanel ID="PropertyPanel2" runat="server" Text="Alias">
                    <asp:TextBox ID="tbAlias" runat="server" CssClass="propertyFormInput prop-alias"></asp:TextBox>
            <asp:Label ID="lblAlias" runat="server" ></asp:Label>
                </cc1:PropertyPanel>

        <cc1:PropertyPanel ID="PropertyPanel3" runat="server" Text="Type">
                    <asp:DropDownList ID="ddlTypes" runat="server" CssClass="propertyFormInput"></asp:DropDownList>
                </cc1:PropertyPanel>

        <cc1:PropertyPanel ID="PropertyPanel4" runat="server" Text="Tab">
                    <asp:DropDownList ID="ddlTab" runat="server" CssClass="propertyFormInput"></asp:DropDownList>
                </cc1:PropertyPanel>

        <cc1:PropertyPanel ID="PropertyPanel5" runat="server" Text="Mandatory" >
                    <asp:CheckBox ID="checkMandatory" runat="server"></asp:CheckBox>
                </cc1:PropertyPanel>

        <cc1:PropertyPanel ID="PropertyPanel6" runat="server" Text="Validation">
                    <asp:TextBox ID="tbValidation" runat="server" TextMode="MultiLine" CssClass="propertyFormInput"></asp:TextBox><br />
           <asp:CustomValidator runat="server" ID="cvValidation" ControlToValidate="tbValidation" ErrorMessage="Invalid expression" ClientValidationFunction="ValidateValidation" /><br />
                    <small>
                        <asp:HyperLink ID="validationLink" runat="server">Search for a regular expression</asp:HyperLink></small>
                </cc1:PropertyPanel>

        <cc1:PropertyPanel ID="PropertyPanel7" runat="server" Text="Description">
                    <asp:TextBox ID="tbDescription" runat="server" CssClass="propertyFormInput" TextMode="MultiLine"></asp:TextBox>
                </cc1:PropertyPanel>
            </cc1:Pane>
        </div>
    </div>
</li>
<script type="text/javascript">    
    function ValidateValidation(sender, args) {
        try {
            var patt = new RegExp(args.Value);
            args.IsValid = true;

        } catch (e) {
            args.IsValid = false;
        }
    }
</script>