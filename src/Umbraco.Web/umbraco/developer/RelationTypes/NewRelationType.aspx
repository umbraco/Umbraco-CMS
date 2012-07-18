<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="NewRelationType.aspx.cs" Inherits="umbraco.cms.presentation.developer.RelationTypes.NewRelationType" MasterPageFile="/umbraco/masterpages/umbracoPage.Master"%>
<%@ Register TagPrefix="umb" Namespace="umbraco.uicontrols" Assembly="controls" %>


<asp:Content ID="headContent" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
    </style>  
  
    <script type="text/javascript">
    </script>
</asp:Content>

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
        <% ///*<asp:RequiredFieldValidator ID="dualRequiredFieldValidator" runat="server" ControlToValidate="dualRadioButtonList" ValidationGroup="NewRelationType" ErrorMessage="Direction Required" Display="Dynamic" /> */ %>


    </umb:Pane>
    <umb:Pane ID="objectTypePane" runat="server" Text="">

		<umb:PropertyPanel runat="server" id="PropertyPanel2" Text="Parent">
                <asp:DropDownList ID="parentDropDownList" runat="server" />
        </umb:PropertyPanel>

		<umb:PropertyPanel runat="server" id="PropertyPanel3" Text="Child">
                <asp:DropDownList ID="childDropDownList" runat="server" />
        </umb:PropertyPanel>

	</umb:Pane>
            
    <div style="margin-top:15px">
        <asp:Button ID="addButton" runat="server" Text="Create" onclick="AddButton_Click" CausesValidation="true" ValidationGroup="NewRelationType" />
        <em>or</em>
        <a onclick="top.UmbClientMgr.closeModalWindow()" style="color: blue;" href="#">Cancel</a>
    </div>


</asp:Content>

