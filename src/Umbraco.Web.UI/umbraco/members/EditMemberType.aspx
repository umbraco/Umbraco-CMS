<%@ Page Async="true" language="c#" MasterPageFile="../masterpages/umbracoPage.Master" Codebehind="EditMemberType.aspx.cs" AutoEventWireup="True" Inherits="umbraco.cms.presentation.members.EditMemberType" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="uc1" TagName="ContentTypeControlNew" Src="../controls/ContentTypeControlNew.ascx" %>
<%@ Register Namespace="umbraco" TagPrefix="umb" Assembly="umbraco" %>

<asp:Content ContentPlaceHolderID="head" runat="server">
<style type="text/css">
.gridHeader{border-bottom:2px solid #D9D7D7;}
.gridItem{border-color: #D9D7D7;}
</style>
</asp:Content>
<asp:Content ContentPlaceHolderID="body" runat="server">
			<uc1:ContentTypeControlNew id="ContentTypeControlNew1" HideStructure="true" runat="server"></uc1:ContentTypeControlNew>
			<cc1:Pane id="Pane1andmore" runat="server">
			
			<asp:DataGrid id="dgEditExtras" runat="server" ItemStyle-BorderStyle="NotSet" HeaderStyle-BorderStyle="NotSet" BorderStyle="NotSet" AutoGenerateColumns="False" CssClass="table"  OnItemDataBound="dgEditExtras_itemdatabound">
				<Columns>
					<asp:BoundColumn DataField="id" HeaderText="" Visible="False"></asp:BoundColumn>
					<asp:BoundColumn DataField="name" HeaderText="Property name"></asp:BoundColumn>

					<asp:TemplateColumn HeaderText="Member can edit">
						<ItemTemplate>
							<asp:CheckBox ID="ckbMemberCanEdit" Runat="server"></asp:CheckBox>
						</ItemTemplate>
					</asp:TemplateColumn>

					<asp:TemplateColumn HeaderText="Show on profile">
						<ItemTemplate>
							<asp:CheckBox ID="ckbMemberCanView" Runat="server"></asp:CheckBox>
						</ItemTemplate>
					</asp:TemplateColumn>

				</Columns>
			</asp:DataGrid>
			
			</cc1:Pane>
</asp:Content>
