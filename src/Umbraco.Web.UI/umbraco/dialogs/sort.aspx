<%@ Page Language="c#" MasterPageFile="../masterpages/umbracoDialog.Master"Codebehind="sort.aspx.cs" AutoEventWireup="True" Inherits="umbraco.cms.presentation.sort" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>

<asp:Content ContentPlaceHolderID="head" runat="server">

  <style type="text/css">
    #sortableFrame{height: 270px; overflow: auto; border: 1px solid #ccc;}
    #sortableNodes{padding: 4px; display: block}
    #sortableNodes thead tr th{border-bottom:1px solid #ccc; padding: 4px; padding-right: 25px;
                                background-image: url(<%= umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco_client) %>/tableSorting/img/bg.gif);     
                                cursor: pointer; 
                                font-weight: bold; 
                                background-repeat: no-repeat; 
                                background-position: center right; 
                               }
    
    #sortableNodes thead tr th.headerSortDown { 
      background-image: url(<%= umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco_client) %>/tableSorting/img/desc.gif); 
    }
     
    #sortableNodes thead tr th.headerSortUp { 
      background-image: url(<%= umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco_client) %>/tableSorting/img/asc.gif); 
    } 
    
    #sortableNodes tbody tr td{border-bottom:1px solid #efefef}
    #sortableNodes td{padding: 4px; cursor: move;}  
    tr.tDnD_whileDrag , tr.tDnD_whileDrag td{background:#dcecf3; border-color:  #a8d8eb !Important; margin-top: 20px;}
    #sortableNodes .nowrap{white-space: nowrap; } 
    </style>
  
  

  
</asp:Content>

<asp:Content ContentPlaceHolderID="body" runat="server">

	<umb:JsInclude ID="JsInclude1" runat="server" FilePath="tablesorting/tableFilter.js" PathNameAlias="UmbracoClient"/>
	<umb:JsInclude ID="JsInclude2" runat="server" FilePath="tablesorting/tableDragAndDrop.js" PathNameAlias="UmbracoClient"/>

<div id="loading" style="display: none;">
<div class="notice">
      <p><%= umbraco.ui.Text("sort", "sortPleaseWait") %></p>
</div>
<br />
    <cc1:ProgressBar ID="prog1" runat="server" Title="sorting.." />
</div>

<div id="sortingDone" style="display: none;" class="success">
  <p><asp:Literal runat="server" ID="sortDone"></asp:Literal></p>
  <p>
  <a href="#" onclick="UmbClientMgr.closeModalWindow()"><%= umbraco.ui.Text("defaultdialogs", "closeThisWindow")%></a>
  </p>
</div>

<div id="sortArea">
<cc1:Pane runat="server" ID="sortPane">
  <p class="help">
    <%= umbraco.ui.Text("sort", "sortHelp") %>
  </p>
  
  <div id="sortableFrame">
    <table id="sortableNodes" cellspacing="0">
      <colgroup>
	      <col/>
	      <col/>
	      <col/>
	    </colgroup>
	    <thead>
	    <tr>
        <th style="width: 100%">Name</th>
        <th class="nowrap">Creation date</th>
        <th class="nowrap">Sort order</th>
      </tr>
      </thead>
      <tbody>
        <asp:Literal ID="lt_nodes" runat="server" />
      </tbody>
    </table>
  </div>
</cc1:Pane>

  <br />
  <p>
    <input type="button" onclick="sort(); return false;" value="<%=umbraco.ui.Text("save") %>" />
    <em> or </em><a href="#" style="color: blue" onclick="UmbClientMgr.closeModalWindow()"><%=umbraco.ui.Text("general", "cancel", this.getUser())%></a>  
  </p>
</div>
  
   <script type="text/javascript">

     jQuery(document).ready(function() {
       jQuery("#sortableNodes").tablesorter();
       jQuery("#sortableNodes").tableDnD({containment: jQuery("#sortableFrame") } );
     });


     function sort() {
       var rows = jQuery('#sortableNodes tbody tr');
       var sortOrder = "";

       jQuery.each(rows, function() {
         sortOrder += jQuery(this).attr("id").replace("node_","") + ",";
       });
               
        document.getElementById("sortingDone").style.display = 'none';
        document.getElementById("sortArea").style.display = 'none';
        	    
		    document.getElementById("loading").style.display = 'block';	    

            var _this = this;
            $.ajax({
                type: "POST",
                url: "<%= umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco)%>/WebServices/NodeSorter.asmx/UpdateSortOrder?app=<%=umbraco.helper.Request("app")%>",
                data: '{ "ParentId": ' + parseInt(<%=umbraco.helper.Request("ID")%>) + ', "SortOrder": "' + sortOrder + '"}',
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function(msg) {
                    showConfirm();
                }
            });

      }       
       
      function showConfirm() {      
		    document.getElementById("loading").style.display = 'none';	    
		    document.getElementById("sortingDone").style.display = 'block';	
		    UmbClientMgr.mainTree().reloadActionNode();
		  }
		  
  </script>
  
</asp:Content>
  