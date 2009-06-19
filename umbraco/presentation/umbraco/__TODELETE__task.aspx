<%@ Page Trace="true" Language="c#" CodeBehind="__TODELETE__task.aspx.cs" AutoEventWireup="True" Inherits="umbraco.cms.presentation.theTask" %>

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
<head>
	<title>task</title>
	<meta name="GENERATOR" content="Microsoft Visual Studio .NET 7.1" />
	<meta name="CODE_LANGUAGE" content="C#" />
	<meta name="vs_defaultClientScript" content="JavaScript" />
	<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5"/>
</head>
<body>
	<form id="dataForm" method="post" runat="server">
	<input type="text" id="nodeID" runat="server">
	<input type="text" id="nodeType" runat="server">
	<input type="text" id="task" runat="server">
	<input type="text" id="parameterName" runat="server">
	<input type="text" id="parameterNumber" runat="server">
	<asp:Label ID="debug" runat="server" />
	</form>

	<script>
		var expandTries = 0;
		
		var refresh = true;
		var refreshParent = false;
		var dontDelete = false;
		
		<asp:PlaceHolder ID="dontRefresh" Runat="server"/>


		if (refreshParent) {
			
			if (parent.node != null && refresh) {
				if (parent.node.parentNode != null) {
					if (dontDelete)
						parent.node = parent.node.parentNode;
					else {
						if (parent.node.parentNode.childNodes.length == 1)
							parent.node.parentNode.childNodes = [];

						// Add delete puff effectEffects
						parent.node.collapse();
						new parent.tree.Effect.DropOut(parent.node.id);

						setTimeout('parent.node.remove()', 1000);
					}
				}
			}
		} 
		
		if (!refreshParent || (refreshParent && dontDelete)) {
			if (parent.node != null && refresh) {
				if (parent.node.parentNode != null) {
					if (parent.node.src != "" && parent.node.src != null) {
						parent.node.src = parent.node.src + '&rnd=' + parent.returnRandom();
						parent.node.reload();
						setTimeout("expandNode()", 200);
					} else if (parent.node.srcRoot != "" && parent.node.srcRoot != null) {
						parent.node.src = parent.node.srcRoot;
						parent.node.reload();
						parent.node.expand();
						setTimeout("expandNode()", 200);
					}	
				} else
					parent.tree.document.location.href = parent.tree.document.location.href;
			} else if (!refresh)
				parent.tree.document.location.href = parent.tree.document.location.href;
		}
		
		
		function expandNode() {
			if (parent.node.childNodes.length == 0 && expandTries < 10) {
				expandTries++;
				setTimeout("expandNode()", 200);
			} else {
				parent.node.expand();
				expandTries = 0;
			}
		}
	</script>

</body>
</html>
