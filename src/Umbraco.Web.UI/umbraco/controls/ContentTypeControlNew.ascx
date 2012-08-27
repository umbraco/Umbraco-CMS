<%@ Control Language="c#" AutoEventWireup="True" Codebehind="ContentTypeControlNew.ascx.cs"
  Inherits="umbraco.controls.ContentTypeControlNew" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="cc2" Namespace="umbraco.uicontrols" Assembly="controls" %>

<cc1:TabView ID="TabView1" Height="392px" Width="552px" runat="server"></cc1:TabView>

<asp:Panel ID="pnlGeneral" runat="server"></asp:Panel>

<asp:Panel ID="pnlTab" Style="text-align: left" runat="server">

  <cc2:Pane ID="PaneTabsInherited" runat="server" Visible="false">
  <p><strong>Master Content Type enabled</strong><br />This Content Type uses <em><asp:Literal ID="tabsMasterContentTypeName" runat="server"></asp:Literal></em> as a Master Content Type. Tabs from Master Content Types are not shown and can only be edited on the Master Content Type itself</p>
  </cc2:Pane>
  
  <cc2:Pane ID="Pane2" runat="server">
    <cc2:PropertyPanel runat="server" id="pp_newTab" Text="New tab">
      <asp:TextBox ID="txtNewTab" runat="server"/> &nbsp; <asp:Button ID="btnNewTab" runat="server" Text="New tab" OnClick="btnNewTab_Click"/>
    </cc2:PropertyPanel>
  </cc2:Pane>
    
  <cc2:Pane ID="Pane1" runat="server" Width="216" Height="80">
    <asp:DataGrid ID="dgTabs" Width="100%" runat="server" CellPadding="2" HeaderStyle-CssClass="propertyHeader"
      ItemStyle-CssClass="propertyContent" GridLines="None" OnItemCommand="dgTabs_ItemCommand"
      HeaderStyle-Font-Bold="True" AutoGenerateColumns="False">
      <Columns>
        <asp:BoundColumn DataField="id" Visible="False"></asp:BoundColumn>
        <asp:TemplateColumn HeaderText="Name">
          <ItemTemplate>
            <asp:TextBox ID="txtTab" runat="server" Value='<%#DataBinder.Eval(Container.DataItem,"name")%>'>
            </asp:TextBox>
          </ItemTemplate>
        </asp:TemplateColumn>
        <asp:TemplateColumn HeaderText="Sort order">
          <ItemTemplate>
            <asp:TextBox ID="txtSortOrder" runat="server" Value='<%#DataBinder.Eval(Container.DataItem,"order") %>'>
            </asp:TextBox>
          </ItemTemplate>
        </asp:TemplateColumn>
        <asp:ButtonColumn ButtonType="PushButton" Text="Delete" CommandName="Delete"></asp:ButtonColumn>
      </Columns>
    </asp:DataGrid>
    <p style="text-align: center;">
      <asp:Literal ID="lttNoTabs" runat="server"></asp:Literal></p>
  </cc2:Pane>
</asp:Panel>

<asp:Panel ID="pnlInfo" runat="server">

  <cc2:Pane ID="Pane3" runat="server">
    <cc2:PropertyPanel ID="pp_name" runat="server" Text="Name">
        <asp:TextBox ID="txtName" CssClass="guiInputText guiInputStandardSize" runat="server"></asp:TextBox>
        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" ControlToValidate="txtName" runat="server" ErrorMessage="Name cannot be empty!"></asp:RequiredFieldValidator>
    </cc2:PropertyPanel>
    
    <cc2:PropertyPanel ID="pp_alias" runat="server" Text="Alias">
         <asp:TextBox ID="txtAlias" CssClass="guiInputText guiInputStandardSize" runat="server"></asp:TextBox>
         <asp:RequiredFieldValidator ControlToValidate="txtAlias" runat="server" ErrorMessage="Alias cannot be empty!"></asp:RequiredFieldValidator>
    </cc2:PropertyPanel>
  </cc2:Pane>
  
  <cc2:Pane runat="server">  
    <cc2:PropertyPanel ID="pp_icon" runat="server" Text="Icon">
        <div class="umbIconDropdownList">
          <asp:DropDownList ID="ddlIcons"  CssClass="guiInputText guiInputStandardSize" runat="server"/>
        </div>
    </cc2:PropertyPanel>
    <cc2:PropertyPanel ID="pp_thumbnail" runat="server" Text="Thumbnail">
        <div class="umbThumbnailDropdownList">
          <asp:DropDownList ID="ddlThumbnails" CssClass="guiInputText guiInputStandardSize" runat="server"/>
        </div>
    </cc2:PropertyPanel>
    <cc2:PropertyPanel ID="pp_description" runat="server" Text="Description">
          <asp:TextBox ID="description" runat="server" CssClass="guiInputText guiInputStandardSize" TextMode="MultiLine" Rows="3"/>
    </cc2:PropertyPanel>
  </cc2:Pane>
</asp:Panel>

<asp:Panel ID="pnlStructure" runat="server">
  <cc2:Pane ID="Pane5" runat="server">
    <cc2:PropertyPanel ID="pp_allowedChildren" runat="server" Text="Allowed Child nodetypes">
       <asp:CheckBoxList ID="lstAllowedContentTypes" runat="server" EnableViewState="True"/>
       <asp:PlaceHolder ID="PlaceHolderAllowedContentTypes" runat="server"/>
    </cc2:PropertyPanel>
  </cc2:Pane>
</asp:Panel>

<asp:Panel ID="pnlProperties" runat="server">
  <cc2:Pane ID="PanePropertiesInherited" runat="server" Visible="false">
  <p><strong>Master Content Type enabled</strong><br />This Content Type uses <em><asp:Literal ID="propertiesMasterContentTypeName" runat="server"></asp:Literal></em> as a Master Content Type. Properties from Master Content Types are not shown and can only be edited on the Master Content Type itself</p>
  </cc2:Pane>
  
  <cc2:Pane ID="Pane4" runat="server" Width="216" Height="80">
      <div class="genericPropertyForm">
            <asp:PlaceHolder ID="PropertyTypeNew" runat="server"></asp:PlaceHolder>
            <asp:PlaceHolder ID="PropertyTypes" runat="server"></asp:PlaceHolder>
      </div>
    </cc2:Pane>
</asp:Panel>
<script type="text/javascript">
    $(function () {
        var mailControlId = '<asp:Literal id="theClientId" runat="server"/>';
        duplicatePropertyNameAsSafeAlias(mailControlId + '_GenericPropertyNew_control_tbName', mailControlId + '_GenericPropertyNew_control_tbAlias');
        checkAlias(mailControlId + '_GenericPropertyNew_control_txtAlias');
    });
</script>