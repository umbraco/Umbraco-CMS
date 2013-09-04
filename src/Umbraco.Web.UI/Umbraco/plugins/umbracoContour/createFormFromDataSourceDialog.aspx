<%@ Page MasterPageFile="../../masterpages/umbracoPage.Master" Language="C#" AutoEventWireup="true" CodeBehind="createFormFromDataSourceDialog.aspx.cs" Inherits="Umbraco.Forms.UI.Dialogs.createFormFromDataSourceDialog" %>

<%@ Register Namespace="umbraco.uicontrols" Assembly="controls" TagPrefix="umb" %>


<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
<link rel="stylesheet" href="css/dialogs.css" type="text/css" media="screen" />
<style>

    #dialogcontainer
    {
        width: 590px;
    	height:390px;
    }
</style>
</asp:Content>


<asp:Content ID="content2" ContentPlaceHolderID="body" runat="server">
    

<umb:UmbracoPanel Text="Create form from datasource" runat="server" hasMenu="false" >



    <div>
    
    <!-- Step 1, select fields -->
    <asp:Panel ID="pnlStep1" runat="server" >
    
         <h1>Select fields you want to include</h1>
        <div class="propertypane">
        
            <div class="propertyItem" style="">
                 <div class="propertyItemheader">
                 Name
                 <br />
                 <small>name of the form that will be created</small>
                 </div>
                  <div class="propertyItemContent">
                      <asp:TextBox ID="txtName" runat="server" CssClass="propertyFormInput"></asp:TextBox>
                  </div>
            </div>
            <div class="propertyItem" style="">
                <div class="propertyItemheader">
                Fields
                <br/>
                <small>select the field you want to include</small>
                </div> 
                
                <div class="propertyItemContent">
                
                    <asp:CheckBoxList ID="cblFields" runat="server" RepeatLayout="Flow">
                    </asp:CheckBoxList>
                    
                </div>
            </div>
            
            <div class="propertyPaneFooter">-</div>

        </div>
        
         <div class="dialogcontrols">
            <asp:Button ID="Button1" runat="server" Text="Continue" onclick="Button1_Click" />
            <em> or </em>
            <asp:LinkButton ID="LinkButton2" runat="server" OnClick="cancelClick">Cancel</asp:LinkButton>
        </div>
    
      </asp:Panel>
      
     <!-- Step 2, setup prevalues -->
     <asp:Panel ID="pnlStep2" runat="server" Visible="false">
     
        <h1>Setup foreign keys</h1>
        
         <div class="propertypane">
         
         <asp:PlaceHolder ID="phForeigKeys" runat="server">
          
          </asp:PlaceHolder>
          
          <div class="propertyPaneFooter">-</div>

        </div>
         <div class="dialogcontrols">
            <asp:Button ID="Button3" runat="server" Text="Previous" 
                 onclick="Button3_Click" />
            <asp:Button ID="Button2" runat="server" Text="Next" onclick="Button2_Click" />
            <em> or </em>
            <asp:LinkButton runat="server" OnClick="cancelClick">Cancel</asp:LinkButton>
            
        </div>
    </asp:Panel>
    
    <!-- Step 3, confirm -->
    
     <asp:Panel ID="pnlFinal" runat="server" Visible="false">
     
      <h1>Set fieldtypes</h1>
      
      <div class="propertypane">
      
          <asp:UpdatePanel ID="UpdatePanel1" runat="server">
          <ContentTemplate>
              <asp:PlaceHolder ID="phTypeSelect" runat="server">
              
              </asp:PlaceHolder>
          </ContentTemplate>
          </asp:UpdatePanel>
          
          <div class="propertyPaneFooter">-</div>

        </div>
     
      <div class="dialogcontrols">
            <asp:Button ID="Button4" runat="server" Text="Previous" 
                onclick="Button4_Click" />
            <asp:Button ID="btnCreate" runat="server" Text="Save form" 
                onclick="btnCreate_Click" />
                
            <em> or </em>
            
            <asp:LinkButton ID="LinkButton1" runat="server" OnClick="cancelClick">Cancel</asp:LinkButton>
        </div>
      </asp:Panel>
   
    </div>
</umb:UmbracoPanel>

</asp:Content>