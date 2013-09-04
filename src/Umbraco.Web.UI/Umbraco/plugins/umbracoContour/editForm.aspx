<%@ Page MasterPageFile="../../masterpages/umbracoPage.Master" Language="C#" AutoEventWireup="true" CodeBehind="editForm.aspx.cs" Inherits="Umbraco.Forms.UI.Pages.editForm" %>
<%@ Register Namespace="umbraco.uicontrols" Assembly="controls" TagPrefix="umb" %>


<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">


<script type="text/javascript">
<!--
    //Form GUID
    var formguid = '<%= Request["guid"] %>';
    
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
    
    var formstorerecordslocally_id = "";
    
    // Copy used in umbracoforms.editformpage.eventhandlers.js
    var lang_formsaveok = "Form saved";
    var lang_formsavefailed = "Failed to save form";


    $(document).ready(function() {
        $("#" + addpage_id).hide();
        $("#" + addfieldset_id).hide();
        $("#" + addfield_id).hide();
    });
    
    
    function ShowPreviewFormDialog() {

    var src = 'previewFormDialogMvc.aspx?guid=<%= Request["guid"] %>&rnd=' + Math.floor(Math.random()*11);

    window.open(src);

    //$.modal('<iframe src="' + src + '" height="600" width="600" style="border:0">');

    }
    
    function ShowFormWorkflows(){
        var src = 'editFormWorkflows.aspx?guid=<%= Request["guid"] %>';

        window.location = src;
    }

    var isDirty = false;
    function setDirty(dirty) {
        isDirty = dirty;
        window.onbeforeunload = isDirty ? function() { return "You have unsaved changes."; } : null;
    }

    function SetDefaultSettings() {
        var formmanualapproval = $("#" + formmanualapproval_id).is(':checked');
        var formsubmitmessage = $("#" + formsubmitmessage_id).val();
        var formsubmitpage = $("#" + formsubmitpage_id + " .picker input").val();

        if (formsubmitpage == "" || formsubmitpage == null) {
            formsubmitpage = $("#" + formsubmitpage_id + " .xpath input").val();
        }

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

        UmbracoContour.Webservices.Designer.SetDefaultSettings(formmanualapproval, formdisablestylesheet, formfieldindicationtype, formindicator, formrequirederrormessage, forminvaliderrormessage, formshowvalsum, formhidefieldval, formsubmitmessage, formsubmitpage, SetDefaultsSucces, SetDefaultsFailure);
    }
    function SetDefaultsSucces(retVal) {
        if (retVal.toString() == "true") {
            top.UmbSpeechBubble.ShowMessage('save', 'Settings saved as default', '');
        }
        else {
            top.UmbSpeechBubble.ShowMessage('error', 'Failed to save settings', '');
        }
    }
    function SetDefaultsFailure(retVal) {
        top.UmbSpeechBubble.ShowMessage('error', 'Failed to save settings', '');
    }    

//-->
</script>

</asp:Content>

<asp:Content ID="Content" ContentPlaceHolderID="body" runat="server">

    <umb:TabView ID="TabView1" runat="server" Width="552px" Height="692px" />

    <umb:Pane ID="DesignerPane" runat="server" >
    
    </umb:Pane>
    
     <umb:Pane ID="MainSettingsPane" runat="server" >
        <umb:PropertyPanel ID="ppName" runat="server" Text="Name">
                <asp:TextBox ID="txtName" runat="server" CssClass="guiInputText guiInputStandardSize"></asp:TextBox>
        </umb:PropertyPanel>
        <umb:PropertyPanel ID="ppGuid" runat="server" Text="Guid">
            <%= Request["guid"] %>
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
        
        <script type="text/javascript">
        <!--
            formstorerecordslocally_id = "<%= cbStoreRecordslocally.ClientID %>";
        //-->
        </script>
        
      </umb:Pane>
      
</asp:Content>


<asp:Content ID="Content2" ContentPlaceHolderID="footer" runat="server">
    <asp:PlaceHolder ID="phModals" runat="server"></asp:PlaceHolder>
    
    <script type="text/javascript" src="scripts/umbracoforms.editformpage.eventhandlers.js"></script>
</asp:Content>
