<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EditRelationType.aspx.cs" Inherits="umbraco.cms.presentation.developer.RelationTypes.EditRelationType" MasterPageFile="../../masterpages/umbracoPage.Master" %>
<%@ Register TagPrefix="umb" Namespace="umbraco.uicontrols" Assembly="controls" %>

<asp:Content ID="headContent" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        table.relations  { }
        table.relations th { width:auto; }
                
        table.relations th.objectTypeIcon { width:20px; }
        table.relations th.directionIcon { width:16px; height:16px; }
        
        table.relations td { background: transparent none no-repeat scroll center center }
        
        /* objectType icons */
        table.relations td.ContentItemType {}
        table.relations td.ROOT {}
        table.relations td.Document {}
        table.relations td.Media {}
        table.relations td.MemberType {}
        table.relations td.Template {}
        table.relations td.MemberGroup {}
        table.relations td.ContentItem {}
        table.relations td.MediaType {}
        table.relations td.DocumentType {}
        table.relations td.RecycleBin {}
        table.relations td.Stylesheet {}
        table.relations td.Member {}
        table.relations td.DataType {}
                
        /* direction icons */
        table.relations td.parentToChild { background-image: url('../../developer/RelationTypes/Images/ParentToChild.png'); }
        table.relations td.bidirectional { background-image: url('../../developer/RelationTypes/Images/Bidirectional.png'); }
    </style>  
  
    <script type="text/javascript">
    </script>
</asp:Content>

<asp:Content ID="bodyContent" ContentPlaceHolderID="body" runat="server">

    <umb:TabView runat="server" ID="tabControl" Width="200" />
       
    <umb:Pane ID="idPane" runat="server" Text="">

        <umb:PropertyPanel runat="server" id="idPropertyPanel" Text="Id">
            <asp:Literal ID="idLiteral" runat="server" />
        </umb:PropertyPanel>

    </umb:Pane>
       
       
    <umb:Pane ID="nameAliasPane" runat="server" Text="">
	
		<umb:PropertyPanel runat="server" ID="nameProperyPanel" Text="Name">			
			<asp:TextBox ID="nameTextBox" runat="server" Columns="40" ></asp:TextBox>
            <asp:RequiredFieldValidator ID="nameRequiredFieldValidator" runat="server" ControlToValidate="nameTextBox" ValidationGroup="RelationType" ErrorMessage="Name Required" Display="Dynamic" />
			
		</umb:PropertyPanel>
		
		<umb:PropertyPanel runat="server" id="aliasPropertyPanel" Text="Alias">
			<asp:TextBox ID="aliasTextBox" runat="server"  Columns="40"></asp:TextBox>				
            <asp:RequiredFieldValidator ID="aliasRequiredFieldValidator" runat="server" ControlToValidate="aliasTextBox" ValidationGroup="RelationType" ErrorMessage="Alias Required" Display="Dynamic" />
            <asp:CustomValidator ID="aliasCustomValidator" runat="server" ControlToValidate="aliasTextBox" ValidationGroup="RelationType" onservervalidate="AliasCustomValidator_ServerValidate" ErrorMessage="Duplicate Alias" Display="Dynamic" />

		</umb:PropertyPanel>

	</umb:Pane>

	
    <umb:Pane ID="directionPane" runat="server" Text="">

        <umb:PropertyPanel runat="server" id="dualPropertyPanel" Text="Direction">
                <asp:RadioButtonList ID="dualRadioButtonList" runat="server" RepeatDirection="Vertical">
                    <asp:ListItem Enabled="true" Selected="False" Text="Parent to Child" Value="0" /> 
                    <asp:ListItem Enabled="true" Selected="False" Text="Bidirectional" Value="1"/>
                </asp:RadioButtonList>
        </umb:PropertyPanel>

    </umb:Pane>


    <umb:Pane ID="objectTypePane" runat="server" Text="">

        <umb:PropertyPanel runat="server" id="parentPropertyPanel" Text="Parent">
            <asp:Literal ID="parentLiteral" runat="server" />
        </umb:PropertyPanel>

        <umb:PropertyPanel runat="server" id="childPropertyPanel" Text="Child">
            <asp:Literal ID="childLiteral" runat="server" />
        </umb:PropertyPanel>

    </umb:Pane>

    
    <umb:Pane ID="relationsCountPane" runat="server" Text="">

        <umb:PropertyPanel runat="server" id="relationsCountPropertyPanel" Text="Count">
            <asp:Literal ID="relationsCountLiteral" runat="server" />
        </umb:PropertyPanel>

    </umb:Pane>
	

    <umb:Pane ID="relationsPane" runat="server" Text="">

        <umb:PropertyPanel runat="server" id="relationsPropertyPanel" Text="Relations">
            
            <asp:Repeater ID="relationsRepeater" runat="server">
                <HeaderTemplate>
                    <table class="table relations">
                        <thead>
                            <tr>
                                <th class="objectTypeIcon">&nbsp;</th>
                                <th>Parent</th>
                                <th class="directionIcon">&nbsp;</th>
                                <th class="objectTypeIcon">&nbsp;</th>
                                <th>Child</th>
                                <th>Created</th>
                                <th>Comment</th>
                            </tr>                        
                        </thead>
                        <tbody>
                </HeaderTemplate>
                <ItemTemplate>
                            <tr>
                                <td class="<%= this.ParentObjectType %>">&nbsp;</td>
                                <td><%# DataBinder.Eval(Container.DataItem, "ParentText") %></td>
                                <td class="<%= this.RelationTypeDirection %>">&nbsp;</td>
                                <td class="<%= this.ChildObjectType %>">&nbsp;</td>
                                <td><%# DataBinder.Eval(Container.DataItem, "ChildText") %></td>
                                <td><%# DataBinder.Eval(Container.DataItem, "DateTime") %></td>
                                <td><%# DataBinder.Eval(Container.DataItem, "Comment") %></td>                
                            </tr>
                </ItemTemplate>            
                <FooterTemplate>
                        </tbody>
                    </table>
                </FooterTemplate>
            </asp:Repeater>
            

        </umb:PropertyPanel>

    </umb:Pane>


</asp:Content>
