<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="insertImage.aspx.cs" Inherits="umbraco.presentation.plugins.tinymce3.insertImage" %>
<%@ Register TagPrefix="ui" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>
<%@ Register TagPrefix="umb2" TagName="Tree" Src="../../controls/Tree/TreeControl.ascx" %>
<%@ Register TagPrefix="umb3" TagName="Image" Src="../../controls/Images/ImageViewer.ascx" %>
<%@ Register TagName="MediaUpload" TagPrefix="umb4" Src="../../controls/Images/UploadMediaImage.ascx" %>
<%@ Register TagPrefix="umbClient" Namespace="Umbraco.Web.UI.Bundles" Assembly="umbraco" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>{#advimage_dlg.dialog_title}</title>

    <base target="_self" />

    <ui:UmbracoClientDependencyLoader runat="server" id="ClientLoader" />
	
	<umbClient:JsApplicationLib runat="server" />
    <umbClient:JsJQueryCore runat="server" />
    <umbClient:JsUmbracoApplicationCore runat="server" />

	<umb:JsInclude ID="JsInclude9" runat="server" FilePath="Application/jQuery/jquery.noconflict-invoke.js" PathNameAlias="UmbracoClient" Priority="1" />
	<umb:JsInclude ID="JsInclude8" runat="server" FilePath="ui/default.js" PathNameAlias="UmbracoClient" Priority="4" />
	<umb:JsInclude ID="JsInclude4" runat="server" FilePath="tinymce3/tiny_mce_popup.js" PathNameAlias="UmbracoClient" Priority="100" />
	<umb:JsInclude ID="JsInclude5" runat="server" FilePath="tinymce3/utils/mctabs.js" PathNameAlias="UmbracoClient" Priority="101" />
	<umb:JsInclude ID="JsInclude6" runat="server" FilePath="tinymce3/utils/form_utils.js" PathNameAlias="UmbracoClient" Priority="102" />
	<umb:JsInclude ID="JsInclude7" runat="server" FilePath="tinymce3/utils/validate.js" PathNameAlias="UmbracoClient" Priority="103" />

	<umb:JsInclude ID="JsInclude2" runat="server" FilePath="tinymce3/utils/editable_selects.js" PathNameAlias="UmbracoClient" Priority="110" />
	<umb:JsInclude ID="JsInclude1" runat="server" FilePath="tinymce3/plugins/umbracoimg/js/image.js" PathNameAlias="UmbracoClient" Priority="111" />

    
    
    <style type="text/css">        
        .imageViewer .bgImage {position: absolute; top: 3px; right: 3px; }
    </style>
    
</head>
<body id="advimage" style="display: none">
    
    <script type="text/javascript" language="javascript">
        function dialogHandler(id) {
            if (id != -1) {
                jQuery("#<%=ImageViewer.ClientID%>").UmbracoImageViewerAPI().updateImage(id, function(p) {
                    //when the image is loaded, this callback method fires
                    if (p.hasImage) {
                        updateImageSource(p.url, p.alt, p.width, p.height);
                        return;
                    }
                    else {
                        alert("An error occured: Image selected could not be found");
                    }
                });
            }                     
        }

        function uploadHandler(e) {
            dialogHandler(e.id);
            //get the tree object for the chooser and refresh
            var tree = jQuery("#<%=DialogTree.ClientID%>").UmbracoTreeAPI();
            tree.refreshTree();
        }

        function updateImageSource(src, alt, width, height) {
            // maybe we'll need to remove the umbraco path from the src
            var umbracoPath = tinyMCE.activeEditor.getParam('umbraco_path');
            if (src.substring(0, umbracoPath.length) == umbracoPath) {
                // if the path contains a reference to the umbraco path, it also contains a reference to go up one level (../)
                src = src.substring(umbracoPath.length + 3, src.length);
            }

            var formObj = document.forms[0];
            formObj.src.value = src;

            formObj.alt.value = alt;

            var imageWidth = width;
            var imageHeight = height;
            var orgHeight = height;
            var orgWidth = width;
            var maxWidth = parseInt(tinyMCE.activeEditor.getParam('umbraco_maximumDefaultImageWidth'));

            if (imageWidth != '' && imageWidth > maxWidth) {
                if (imageWidth > imageHeight)
                    orgRatio = parseFloat(imageHeight / imageWidth).toFixed(2)
                else
                    orgRatio = parseFloat(imageWidth / imageHeight).toFixed(2)
                imageHeight = Math.round(maxWidth * parseFloat(imageHeight / imageWidth).toFixed(2));
                imageWidth = maxWidth;
            }

            if (imageWidth > 0)
                formObj.width.value = imageWidth;

            if (orgWidth > 0)
                formObj.orgWidth.value = orgWidth;

            if (imageHeight > 0)
                formObj.height.value = imageHeight;

            if (orgHeight > 0)
                formObj.orgHeight.value = orgHeight;
        }


        function changeDimensions(mode) {
            var f = document.forms[0], tp, t = this;

            var bTop = f.orgHeight;
            var bBot = f.orgWidth;
            var rTop = f.height;
            var rBot = f.width;

            if (mode == "width") {
                rTop.value = ((bTop.value * rBot.value) / bBot.value).toFixed(0);
                rBot.value = Number(rBot.value).toFixed(0);
            } else if (mode == "height") {
                rBot.value = ((bBot.value * rTop.value) / bTop.value).toFixed(0);
                rTop.value = Number(rTop.value).toFixed(0);
            }
        }

        var functionsFrame = this;
        var tabFrame = this;
        var isDialog = true;
        var submitOnEnter = true;
        var preloadImg = true;
        
        jQuery(document).ready(function() {
            //show the image if one is selected
            setTimeout(function() {
                if (document.forms[0].src.value != "") {
                    //get the thumb of the image
                    var src = document.forms[0].src.value;
                    var ext = src.split('.').pop();
                    var thumb = src.replace("." + ext, "_thumb.jpg");
                    if (src != "") jQuery("#<%=ImageViewer.ClientID%>").UmbracoImageViewerAPI().showImage(thumb);
                }
            }, 500);
        });

        jQuery(window).load(function() {
            //for some very silly reason, we need to manually initialize the tree on window load as firefox and chrome won't load
            //the tree properly when in the TinyMCE window. This is why we need to specify ManualInitialization="true" on the tree
            //control.
            var tree = jQuery("#<%=DialogTree.ClientID%>").UmbracoTreeAPI();
            tree.rebuildTree("media");
        });
        
    </script>
    
	
      
    <form id="Form1" runat="server">
    
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
    
    <input type="hidden" name="orgWidth" id="orgWidth" /><input type="hidden" name="orgHeight" id="orgHeight" />
    
    <ui:Pane ID="pane_src" runat="server">
      <umb3:Image runat="server" ID="ImageViewer" ViewerStyle="ThumbnailPreview" />
      <ui:PropertyPanel id="pp_src" runat="server" Text="Url">
          <input type="text" id="src" style="width: 300px"/>
      </ui:PropertyPanel>
      <ui:PropertyPanel id="pp_title" runat="server" Text="Title">
           <input type="text" id="alt" style="width: 300px"/>
      </ui:PropertyPanel>
      <ui:PropertyPanel id="pp_dimensions" runat="server" Text="Dimensions">
            <asp:Literal ID="lt_widthLabel" runat="server" />:  <input name="width" type="text" id="width" value="" size="5" maxlength="5" class="size" onchange="changeDimensions('width');" />
            <asp:Literal ID="lt_heightLabel" runat="server" />:   <input name="height" type="text" id="height" value="" size="5" maxlength="5" class="size" onchange="changeDimensions('height');" />
           
      </ui:PropertyPanel>
    </ui:Pane>
    
    <br /> 
    
    <ui:TabView AutoResize="false" Width="555px" Height="305px" runat="server"  ID="tv_options" />
    <ui:Pane ID="pane_select" runat="server"> 
      
      <div style="padding: 5px; background: #fff; height: 250px;">

        <%--Manual initialization is set to true because the tree doesn't load properly in some browsers in this TinyMCE window--%>
        <umb2:Tree runat="server" ID="DialogTree" ManualInitialization="true"
            App="media" TreeType="media" IsDialog="true" 
            ShowContextMenu="false" 
            DialogMode="id" FunctionToCall="dialogHandler" />
            
      </div>
    </ui:Pane>
    <asp:Panel ID="pane_upload" runat="server">
        <umb4:MediaUpload runat="server" ID="MediaUploader" OnClientUpload="uploadHandler" />
    </asp:Panel>
    <br />
         
    <p>
      <input id="SubmitInsertImage" type="submit" value="{#insert}" style="width: 60px;" onclick="ImageDialog.insert();return false;" /> <em>or</em> <a href="#" onclick="tinyMCEPopup.close();">{#cancel}</a>
    </p>   
    
</form>

</body>
</html>
