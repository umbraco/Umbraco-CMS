<%@ Page MasterPageFile="../../masterpages/umbracoPage.Master" Language="C#" AutoEventWireup="true" CodeBehind="editFormWorkflows.aspx.cs" Inherits="Umbraco.Forms.UI.Pages.editFormWorkflows" %>

<%@ Register Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
    Namespace="System.Web.UI" TagPrefix="asp" %>
<%@ Register Namespace="umbraco.uicontrols" Assembly="controls" TagPrefix="umb" %>


<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

<link rel="stylesheet" href="css/style.css" type="text/css" media="screen" />
<link href="../../../umbraco_client/GenericProperty/genericproperty.css" type="text/css" rel="stylesheet">
<style>
 .notactive
 {
 	color: #888888;
 }
 .workflowname
 {
 	float:left;
 }
</style>


<script type="text/javascript">
<!--
    $(document).ready(function() {

        $(".statecontainer").sortable({
            connectWith: '.statecontainer',
            items: '.sortablepropertypane:not(.empty)',
            update: function() {

                var wfGuids = new Array();
                var wfcount = 0;
                $(this).children().each(function() {
                    if ($(this).attr("rel") != null) {
                        wfGuids[wfcount] = $(this).attr("rel");
                        wfcount++;
                    }
                });

                $(this).children('.empty').remove();
                CheckStateWorkflows();
                
                var state = $(this).attr('id');
                UmbracoContour.Webservices.Workflows.UpdateSortOrder(state, wfGuids, SortOrderSuccess, SortOrderFailure);

            }
        });

        CheckStateWorkflows();

    });


function SortOrderSuccess(retVal) {
    if (retVal.toString() == "true") {
        top.UmbSpeechBubble.ShowMessage('save', 'Workflows updated', '');
    }
    else {
        top.UmbSpeechBubble.ShowMessage('error', 'Failed to update workflows', '');
    }
}
function SortOrderFailure(retVal) {
    top.UmbSpeechBubble.ShowMessage('error', 'Failed to update workflows', '');

}


function CheckStateWorkflows() {

    var nowf = "<div class='empty'>No workflows defined for this state. Click on the 'add a new workflow' link at the top to create a new workflow.</div>";

    $(".statecontainer").each(function() {
        if ($(this).children().size() == 0) {
          
            $(this).append(nowf);
        }
      
    });
    
    
}

function AddNewWorkFlowToUI(state, name, guid, active) {

    var cssclass = "workflowname";

    if (active != 'True' && active != 'true') {
        cssclass += " notactive"
    }
    
    var newwf = "<div style='margin-left:10px;margin-right:10px;' id='" + guid + "' class='propertypane sortablepropertypane' rel='" + guid + "'>";
    newwf += "<div>";
    newwf += "<span class='" + cssclass + "'>" + name + "</span>";
    newwf += " <a class='update' href=\"javascript:ShowUpdateWorkFlowDialog('" + guid + "');\">Update</a>";
    newwf += " <a class='delete' style='color: Red;' href=\"javascript:DeleteWorkFlow('" + guid + "');\">Delete</a>";
    newwf += "<br style='clear:both;' /></div></div>";
    $("#" + state + ' .empty').toggle();
    $("#" + state).append(newwf);
    CheckStateWorkflows();
}

function ShowUpdateWorkFlowDialog(wfguid, state) {
    var src = "editWorkflowDialog.aspx?guid=" + wfguid + "&state=" + state + "&form=<%= Request["guid"] %>";
    $.modal('<iframe src="' + src + '" height="400" width="600" style="border:0">');
}

function CloseUpdateWorkFlowDialog(updated,id,name,active) {
    if (updated != null) {
        
        $("#" + id + " .workflowname").removeClass("notactive");
        if (active != 'True' && active != 'true') {
            $("#" + id + " .workflowname").addClass("notactive");
        }
        $("#" + id + " .workflowname").text(name);
        top.UmbSpeechBubble.ShowMessage('save', 'Workflow updated', '');
    }
    $.modal.close();
}

function DeleteWorkFlow(wfguid) {
    if (ConfirmDelete()) {
        UmbracoContour.Webservices.Workflows.Delete(wfguid, DeleteSuccess, DeleteFailure);
        $('#' + wfguid).remove();
        CheckStateWorkflows();
    }
}

function DeleteSuccess(retVal) {
    if (retVal.toString() == "true") {
        top.UmbSpeechBubble.ShowMessage('save', 'Workflow deleted', '');
    }
    else {
        top.UmbSpeechBubble.ShowMessage('error', 'Failed to delete workflow', '');
    }
}
function DeleteFailure(retVal) {
    top.UmbSpeechBubble.ShowMessage('error', 'Failed to delete workflow', '');
}

function ConfirmDelete() {
    if (confirm("Are you sure you want to delete this item?") == true)
        return true;
    else
        return false;
}

function ToggleAdd() {
    $("#showadd").toggle();
    $("#addworkflow").toggle();
  }

function ShowAdvanced()
{
    $("#showadvanced").hide();
    $("div.advanced").show();
    $("#showsimple").show();
}

function ShowSimple()
{
    $("#showadvanced").show();
    $("div.advanced").hide();
    $("#showsimple").hide();
}
   //-->
</script>
 
</asp:Content>

<asp:Content ID="Content" ContentPlaceHolderID="body" runat="server">


<script type="text/javascript" src="../../../umbraco_client/<%= Umbraco.Forms.UI.Config.JqueryUI %>"></script>
<script type="text/javascript" src="scripts/jquery.simplemodal-1.2.3.js"></script>

   <umb:UmbracoPanel ID="Panel1" runat="server" hasMenu="false" Text="Form workflows">
   
   <umb:Pane ID="pane1" runat="server">
    <umb:PropertyPanel runat="server">
    <p style="text-align: center;">
      A form comes with a basic workflow which allows you to attach extra functionality on each step of the forms life-cycle.
    </p>
    </umb:PropertyPanel>
    
    <umb:PropertyPanel ID="PropertyPanel1" runat="server">
    <p style="text-align: center;">
      Use these steps to integrate with 3rd party systems, modify data or perform basic filtering
    </p>
   <p style="text-align: center;" id="showadvanced">
        There are even more events to use if you turn on the <a href="#" onClick="javascript:ShowAdvanced(); return false;">Advanced mode</a>
    </p>
     <p style="text-align: center;display:none;" id="showsimple">
        Back to <a href="#" onClick="javascript:ShowSimple(); return false;">Simple mode</a>
    </p>
    
    </umb:PropertyPanel>
   
   </umb:Pane>
   
   
   <div id="workflows">
      <asp:PlaceHolder ID="phStates" runat="server"></asp:PlaceHolder>
      <h2>Success!</h2>
   </div>
    
   </umb:UmbracoPanel>
</asp:Content>
