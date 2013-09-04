<%@ Page MasterPageFile="../../masterpages/umbracoPage.Master" Language="C#" AutoEventWireup="true" CodeBehind="editDataSource.aspx.cs" Inherits="Umbraco.Forms.UI.Pages.editDataSource" %>
<%@ Register Namespace="umbraco.uicontrols" Assembly="controls" TagPrefix="umb" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

<link href="../../../umbraco_client/GenericProperty/genericproperty.css" type="text/css" rel="stylesheet">


<script type="text/javascript">
<!--

    $(document).ready(function() {
        CheckForms();
    });
    
    function ShowCreateFormFromDataSourceDialog() {

        var src = 'createFormFromDataSourceDialog.aspx?guid=<%= Request["guid"] %>';
        document.location.href = src;
    }

    function CloseCreateFormFromDataSourceDialog(formguid,formname) {
        if (formguid != null) {
            top.UmbSpeechBubble.ShowMessage('save', 'Form created', '');

            var newform = "<div class='propertypane' style='margin-left: 10px; margin-right: 10px;'><div><span class='formname'>" + formname + "<span> <a class='details' href='editForm.aspx?guid=" + formguid + "'>Details</a></div></div>";
            
            $("#formcontainer").append(newform);

            $("#formcontainer .empty").toggle();
        }
        $.modal.close();
    }

    function CheckForms() {

        var noforms = "<div class='empty' style='border: 1px solid rgb(204, 204, 204); margin: 10px; padding: 4px;'>No forms using this datasource. Click on the 'create a new form' link at the top to create a new form.</div>";
        
        if($("#formcontainer").children().size() == 0)
        {
            $("#formcontainer").append(noforms);
        }
    }
   //-->
</script>

</asp:Content>

<asp:Content ID="Content" ContentPlaceHolderID="body" runat="server">
     
    
     <script type="text/javascript" src="../../../umbraco_client/jqueryui.js</script>

     <script type="text/javascript" src="scripts/jquery.simplemodal-1.2.3.js"></script>

      <umb:TabView ID="TabView1" runat="server" Width="552px" Height="692px" />
       
      <asp:ValidationSummary ID="valSum" runat="server" CssClass="error" style="margin-top: 10px;" DisplayMode="BulletList" />
       
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
      
      <umb:Pane ID="paneCreateForm" runat="server" Visible="false">
      

      
        <asp:Button ID="btnCreateFormFrom" runat="server" Text="Create form" OnClientClick="ShowCreateFormFromDataSourceDialog();return false;" />
     
       </umb:Pane>
     
      <umb:Pane ID="paneForms" runat="server" Visible="false">
      
            <h2 class="propertypaneTitel">Create new form</h2>
            
            <ul class="genericPropertyList addNewProperty">
              <li>
             
             
             <div class="propertyForm">
                      <div style="margin: 0px; padding: 0px; display: block;" id="showadd">
                          <h3 style="margin: 0px; padding: 0px;">
                              <a href="javascript:ShowCreateFormFromDataSourceDialog();">
                                 
                                  Click here to create a new form based on this datasource </a>
                          </h3>
                      </div>
             </div>
             </li>
             </ul>
             
            <h2 class="propertypaneTitel">Forms using this workflow</h2>
            
            <div id="formcontainer">
                <asp:Repeater ID="rptForms" runat="server">
                    <ItemTemplate>
                    
                            <div class="propertypane" style="margin-left: 10px; margin-right: 10px;">
                                <div>
                                    <span class="formname"><%# ((Umbraco.Forms.Core.Form)Container.DataItem).Name %></span>
                                    <a class="details" href="editForm.aspx?guid=<%# ((Umbraco.Forms.Core.Form)Container.DataItem).Id %>">Details</a>

                                </div>
                            </div>
                         
                    </ItemTemplate>
                </asp:Repeater>
            </div>
      </umb:Pane>
      

 
 
</asp:Content>