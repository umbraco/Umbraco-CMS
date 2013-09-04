<%@ Page MasterPageFile="../../masterpages/umbracoPage.Master" Language="C#" AutoEventWireup="true" CodeBehind="editFormEntries.aspx.cs" Inherits="Umbraco.Forms.UI.Pages.editFormEntries" %>
<%@ Register Namespace="umbraco.uicontrols" Assembly="controls" TagPrefix="umb" %>

<asp:Content ID="headContent" ContentPlaceHolderID="head" runat="server">
      
      <link rel="Stylesheet" href="css/datatables.css" type="text/css" />
      <script src="scripts/jquery.datatables.js" type="text/javascript"></script>
      <script src="scripts/ZeroClipboard.js" type="text/javascript" charset="utf-8"></script>
      <script src="scripts/TableTools.js" type="text/javascript" charset="utf-8"></script>
      <link rel="stylesheet" href="css/style.css" type="text/css" media="screen" />
      <link rel="stylesheet" href="css/TableTools.css" type="text/css" media="screen" />

      <script type="text/javascript">
             var webserviceUrl = "webservices/records.aspx?guid=<%= formGuid %>";
             var actionsUrl = "webservices/recordActions.aspx?form=<%= formGuid %>";
             var tableName = "#entries<%= formGuid.Replace("-","") %>";

             var otable;
             var arr = new Array();

			 var selectedIds = new Array();
             var selectedContextGuid;
             

             $(document).ready(function() {
                


                TableToolsInit.sSwfPath = "/umbraco/plugins/umbracoContour/scripts/ZeroClipboard.swf";

				otable = $(tableName).dataTable( {
				    "sPaginationType": "full_numbers",
                    "bStateSave": true,
					"bProcessing": true,
					"bServerSide": true,
					"sAjaxSource": webserviceUrl,
                    "sServerMethod": "POST",
                    "aaSorting": [[ 1, "desc" ]],
					"aoColumns": [ 
			                        {"fnRender": renderKey, "bSortable": false}, {"fnRender": renderDate},{"fnRender": renderString, "bSortable": false},{"fnRender": renderString, "bSortable": false},{"fnRender": renderString, "bSortable": false} <asp:literal id="colSettings" runat="server"/>
			                     ],
                    "sDom": 'T<"clear">lfrtip',
				    "fnDrawCallback" : function() {
				        ResizeContainer();
				        ReplaceFileDownloads();
				    }
				});
								
				$(tableName).css("width","100%");

				$("#delBt").click(deleteSelected);
                $(".recordOption").click(openContextMenu);

                $("#bt_setAction").click(function(){
                    if($("#recordSetActions").val() != ''){
                        eval($("#recordSetActions").val());
                    }
                });
                
              $('#recordContextMenu li').hover(function() {

                    $(this).addClass('activemenuitem');

                }, function() {

                    $(this).removeClass('activemenuitem');

               });
               
               $('#recordContextMenu').hover(function() {}, function() {

                   $('#recordContextMenu').hide();
               }); 
                     
                     
               
               $( "#columnToggler li" ).each(
	                function( intIndex ){
                                if(!otable.fnSettings().aoColumns[intIndex].bVisible){jQuery('a',this).addClass("hidden");}
 	                }
                );         
                				
			 });
             
             function ResizeContainer() {
                //update ui for big form
                
                if($("table thead").width() > $(".content").width())
                {
	                $(".propertypane").width($("table thead").width() + 35);
                }
             }
             
             function ReplaceFileDownloads(){
                 $(".content td").each(function () { 
	     
	     	    if($(this).html().indexOf("/umbraco/plugins") != -1){
	     		    var c = $(this).html();
	     		    var file = c.split('/').pop();
	     		    var h = "<a href='"+c.replace('~','')+"' target='_blank'>"+file+"</a>";
	     		    $(this).html(h);
	     	        } 
		        });
             }
             
             function openContextMenu(link){
                var menu = $("#recordContextMenu");
                var l = $(link);

                selectedContextGuid = l.attr('rel');

                //this needs polish
                menu.css("top", l.offset().top + 5).css("left", l.offset().left + 5); 
                menu.show();
             }



             function ExecuteRecordSetAction(action, needsConfirm, confirmMessage){

                 var execute = true;

                 if(needsConfirm != null){
                    execute = confirm(confirmMessage);
                 }

                 if(execute){

                    
                    jQuery.get(actionsUrl + '&records=' + selectedIds.join(',') + '&action=' + action, function(data) {
                        FetchRecords();

                        jQuery("#buttons:visible").fadeOut();
                    });
                }
             }

             function ExecuteRecordAction(action, needsConfirm, confirmMessage){
                 $('#recordContextMenu').hide();

                 var execute = true;

                 if(needsConfirm != null){
                    execute = confirm(confirmMessage);
                 }

                 if(execute){
                    jQuery.get(actionsUrl + '&record=' + selectedContextGuid + '&action=' + action, function(data) {
                   
                        FetchRecords();
                    });
                }
             }

             function FetchRecords(){
                var state = jQuery("#<%= dd_state.ClientID %>").val();
                var date = jQuery("#<%= dd_timespan.ClientID %>").val();
                
                otable.fnSettings().sAjaxSource = webserviceUrl + "&date=" + date + "&state=" + state;            
                otable.fnDraw();
               
                arr = new Array();
                selectedIds = new Array();
             }
             

             function ShowExportFormEntriesDialog(){
                 var src = "exportFormEntriesDialog.aspx?guid=<%= formGuid %>";;
                 $.modal('<iframe src="' + src + '" height="400" width="600" style="border:0">');
             }
             

             function ShowRecordActionSettingsDialog(action){
                 var src = "executeRecordAction.aspx?form=<%= formGuid %>&action=" + action + "&record=" + selectedContextGuid;
                 $.modal('<iframe src="' + src + '" height="400" width="600" style="border:0">');
             }

            function ShowRecordSetActionSettingsDialog(action){
                 var src = "executeRecordAction.aspx?form=<%= formGuid %>&action=" + action + "&records=" + selectedIds.join(',');
                 $.modal('<iframe src="' + src + '" height="400" width="600" style="border:0">');
             }


             function CloseExportFormEntriesDialog(){
                $.modal.close();
             }
                
             function CloseRecordActionSettingsDialog(refresh) {
                if (refresh != null) {    
                   FetchRecords();
                }

                $.modal.close();
             }		
                        
                        
                 /* RENDERS */
			    function renderKey(obj){
				    return  "<a href='#' class='recordOption' onClick='javascript:openContextMenu(this);' rel='" + obj.aData[obj.iDataColumn] + "'>options</a> <input onClick='javascript:selectColumn(this);' type='checkbox' id='" + obj.aData[obj.iDataColumn] +"'/>";				
				}				
				function renderState(obj){
				    return  "<span class='" + obj.aData[obj.iDataColumn] + "'>" + obj.aData[obj.iDataColumn] + "</span>";						
				}				
				function renderDate(obj){
				    return  obj.aData[obj.iDataColumn] ;						
				}				
				function renderString(obj){
				    return  obj.aData[obj.iDataColumn];						
				}				
				function renderLongString(obj){
				    return  obj.aData[obj.iDataColumn];					
				}				
				function renderBoolean(obj){
				   return  obj.aData[obj.iDataColumn];		
				}
                
                /* for selecting multiple records */				
				function selectColumn(checkbox){
				   var row = jQuery(checkbox).parent().parent();
				   var id = jQuery(checkbox).attr("id");

                   row.toggleClass("selected");
				   
				   var index = jQuery.inArray(row, arr);
				   
				   if(checkbox.checked){
				   
                   if(index < 0){
				        arr[arr.length] = row;
				        selectedIds[arr.length] = id;
                        }
                            
                   }else{
				        arr.splice(index, 1);
                        selectedIds.splice(index, 1);
				   }

                   
                   if(selectedIds.length > 1)
                        jQuery("#buttons:hidden").fadeIn();
                   else
                        jQuery("#buttons:visible").fadeOut();
                   
				}
				

                			
				function fnShowHide(iCol,sender)
			    {
				    var bVis = otable.fnSettings().aoColumns[iCol].bVisible;
				    otable.fnSetColumnVis( iCol, bVis ? false : true );

                    jQuery(sender).toggleClass("hidden");
			    }


                /* not in use anymore.. this is supported by a generic model instead */
				function deleteSelected(){
				if( confirm("Are you sure you want to delete these entries?") ){
				   
				   jQuery.each(arr, function(){
				        var cb = jQuery("input", this);
				        jQuery.get(webserviceUrl + '&record=' + cb.attr("id") + '&mode=delete');
				        this.hide();				   
				   });
				   
				   arr = new Array();
				   
				   }				   
				}
				function approveSelected(){
				   jQuery.each(arr, function(){
				       var cb = jQuery("input", this);
				       var stateTd = jQuery("td.stateCol", this);
				       
				        jQuery.get(webserviceUrl + '&record=' + cb.attr("id") + '&mode=approve');
				        cb.attr("checked", false);
				        
				        this.removeClass("selected");
				        
				        stateTd.html("<span class='Approved'>Approved</span>");  	   
				   });
				   
				   arr = new Array();
				}					
      </script> 
      
      <style type="text/css">
        #columnToggler li{padding-right: 20px; float: left;}
        
        .TableTools
        {
            position:absolute;
            right:0;
            top:-73px;
            border:none;
            background-color:transparent;
        }
        
        .TableTools_print
        {
            display:none;
        }
      </style>

</asp:Content>

<asp:Content ID="Content" ContentPlaceHolderID="body" runat="server">

     
      <script type="text/javascript" src="../../../umbraco_client/<%= Umbraco.Forms.UI.Config.JqueryUI %>"></script>
      <script type="text/javascript" src="scripts/jquery.simplemodal-1.2.3.js"></script>

     <umb:UmbracoPanel ID="Panel1" runat="server" hasMenu="true" Text="Form entries">
        
        <umb:Pane runat="server">
            <umb:PropertyPanel runat="server">
                
                <p>
                    Get all 
                    
                    <asp:DropDownList runat="server" ID="dd_state" /> 
                    
                    records from the last 
                    
                    <asp:DropDownList ID="dd_timespan" runat="server" />
                
                    <a id="refresh" href="javascript:void(0);"  onclick="FetchRecords();">Reload</a>
                </p>
                                
            </umb:PropertyPanel>
        </umb:Pane>
        
                        
         <umb:Pane ID="Pane1" runat="server" Text="Form Entries">
          
           
          <div id="dynamic">
            <table cellpadding="0" cellspacing="0" border="0" class="display" style="width: 700px" id="entries<%= formGuid.Replace("-","") %>"> 
	            <thead> 
		            <tr> 
		                <th style="width: 25px !Important"></th>
		                <th>Created</th>
		                <th>Ip</th>    	
		                <th>PageID</th>    	
		                <th>Url</th>    	                
		                <asp:literal runat="server" ID="colHeaders" />
			        </tr> 
	            </thead> 
	            <tbody>
	            </tbody>
	        </table>
            
            <div>

	           <div id="buttons" style="display: none; clear: both; padding: 10px; float: left;">
                                <select id="recordSetActions">
                                    <option>Choose..</option>
                                    <asp:literal runat="server" ID="lt_renderedRSActions" />
                                </select>
                                <input type="button" id="bt_setAction" value="Execute" style="width: 70px;"/>
	           </div>
                        

               
               
               	                    
	        </div> 
             
         </div> 
                  
         <div class="spacer"></div> 
         </umb:Pane>

         <umb:Pane ID="p_toggleColoumns" runat="server">
            <umb:PropertyPanel runat="server" Text="Toggle columns">         
               <ul id="columnToggler">
                    <li><a href="javascript:void(0);"  onclick="fnShowHide(0,this);">Options</a></li>
                    <li><a href="javascript:void(0);"  onclick="fnShowHide(1,this);">Created</a></li>
		            <li><a href="javascript:void(0);"  onclick="fnShowHide(2,this);">Ip</a></li>    	
		            <li><a href="javascript:void(0);"  onclick="fnShowHide(3,this);">PageID</a></li>    	
		            <li><a href="javascript:void(0);"  onclick="fnShowHide(4,this);">Url</a></li>
                    <asp:Literal ID="lt_columnToggler" runat="server" />        
                </ul>
            </umb:PropertyPanel>
         </umb:Pane>

     </umb:UmbracoPanel>
     

     <ul id="recordContextMenu" style="display:none;">
        <asp:literal runat="server" ID="lt_renderedRActions" />
     </ul>
     
</asp:Content>