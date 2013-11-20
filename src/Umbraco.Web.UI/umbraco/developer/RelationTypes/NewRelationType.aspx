<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="NewRelationType.aspx.cs" Inherits="umbraco.cms.presentation.developer.RelationTypes.NewRelationType" MasterPageFile="../../masterpages/umbracoDialog.Master"%>
<%@ Register TagPrefix="umb" Namespace="umbraco.uicontrols" Assembly="controls" %>

<asp:Content ID="bodyContent" ContentPlaceHolderID="body" runat="server">
    <umb:Pane ID="nameAliasPane" runat="server" Text="">
        	
		<umb:PropertyPanel runat="server" ID="nameProperyPanel" Text="Name">			
                <asp:TextBox ID="descriptionTextBox" runat="server" Columns="40" AutoCompleteType="Disabled" style="width:200px;" />
                <asp:RequiredFieldValidator ID="descriptionRequiredFieldValidator" runat="server" ControlToValidate="descriptionTextBox" ValidationGroup="NewRelationType" ErrorMessage="Name Required" Display="Dynamic" />
		</umb:PropertyPanel>
        			
		<umb:PropertyPanel runat="server" id="aliasPropertyPanel" Text="Alias">
                <asp:TextBox ID="aliasTextBox" runat="server" Columns="40" AutoCompleteType="Disabled" style="width:200px;" />
                <asp:RequiredFieldValidator ID="aliasRequiredFieldValidator" runat="server" ControlToValidate="aliasTextBox" ValidationGroup="NewRelationType" ErrorMessage="Alias Required" Display="Dynamic" />
                <asp:CustomValidator ID="aliasCustomValidator" runat="server" ControlToValidate="aliasTextBox" ValidationGroup="NewRelationType" onservervalidate="AliasCustomValidator_ServerValidate" ErrorMessage="Duplicate Alias" Display="Dynamic" />
		</umb:PropertyPanel>

    </umb:Pane>

    <umb:Pane ID="directionPane" runat="server" Text="">

		<umb:PropertyPanel runat="server" id="PropertyPanel1" Text="Direction">
                <asp:RadioButtonList ID="dualRadioButtonList" runat="server" RepeatDirection="Horizontal">
                    <asp:ListItem Enabled="true" Selected="True" Text="Parent to Child" Value="0"/> 
                    <asp:ListItem Enabled="true" Selected="False" Text="Bidirectional" Value="1"/>
                </asp:RadioButtonList>
		</umb:PropertyPanel>
    </umb:Pane>
    
    <umb:Pane ID="objectTypePane" runat="server" Text="">

		<umb:PropertyPanel runat="server" id="PropertyPanel2" Text="Parent">
                <asp:DropDownList ID="parentDropDownList" runat="server" />
        </umb:PropertyPanel>

		<umb:PropertyPanel runat="server" id="PropertyPanel3" Text="Child">
                <asp:DropDownList ID="childDropDownList" runat="server" />
        </umb:PropertyPanel>

	</umb:Pane>
            
    
    
<umb:Pane runat="server" CssClass="btn-toolbar umb-btn-toolbar">
     <a href="#" class="btn btn-link" onclick="UmbClientMgr.closeModalWindow()"><%=umbraco.ui.Text("cancel")%></a>
     <asp:Button ID="addButton" runat="server" Text="Create" cssclass="btn btn-primary" onclick="AddButton_Click" CausesValidation="true" ValidationGroup="NewRelationType" />
</umb:Pane>


</asp:Content>