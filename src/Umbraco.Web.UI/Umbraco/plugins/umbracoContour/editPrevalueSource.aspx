<%@ Page MasterPageFile="../../masterpages/umbracoPage.Master" Language="C#" AutoEventWireup="true" CodeBehind="editPrevalueSource.aspx.cs" Inherits="Umbraco.Forms.UI.Pages.editPrevalues" %>
<%@ Register Namespace="umbraco.uicontrols" Assembly="controls" TagPrefix="umb" %>


<asp:Content ID="Content" ContentPlaceHolderID="body" runat="server">

    <umb:UmbracoPanel ID="Panel1" runat="server" hasMenu="true" Text="Prevalue Source">
           
          <asp:ValidationSummary ID="valsum" style="margin:10px 0px 10px 0px;" CssClass="error" runat="server" DisplayMode="BulletList" />
          
          <umb:Pane ID="paneMainSettings" runat="server">
            
            <umb:PropertyPanel ID="ppName" runat="server" Text="Name">
                    <asp:TextBox ID="txtName" runat="server" CssClass="guiInputText guiInputStandardSize"></asp:TextBox>
           </umb:PropertyPanel>
                
           <umb:PropertyPanel ID="ppType" runat="server" Text="Type">
                    <asp:DropDownList ID="ddType" runat="server" AutoPostBack="true" CssClass="guiInputText guiInputStandardSize"></asp:DropDownList>
           </umb:PropertyPanel>
                
          </umb:Pane>
          
          <umb:Pane ID="paneDynamicSettings" runat="server" Visible="false">
                
          </umb:Pane>

           <umb:Pane ID="panePrevalues" runat="server" Visible="false">
                <style type="text/css">
                    #prevalues p, #prevalues #header
                    {
                        color:#666666;
                    }
                    
                     #prevalues #header
                     {
                        height:20px;
                     }
                    #prevalues .id
                    {
                        float:left;
                        min-width:100px;
                    }
                    #prevalues .value
                    {
                        float:left;
                        margin-left:5px;
                    }
                    .prevalue
                    {
                        clear:both;
                        height:15px;
                    }
                </style>
               <asp:Repeater ID="rptPrevalues" runat="server">
                    <HeaderTemplate>
                    <div id="prevalues">
                        <p>Prevalues overview:</p>
                        <div id="header">
                            <div class="id">
                                 ID
                            </div>
                            <div class="value">
                                Value
                            </div>
                        </div>
                    </HeaderTemplate>
                   <ItemTemplate>
                        <div class="prevalue">
                            <div class="id">
                            <%# ((Umbraco.Forms.Core.PreValue)Container.DataItem).Id %>
                            </div>
                            <div class="value">
                            <%# ((Umbraco.Forms.Core.PreValue)Container.DataItem).Value%>
                            </div>
                        </div>
                   </ItemTemplate>
                   <FooterTemplate>
                    </div>
                   </FooterTemplate>

               </asp:Repeater>

                
          </umb:Pane>
          
     </umb:UmbracoPanel>
 
</asp:Content>
