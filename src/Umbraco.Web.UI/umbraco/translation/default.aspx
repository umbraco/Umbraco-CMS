<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="../masterpages/umbracoPage.Master" CodeBehind="default.aspx.cs" Inherits="umbraco.presentation.translation._default" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<asp:Content ContentPlaceHolderID="head" runat="server">
  <style type="text/css">
  .fieldsTable tr{
    border-color: #D9D7D7 !Important;
  }  
</style>
</asp:Content>

<asp:Content ContentPlaceHolderID="body" runat="server">
  <cc1:UmbracoPanel ID="Panel2" runat="server">
    
    <cc1:Feedback ID="feedback" runat="server" />
    
    <cc1:Pane ID="pane_uploadFile" runat="server" Text="Upload translation file">
      <p>
        When you have completed the translation. Upload the edited XML file here. The related translation tasks will automatically be closed when a file is uploaded.
      </p>
      
      <cc1:PropertyPanel runat="server">
                <input type="file" runat="server" id="translationFile" size="30" /> &nbsp; <asp:Button ID="uploadFile" runat="server" Text="Upload file" OnClick="uploadFile_Click" />
      </cc1:PropertyPanel>
    </cc1:Pane>
    
   
   <cc1:Pane ID="pane_tasks" runat="server" Text="Your tasks">
       <p>
        <asp:Literal ID="lt_tasksHelp" runat="server"></asp:Literal>
       </p>
        
        <p>
            <a href="xml.aspx?task=all" target="_blank"><%= umbraco.ui.Text("translation", "downloadAllAsXml") %></a>
             &nbsp; &nbsp;
            <a href="translationTasks.dtd" target="_blank"><%= umbraco.ui.Text("translation", "DownloadXmlDTD")%></a>        
        </p>              
      <asp:GridView GridLines="Horizontal" ID="taskList" runat="server" CssClass="fieldsTable" BorderStyle="None" Width="100%"
            CellPadding="5" AutoGenerateColumns="false">
            <Columns>
              <asp:TemplateField>
                <ItemTemplate>
                 <a href="details.aspx?id=<%#Eval("Id") %>"><%#Eval("NodeName") %></a>
                </ItemTemplate>
              </asp:TemplateField>
              <asp:BoundField DataField="ReferingUser" />
              <asp:BoundField DataField="Date" />
              <asp:HyperLinkField DataNavigateUrlFields="Id" DataNavigateUrlFormatString="details.aspx?id={0}" Text="Details" />
              <asp:HyperLinkField DataNavigateUrlFields="Id" DataNavigateUrlFormatString="xml.aspx?id={0}" Target="_blank" Text="Download Xml" />
            </Columns>
          </asp:GridView>
   </cc1:Pane>
    
  </cc1:UmbracoPanel>
</asp:Content>
