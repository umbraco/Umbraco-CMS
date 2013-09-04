<%@ Page  MasterPageFile="../../masterpages/umbracoDialog.Master" Language="C#" AutoEventWireup="true" CodeBehind="formPickerCreateFormDialog.aspx.cs" Inherits="Umbraco.Forms.UI.Dialogs.formPickerCreateFormDialog" %>
<%@ Register Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
    Namespace="System.Web.UI" TagPrefix="asp" %>
<%@ Register Namespace="umbraco.uicontrols" Assembly="controls" TagPrefix="umb" %>


<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="server">

    <style type="text/css">
        #header
        {
            background: #FFFFFF url(/umbraco_client/modal/modalGradiant.gif) center bottom repeat-x;
            border-bottom: 1px solid #CCC;
        }
        
        #caption
        {
            font: bold 100% "Lucida Grande" , Arial, sans-serif;
            text-shadow: #FFF 0 1px 0;
            padding: .5em 2em .5em .75em;
            margin: 0;
            text-align: left;
        }
        #close
        {
            display: block;
            position: absolute;
            right: 5px;
            top: 4px;
            padding: 2px 3px;
            font-weight: bold;
            text-decoration: none;
            font-size: 13px;
        }
        #close:hover
        {
            background: transparent;
        }
        #body
        {
            padding: 7px;
        }    
    </style>    
    
    <script type="text/javascript">
<!--
        //Form GUID
        var formguid = "<%= Request["guid"] %>";
        
        // ID's of the action add buttons
        var addpage_id = "ctl00_body_addpage";
        var addfieldset_id = "ctl00_body_addfieldset";
        var addfield_id = "ctl00_body_addfield";
            
        //ID's of the settings controls
        var formname_id = "<%= txtName.ClientID %>";
        var formsubmitmessage_id = "<%= txtMessage.ClientID %>";
        var formsubmitpage_id = "<%= ContentPickerID %>";
        var formshowvalidationsum_id = "<%= cbShowValidationSummary.ClientID %>";
        var formhidefieldvalidation_id = "<%= cbHideFieldValidation.ClientID %>";
        var formrequirederrormessage_id = "<%= txtRequiredErrorMessage.ClientID %>";
        var forminvaliderrormessage_id = "<%= txtInvalidErrorMessage.ClientID %>";
        var formmanualapproval_id = "<%=cbManualApproval.ClientID %>";
        
        var formindicator_id = "<%= txtIndicator.ClientID %>";
        var formmandatoryonly_id = "<%= rbMandatoryOnly.ClientID %>";
        var formoptionalonly_id = "<%= rbOptionalFieldsOnly.ClientID %>";
        var formdisablestylesheet_id = "<%= cbDisableDefaultStylesheet.ClientID %>";
           
        var formguid = '';
        function dialogHandler(id) {
            formguid = id;
            jQuery("#buttons").show();
        }

        function setFormGuid(id)
        {
             formguid = id;
             jQuery("#buttons").show();
        }
        function UpdatePicker() {
            if (formguid != '') {
            
               SaveCurrentDesign();
                
               
            }
        }
        
        function SaveCurrentDesign()
        {
                var design = $("#designsurface").html();
                
                var formname = $("#" + formname_id).val();
                var formmanualapproval = $("#" + formmanualapproval_id).is(':checked'); 
                var formsubmitmessage = $("#" + formsubmitmessage_id).val();
                var formsubmitpage = $("#" + formsubmitpage_id +" .picker input").val();
                var formshowvalsum = $("#" + formshowvalidationsum_id).is(':checked'); 
                var formhidefieldval = $("#" + formhidefieldvalidation_id).is(':checked'); 

                var formrequirederrormessage = $("#" + formrequirederrormessage_id).val();
                var forminvaliderrormessage = $("#" + forminvaliderrormessage_id).val();

                var formfieldindicationtype = 0;

                if ($("#" + formmandatoryonly_id).is(':checked')) {
                    formfieldindicationtype = 1;
                }
                if ($("#" + formoptionalonly_id).is(':checked')) {
                    formfieldindicationtype = 2;
                }

                var formindicator = $("#" + formindicator_id).val();
                var formdisablestylesheet = $("#" + formdisablestylesheet_id).is(':checked');
                
                var formsaverecordlocaly = true;
                
                //Call save method of designer webservice
                UmbracoContour.Webservices.Designer.Save(formguid, formname, formmanualapproval, formsubmitmessage, formsubmitpage, formrequirederrormessage, forminvaliderrormessage, formshowvalsum, formhidefieldval, design, formfieldindicationtype, formindicator, formdisablestylesheet,formsaverecordlocaly, SaveSucces, SaveFailure);
        }
        function SaveSucces(retVal) {

              <% if (Umbraco.Forms.Core.CompatibilityHelper.IsVersion4dot5OrNewer){%>
                    UmbClientMgr.closeModalWindow(formguid);
              <%}else{%>                
                 parent.hidePopWin(true, formguid);
               <%}%>
           
        }
        function SaveFailure(retVal) {
        alert("Failed to save form");
        }
        
        function setDirty() {
        
        }

        function cancelThis()
        {
            <% if (Umbraco.Forms.Core.CompatibilityHelper.IsVersion4dot5OrNewer){%>
            UmbClientMgr.closeModalWindow();
            <%}else{%>        
            parent.hidePopWin(false,0);
            <%}%>
        }
//-->
</script>

</asp:Content>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="body">



   
    
    <div>
    
        <umb:Pane ID="createpane" runat="server" Text="Create">
            <umb:PropertyPanel ID="PropertyPanel3" runat="server" Text="Name">
                <asp:TextBox ID="txtCreate" runat="server" Width="250"></asp:TextBox><asp:RequiredFieldValidator ID="RequiredFieldValidator1" ControlToValidate="txtCreate" ErrorMessage="*" runat="server"  ValidationGroup="create"/>
            </umb:PropertyPanel>
            
            <umb:PropertyPanel ID="PropertyPanel4" runat="server" Text="Choose a template (optional)">
                <asp:ListBox ID="formTemplate" runat="server" Width="250" Rows="1" SelectionMode="Single">
                    <asp:ListItem Value="">None</asp:ListItem>
                </asp:ListBox>
            </umb:PropertyPanel>
            
            <umb:PropertyPanel ID="PropertyPanel5" Text=" " runat="server">
                
                <p>
                    <asp:Button ID="btnCreate" runat="server" Text="Create" OnClick="btnCreate_Click" ValidationGroup="create"/>
                     <em> or </em>
                     <a href="#" style="color: blue" onclick="cancelThis();" id="cancelbutton">cancel</a>
                </p>
                
            </umb:PropertyPanel>
            
        </umb:Pane>
        

        
        <umb:TabView AutoResize="false" Width="650px" Height="445px" runat="server"  ID="form_options" Visible="false"/>
         
        <umb:Pane ID="phdesigner" runat="server" />
        
        <umb:Pane ID="MainSettingsPane" runat="server" >
        
        <umb:PropertyPanel ID="ppName" runat="server" Text="Name">
                <asp:TextBox ID="txtName" runat="server" CssClass="guiInputText guiInputStandardSize"></asp:TextBox>
        </umb:PropertyPanel>
     
        <umb:PropertyPanel ID="ppManualApproval" runat="server" Text="Manual Approval">
            <asp:CheckBox ID="cbManualApproval" runat="server" />
        </umb:PropertyPanel>
        
     </umb:Pane>

    <umb:Pane ID="SubmitSettingsPane" Text="Submitting the form" runat="server" >
        <umb:PropertyPanel ID="ppMessage" runat="server" Text="Message on submit">
                <asp:TextBox ID="txtMessage" TextMode="MultiLine" runat="server" CssClass="guiInputText guiInputStandardSize"></asp:TextBox>
        </umb:PropertyPanel>
        <umb:PropertyPanel ID="ppPage" runat="server" Text="Send to page">
            <asp:PlaceHolder ID="phContentPicker" runat="server"></asp:PlaceHolder>
         </umb:PropertyPanel>
     </umb:Pane>
    
    <umb:Pane ID="MandatoryIndicationSettingsPage" Text="Field Indicators" runat="server" >
             <umb:PropertyPanel ID="ppMarkFields" runat="server" Text="Mark fields"> 
                 <asp:RadioButton ID="rbMandatoryOnly" runat="server" text="Mark mandatory fields only " GroupName="indicator" /> <br />
                 <asp:RadioButton ID="rbOptionalFieldsOnly" runat="server" text="Mark optional fields only" GroupName="indicator"/> <br />
                 <asp:RadioButton ID="rbNoIndicator" runat="server" text="No indicator" GroupName="indicator"/>
             </umb:PropertyPanel>
             <umb:PropertyPanel ID="ppIndicator" runat="server" Text="Indicator"> 
                 <asp:TextBox ID="txtIndicator" runat="server" CssClass="guiInputText guiInputStandardSize"></asp:TextBox>
            </umb:PropertyPanel>
    </umb:Pane>
    
    <umb:Pane ID="ValidationSettingsPane" Text="Validation" runat="server" >
             <umb:PropertyPanel ID="ppRequiredErrorMessage" runat="server" Text="Required error message">
                    <asp:TextBox ID="txtRequiredErrorMessage" runat="server" CssClass="guiInputText guiInputStandardSize"></asp:TextBox>
             </umb:PropertyPanel>
             
             <umb:PropertyPanel ID="ppInvalidErrorMessage" runat="server" Text="Invalid error message">
                    <asp:TextBox ID="txtInvalidErrorMessage" runat="server" CssClass="guiInputText guiInputStandardSize"></asp:TextBox>
             </umb:PropertyPanel>
             
            <umb:PropertyPanel ID="ppShowValidationSummary" runat="server" Text="Show validation summary">
                <asp:CheckBox ID="cbShowValidationSummary" runat="server" />
            </umb:PropertyPanel>
            <umb:PropertyPanel ID="ppHideFieldValidation" runat="server" Text="Hide field validation labels">
                 <asp:CheckBox ID="cbHideFieldValidation" runat="server" />
            </umb:PropertyPanel>
     </umb:Pane>
     
      <umb:Pane ID="StylingSettingsPane" Text="Styling" runat="server" >
        <umb:PropertyPanel ID="ppDisableDefaultStylesheet" runat="server" Text="Disable default stylesheet">
                <asp:CheckBox ID="cbDisableDefaultStylesheet" runat="server" />
        </umb:PropertyPanel>
      </umb:Pane>
      
      <umb:Pane id="RecordStoragePane" Text="Record Storage" Visible="false" runat="server">
        <umb:PropertyPanel ID="ppStoreRecordslocally" runat="server" Text="Store records locally">
                <asp:CheckBox ID="cbStoreRecordslocally" runat="server" />
                     <small>If unchecked, this will disable entries viewing and easy access to the reacords through the xslt libraries. <br /> You should uncheck this if 
                        you wish to avoid having redundant records stored.
                    </small>
        </umb:PropertyPanel>
      </umb:Pane>

        
        <p style="clear:both;padding-top:20px; display:none;" id="buttons">
            <input type="submit" value="save and insert" style="width: 100px;" id="submitbutton" onclick="UpdatePicker();return false;"/>
            <em> or </em>
            <a href="#" style="color: blue" onclick="cancelThis();" id="A1">cancel</a> 
        </p> 
        
       </div>

          
           <asp:PlaceHolder ID="phModals" runat="server"></asp:PlaceHolder>

           </asp:Content>