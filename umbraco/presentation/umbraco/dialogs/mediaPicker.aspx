<%@ Page Language="C#"  AutoEventWireup="true" CodeBehind="mediaPicker.aspx.cs" Inherits="umbraco.presentation.umbraco.dialogs.mediaPicker" %>
<%@ Register TagPrefix="ui" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>


<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title></title>
    
    <!-- Default script and style -->
	<umb:CssInclude ID="CssInclude1" runat="server" FilePath="ui/default.css" PathNameAlias="UmbracoClient" />
     
     <umb:JsInclude ID="JsInclude1" runat="server" FilePath="Application/NamespaceManager.js" PathNameAlias="UmbracoClient" Priority="0"  />     
     <umb:JsInclude ID="JsInclude3" runat="server" FilePath="ui/jquery.js" PathNameAlias="UmbracoClient" Priority="1"  />
     <umb:JsInclude ID="JsInclude4" runat="server" FilePath="Application/UmbracoClientManager.js" PathNameAlias="UmbracoClient" Priority="2"  />
     <umb:JsInclude ID="JsInclude2" runat="server" FilePath="ui/default.js" PathNameAlias="UmbracoClient" Priority="5"  />     
    
    <script type="text/javascript" language="javascript">




        var mediaid = -1;
        
        function dialogHandler(id) {

            if (id != -1) {
                mediaid = id;
                jQuery("#submitbutton").attr("disabled", false);
            }
            else 
            {           
                jQuery("#submitbutton").attr("disabled", true);
            }

            document.getElementById('imageViewer').src = '/umbraco/dialogs/imageViewer.aspx?id=' + id;
        }
        function updateImageSource(src, alt, width, height,id) {

            if (id != null) {
                mediaid = id;
                jQuery("#submitbutton").attr("disabled", false);
            }
            jQuery("#previewImage").css("background-image", "url(" + src.substring(0, (src.length - 4)) + "_thumb.jpg)");
        }

        function refreshTree() {
            jQuery("#treeFrame").attr("src", jQuery("#treeFrame").attr("src"));
        }

        function UpdatePicker() {
           if(mediaid != -1)
           {
                parent.hidePopWin(true,mediaid);
           }
        }
    
</script>
</head>


<body class="umbracoDialog" style="margin: 15px 10px 0px 10px;">
    <ui:UmbracoClientDependencyLoader runat="server" id="ClientLoader" />

<form id="Form1" runat="server" onsubmit="UpdatePicker();return false;" action="#">
 <ui:Pane ID="pane_src" runat="server" >
 <div style="height:105px"></div>
 <div id="previewImage" style="width: 105px; height: 105px; background: #fff center center no-repeat; border: 1px solid #ccc; position: absolute; top: 3px; right: 3px;">
         
 </div>
 </ui:Pane>
      <br /> 
 <ui:TabView AutoResize="false" Width="455px" Height="305px" runat="server"  ID="tv_options" />
<ui:Pane ID="pane_select" runat="server"> 
      <div style="padding: 5px; background: #fff; height: 250px;">
        <iframe id="treeFrame" name="treeFrame" src="../TreeInit.aspx?app=media&isDialog=true&dialogMode=id&contextMenu=false&functionToCall=parent.dialogHandler" style="width: 405px; height: 250px; float: left; border: none;" frameborder="0"></iframe>
        <iframe src="imageViewer.aspx" id="imageViewer" style="width: 0px; height: 0px; visibility: hidden; float: right; border: none;" frameborder="0"></iframe>
      </div>
    </ui:Pane>
    <asp:Panel ID="pane_upload" runat="server">
        <iframe frameborder="0" src="uploadImage.aspx" style="border: none; width: 435px; height: 250px;"></iframe>
    </asp:Panel>
    
    <br />
     <p>
        <input type="submit" value="select" style="width: 60px;" disabled="true" id="submitbutton"/> <em id="orcopy">or</em>
        <a href="#" style="color: blue" onclick="parent.hidePopWin(false,0);" id="cancelbutton">{#cancel}</a>
      </p>      
      </form>

 <script type="text/javascript" language="javascript">
     jQuery(document).ready(function() {
         jQuery("#submitbutton").attr("value", '<%= umbraco.ui.Text("select") %>');
         jQuery("#cancelbutton").text('<%= umbraco.ui.Text("cancel") %>');
         jQuery("#orcopy").text('<%= umbraco.ui.Text("or") %>'); 
     });
 </script>

</body>
</html>
