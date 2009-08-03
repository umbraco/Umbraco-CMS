<%@ Page Language="c#" Codebehind="treePicker.aspx.cs" AutoEventWireup="True" Inherits="umbraco.dialogs.treePicker" %>
<%@ Register TagPrefix="umb" Namespace="umbraco.presentation.ClientDependency.Controls" Assembly="umbraco.presentation.ClientDependency" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >

<head runat="server">
	<title>umbraco</title>
  
  <style type="text/css">
  #header{
  background: url(../../umbraco_client/modal/modalGradiant.gif) repeat-x bottom #fff;
  border-bottom: 1px solid #CCC;
  }
 
  #caption {
  font: bold 100% "Lucida Grande", Arial, sans-serif;
  text-shadow: #FFF 0 1px 0;
  padding: .5em 2em .5em .75em;
  margin: 0;
  text-align: left;
  }

#close {
 display: block;
 position: absolute;
 right: 5px; top: 4px;
 padding: 2px 3px;
 font-weight: bold;
 text-decoration: none;
 font-size: 13px;
}

#close:hover {
 background: transparent;
}
#body{padding-left: 7px;}

</style>
</head>
<body onload="this.focus()" style="overflow: hidden; padding: 0px; margin: 0px;">
	<script type="text/javascript" language="javascript">
			function dialogHandler(id) {
			<%
			    if (umbraco.helper.Request("useSubModal") != "") {%>
                window.parent.hidePopWin(true, id)
			<%
     } else
     {
    %>
				    window.returnValue = id;
				    window.close();
    <%
    }
    %>
			}
			
			function closeWindow() {
			<%
			    if (umbraco.helper.Request("useSubModal") != "") {%>
                window.parent.hidePopWin(false)
			<%
      } else
      {
      %>
				      window.close();
      <%
      }
      %>
			}
	</script>

	<cc1:UmbracoClientDependencyLoader runat="server" id="ClientLoader" />
	
	<umb:CssInclude runat="server" FilePath="css/umbracoGui.css" PathNameAlias="UmbracoRoot" />
	<umb:JsInclude ID="JsInclude1" runat="server" FilePath="ui/jquery.js" PathNameAlias="UmbracoClient" />

  <div id="umbModal">
    <div id="header">
      <div id="caption">Pick a node</div>
      <a id="close" onclick="closeWindow(); return false;"  title="Close window" href="#"><span>&times;</span></a>
    </div>
    <div id="body">
        <iframe src="<%=TreeInitUrl %>"  frameborder="0" style="overflow: auto; width: 291px; position: relative; height: 370px; background: white"></iframe>
    </div>
  </div>
</body>
</html>
