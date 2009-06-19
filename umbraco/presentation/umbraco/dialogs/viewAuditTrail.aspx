<%@ Page Language="c#" MasterPageFile="../masterpages/umbracoPage.Master"Codebehind="viewAuditTrail.aspx.cs" AutoEventWireup="True"
  Inherits="umbraco.presentation.umbraco.dialogs.viewAuditTrail" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
<style type="text/css">
.gridHeader{border-bottom:2px solid #D9D7D7;}
.gridItem{border-color: #D9D7D7;}
</style>

<link rel="Stylesheet" href="../css/treeIcons.css"></link>
</asp:Content>

<asp:Content ContentPlaceHolderID="body" runat="server">
<cc1:Pane runat="server">
 <asp:DataGrid ID="auditLog" runat="server" BorderStyle="None" HeaderStyle-CssClass="gridHeader" ItemStyle-CssClass="gridItem" GridLines="Horizontal" HeaderStyle-Font-Bold="True" AutoGenerateColumns="False"
              width="100%">
              <Columns>
                <asp:TemplateColumn>
                  <HeaderTemplate>
                    <b>
                      <%=umbraco.ui.Text("action")%>
                    </b>
                  </HeaderTemplate>
                  <ItemTemplate>
                    <%# FormatAction(DataBinder.Eval(Container.DataItem, "Action", "{0}")) %>
                  </ItemTemplate>
                </asp:TemplateColumn>
                <asp:TemplateColumn>
                  <HeaderTemplate>
                    <b>
                      <%=umbraco.ui.Text("user")%>
                    </b>
                  </HeaderTemplate>
                  <ItemTemplate>
                    <%# DataBinder.Eval(Container.DataItem, "User", "{0}") %>
                  </ItemTemplate>
                </asp:TemplateColumn>
                <asp:TemplateColumn>
                  <HeaderTemplate>
                    <b>
                      <%=umbraco.ui.Text("date")%>
                    </b>
                  </HeaderTemplate>
                  <ItemTemplate>
                    <%# DataBinder.Eval(Container.DataItem, "Date", "{0:D} {0:T}") %>
                  </ItemTemplate>
                </asp:TemplateColumn>
                <asp:TemplateColumn>
                  <HeaderTemplate>
                    <b>
                      <%=umbraco.ui.Text("comment")%>
                    </b>
                  </HeaderTemplate>
                  <ItemTemplate>
                    <%# DataBinder.Eval(Container.DataItem, "Comment", "{0}") %>
                  </ItemTemplate>
                </asp:TemplateColumn>
              </Columns>
            </asp:DataGrid>
</cc1:Pane>

</asp:Content>


           
            
            
