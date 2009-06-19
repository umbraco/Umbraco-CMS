<%@ Page language="c#" Codebehind="edit.aspx.cs" AutoEventWireup="True" Inherits="umbraco.cms.presentation.edit" %>
<HTML>
	<HEAD>
		<script>
		function resizeTabView(TabPageArr, TabViewName) {
		
		var clientHeight =(document.compatMode=="CSS1Compat")?document.documentElement.clientHeight : document.body.clientHeight;
		var clientWidth = document.body.clientWidth;

		var leftWidth = parseInt(clientWidth*0.28);
		var rightWidth = clientWidth; // leftWidth - 40;
		
	
		newWidth = rightWidth; //-20;
		newHeight = clientHeight-37;
			
		document.getElementById(TabViewName + "_container").style.width = newWidth + "px";
		document.getElementById(TabViewName + "_container").style.height = newHeight + "px";
		//document.getElementById(TabViewName + "_container").style.border = "1px solid";
			for (i=0;i<TabPageArr.length;i++) {
				scrollwidth = newWidth - 32;
				document.getElementById(TabPageArr[i] +"layer").style.height = (newHeight-100) + "px";
				document.getElementById(TabPageArr[i] +"layer_menu").style.width = scrollwidth + "px";
				document.getElementById(TabPageArr[i] +"layer_menu_slh").style.width = scrollwidth + "px";
			}
		}

		// Save handlers for IDataFields		
		var saveHandlers = new Array()
		
		function addSaveHandler(handler) {
			saveHandlers[saveHandlers.length] = handler;
		}		
		
		function invokeSaveHandlers() {
			for (var i=0;i<saveHandlers.length;i++) {
				eval(saveHandlers[i]);
			}
		}

		</script>
		<LINK href="css/umbracoGui.css" type="text/css" rel="stylesheet">
	</HEAD>
	<body ms_positioning="GridLayout" onload="resizeTabView(TabView1_tabs, 'TabView1')" onresize="resizeTabView(TabView1_tabs, 'TabView1')"
		bgColor="#f2f2e9">
		<form runat=server id=contentForm>
		<INPUT id="doSave" type="hidden" name="doSave" runat="server">
		<INPUT id="doPublish" type="hidden" name="doPublish" runat="server">
		<asp:PlaceHolder id="plc" Runat="server"></asp:PlaceHolder>
		</form>
	</body>
</HTML>