<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="insertImage.aspx.cs" Inherits="umbraco.presentation.plugins.tinymce3.insertImage" %>
<%@ Register TagPrefix="ui" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="umb" Namespace="umbraco.presentation.ClientDependency.Controls" Assembly="umbraco.presentation.ClientDependency" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>{#advimage_dlg.dialog_title}</title>

    <base target="_self" />

    <script type="text/javascript" language="javascript">
        function dialogHandler(id) {
            document.getElementById('imageViewer').src = '../../dialogs/imageViewer.aspx?id=' + id;
        }

        function refreshTree() {
          jQuery("#treeFrame").attr("src", jQuery("#treeFrame").attr("src"));
        }
        
        function updateImageSource(src, alt, width, height) {

            // maybe we'll need to remove the umbraco path from the src
            var umbracoPath = tinyMCE.activeEditor.getParam('umbraco_path');
            if (src.substring(0, umbracoPath.length) == umbracoPath) {
                // if the path contains a reference to the umbraco path, it also contains a reference to go up one level (../)
                src = src.substring(umbracoPath.length+3, src.length);
            }
        
            var formObj = document.forms[0];
            formObj.src.value = src;
                       
            jQuery("#previewImage").css("background-image", "url(" + src.substring(0, (src.length-4)) + "_thumb.jpg)");
            
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
            rBot.value = rBot.value.toFixed(0);
          }else if (mode == "height") {
            rBot.value = ((bBot.value * rTop.value) / bTop.value).toFixed(0);
            rTop.value = rTop.value.toFixed(0);
          }
        }    
    
        function doSubmit() { }
        
        var functionsFrame = this;
        var tabFrame = this;
        var isDialog = true;
        var submitOnEnter = true;
        var preloadImg = true;

        jQuery(document).ready(function() {
          if (document.forms[0].src.value != "") {
            var src = document.forms[0].src.value;
            jQuery("#previewImage").css("background-image", "url(" + src.substring(0, (src.length - 4)) + "_thumb.jpg)");
          }
        });
        
    </script>

</head>
<body id="advimage" style="display: none">

	<umb:ClientDependencyLoader runat="server" id="ClientLoader" EmbedType="Header" IsDebugMode="false" >
		<Paths>
			<umb:ClientDependencyPath Name="UmbracoClient" Path='<%#umbraco.GlobalSettings.ClientPath%>' />
			<umb:ClientDependencyPath Name="UmbracoRoot" Path='<%#umbraco.GlobalSettings.Path%>' />
		</Paths>		
	</umb:ClientDependencyLoader>
	
	<umb:JsInclude ID="JsInclude3" runat="server" FilePath="ui/jquery.js" PathNameAlias="UmbracoClient" Priority="0" />
	<umb:JsInclude ID="JsInclude8" runat="server" FilePath="ui/default.js" PathNameAlias="UmbracoClient" Priority="4" />
	<umb:JsInclude ID="JsInclude4" runat="server" FilePath="tinymce3/tiny_mce_popup.js" PathNameAlias="UmbracoClient" Priority="100" />
	<umb:JsInclude ID="JsInclude5" runat="server" FilePath="tinymce3/utils/mctabs.js" PathNameAlias="UmbracoClient" Priority="101" />
	<umb:JsInclude ID="JsInclude6" runat="server" FilePath="tinymce3/utils/form_utils.js" PathNameAlias="UmbracoClient" Priority="102" />
	<umb:JsInclude ID="JsInclude7" runat="server" FilePath="tinymce3/utils/validate.js" PathNameAlias="UmbracoClient" Priority="103" />

	<umb:JsInclude ID="JsInclude2" runat="server" FilePath="tinymce3/utils/editable_selects.js" PathNameAlias="UmbracoClient" Priority="110" />
	<umb:JsInclude ID="JsInclude1" runat="server" FilePath="tinymce3/plugins/umbracoimg/js/image.js" PathNameAlias="UmbracoClient" Priority="111" />

    <form runat="server" onsubmit="ImageDialog.insert();return false;" action="#">
    <input type="hidden" name="orgWidth" id="orgWidth" /><input type="hidden" name="orgHeight" id="orgHeight" />
    
    <ui:Pane ID="pane_src" runat="server">
      <div id="previewImage" style="width: 105px; height: 105px; background: #fff center center no-repeat; border: 1px solid #ccc; position: absolute; top: 3px; right: 3px;">
          &nbsp;
      </div>
      <ui:PropertyPanel id="pp_src" runat="server" Text="Url">
          <input type="text" id="src" style="width: 300px"/>
      </ui:PropertyPanel>
      <ui:PropertyPanel id="pp_title" runat="server" Text="Title">
           <input type="text" id="alt" style="width: 300px"/>
      </ui:PropertyPanel>
      <ui:PropertyPanel id="pp_dimensions" runat="server" Text="Dimensions">
            <asp:Literal ID="lt_widthLabel" runat="server" />:  <input name="width" type="text" id="width" value="" size="5" maxlength="5" class="size" onchange="changeDimensions('width');" />
            <asp:Literal ID="lt_heightLabel" runat="server" />:   <input name="height" type="text" id="height" value="" size="5" maxlength="5" class="size" onchange="changeDimensions('height');" />
            <br /> 
            <input id="constrain" type="checkbox" name="constrain" class="checkbox" /> <asp:Literal ID="lt_constrainLabel" runat="server" />
      </ui:PropertyPanel>
    </ui:Pane>
    
    <br /> 
    
    <ui:TabView AutoResize="false" Width="555px" Height="305px" runat="server"  ID="tv_options" />
    <ui:Pane ID="pane_select" runat="server"> 
      <div style="padding: 5px; background: #fff; height: 250px;">
        <iframe id="treeFrame" name="treeFrame" src="../../TreeInit.aspx?app=media&isDialog=true&dialogMode=id&contextMenu=false&functionToCall=parent.dialogHandler" style="width: 505px; height: 250px; float: left; border: none;" frameborder="0"></iframe>
        <iframe src="../../dialogs/imageViewer.aspx" id="imageViewer" style="width: 0px; height: 0px; visibility: hidden; float: right; border: none;" frameborder="0"></iframe>
      </div>
    </ui:Pane>
    <asp:Panel ID="pane_upload" runat="server">
        <iframe frameborder="0" src="../../dialogs/uploadImage.aspx" style="border: none; width: 535px; height: 250px;"></iframe>
    </asp:Panel>
    <br />
         
    <p>
      <input type="submit" value="{#insert}" style="width: 60px;" /> <em>or</em> <a href="#" onclick="tinyMCEPopup.close();">{#cancel}</a>
    </p>   
    
    
    
    
    
    <!--
    <div style="display: none !Important">
    <div class="tabs">
        <ul>
            <li id="general_tab" class="current"><span><a href="javascript:mcTabs.displayTab('general_tab','general_panel');"
                onmousedown="return false;">{#advimage_dlg.tab_general}</a></span></li>
            <li id="appearance_tab"><span><a href="javascript:mcTabs.displayTab('appearance_tab','appearance_panel');"
                onmousedown="return false;">{#advimage_dlg.tab_appearance}</a></span></li>
            <li id="advanced_tab"><span><a href="javascript:mcTabs.displayTab('advanced_tab','advanced_panel');"
                onmousedown="return false;">{#advimage_dlg.tab_advanced}</a></span></li>
        </ul>
    </div>
    <div class="panel_wrapper">
        <div id="general_panel" class="panel current" style="height: 480px;">
            <fieldset>
                <legend>
                   Choose</legend>
                <table class="propertyPane" cellspacing="0" cellpadding="4" width="98%" border="0"
                    runat="server" id="Table1">
                    <tbody>
                        <tr>
                            <td class="propertyHeader" width="30%">
                                <asp:PlaceHolder ID="PlaceHolder1" runat="server" />
                                <br style="clear: both" />
                            </td>
                        </tr>
                    </tbody>
                </table>
            </fieldset>
            <fieldset>
                <legend>{#advimage_dlg.general}</legend>
                <table class="properties">
                    <tr>
                        <td class="column1">
                            <label id="srclabel" for="src">
                                {#advimage_dlg.src}</label>
                        </td>
                        <td colspan="2">
                            <table border="0" cellspacing="0" cellpadding="0">
                                <tr>
                                    <td>
                                        <input name="src" type="text" id="src" value="" class="mceFocus" onchange="ImageDialog.showPreviewImage(this.value);" />
                                    </td>
                                    <td id="srcbrowsercontainer">
                                        &nbsp;
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <label for="src_list">
                                {#advimage_dlg.image_list}</label>
                        </td>
                        <td>
                            <select id="src_list" name="src_list" onchange="document.getElementById('src').value=this.options[this.selectedIndex].value;document.getElementById('alt').value=this.options[this.selectedIndex].text;document.getElementById('title').value=this.options[this.selectedIndex].text;ImageDialog.showPreviewImage(this.options[this.selectedIndex].value);">
                            </select>
                        </td>
                    </tr>
                    <tr>
                        <td class="column1">
                            <label id="altlabel" for="alt">
                                {#advimage_dlg.alt}</label>
                        </td>
                        <td colspan="2">
                            <input id="alt" name="alt" type="text" value="" />
                        </td>
                    </tr>
                    <tr>
                        <td class="column1">
                            <label id="titlelabel" for="title">
                                {#advimage_dlg.title}</label>
                        </td>
                        <td colspan="2">
                            <input id="title" name="title" type="text" value="" />
                        </td>
                    </tr>
                </table>
            </fieldset>
            <fieldset style="display: none;">
                <legend>{#advimage_dlg.preview}</legend>
                <div id="prev">
                </div>
            </fieldset>
        </div>
        <div id="appearance_panel" class="panel">
            <fieldset>
                <legend>{#advimage_dlg.tab_appearance}</legend>
                <table border="0" cellpadding="4" cellspacing="0">
                    <tr>
                        <td class="column1">
                            <label id="alignlabel" for="align">
                                {#advimage_dlg.align}</label>
                        </td>
                        <td>
                            <select id="align" name="align" onchange="ImageDialog.updateStyle('align');ImageDialog.changeAppearance();">
                                <option value="">{#not_set}</option>
                                <option value="baseline">{#advimage_dlg.align_baseline}</option>
                                <option value="top">{#advimage_dlg.align_top}</option>
                                <option value="middle">{#advimage_dlg.align_middle}</option>
                                <option value="bottom">{#advimage_dlg.align_bottom}</option>
                                <option value="text-top">{#advimage_dlg.align_texttop}</option>
                                <option value="text-bottom">{#advimage_dlg.align_textbottom}</option>
                                <option value="left">{#advimage_dlg.align_left}</option>
                                <option value="right">{#advimage_dlg.align_right}</option>
                            </select>
                        </td>
                        <td rowspan="6" valign="top">
                            <div class="alignPreview">
                                <img id="alignSampleImg" src="img/sample.gif" alt="{#advimage_dlg.example_img}" />
                                Lorem ipsum, Dolor sit amet, consectetuer adipiscing loreum ipsum edipiscing elit,
                                sed diam nonummy nibh euismod tincidunt ut laoreet dolore magna aliquam erat volutpat.Loreum
                                ipsum edipiscing elit, sed diam nonummy nibh euismod tincidunt ut laoreet dolore
                                magna aliquam erat volutpat.
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td class="column1">
                            <label id="widthlabel" for="width">
                                {#advimage_dlg.dimensions}</label>
                        </td>
                        <td nowrap="nowrap">
                            <input name="width" type="text" id="width" value="" size="5" maxlength="5" class="size"
                                onchange="ImageDialog.changeHeight();" />
                            x
                            <input name="height" type="text" id="height" value="" size="5" maxlength="5" class="size"
                                onchange="ImageDialog.changeWidth();" />
                            px
                        </td>
                    </tr>
                    <tr>
                        <td>
                            &nbsp;
                        </td>
                        <td>
                            <table border="0" cellpadding="0" cellspacing="0">
                                <tr>
                                    <td>
                                        <input id="constrain" type="checkbox" name="constrain" class="checkbox" />
                                    </td>
                                    <td>
                                        <label id="constrainlabel" for="constrain">
                                            {#advimage_dlg.constrain_proportions}</label>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td class="column1">
                            <label id="vspacelabel" for="vspace">
                                {#advimage_dlg.vspace}</label>
                        </td>
                        <td>
                            <input name="vspace" type="text" id="vspace" value="" size="3" maxlength="3" class="number"
                                onchange="ImageDialog.updateStyle('vspace');ImageDialog.changeAppearance();"
                                onblur="ImageDialog.updateStyle('vspace');ImageDialog.changeAppearance();" />
                        </td>
                    </tr>
                    <tr>
                        <td class="column1">
                            <label id="hspacelabel" for="hspace">
                                {#advimage_dlg.hspace}</label>
                        </td>
                        <td>
                            <input name="hspace" type="text" id="hspace" value="" size="3" maxlength="3" class="number"
                                onchange="ImageDialog.updateStyle('hspace');ImageDialog.changeAppearance();"
                                onblur="ImageDialog.updateStyle('hspace');ImageDialog.changeAppearance();" />
                        </td>
                    </tr>
                    <tr>
                        <td class="column1">
                            <label id="borderlabel" for="border">
                                {#advimage_dlg.border}</label>
                        </td>
                        <td>
                            <input id="border" name="border" type="text" value="" size="3" maxlength="3" class="number"
                                onchange="ImageDialog.updateStyle('border');ImageDialog.changeAppearance();"
                                onblur="ImageDialog.updateStyle('border');ImageDialog.changeAppearance();" />
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <label for="class_list">
                                {#class_name}</label>
                        </td>
                        <td colspan="2">
                            <select id="class_list" name="class_list" class="mceEditableSelect">
                            </select>
                        </td>
                    </tr>
                    <tr>
                        <td class="column1">
                            <label id="stylelabel" for="style">
                                {#advimage_dlg.style}</label>
                        </td>
                        <td colspan="2">
                            <input id="style" name="style" type="text" value="" onchange="ImageDialog.changeAppearance();" />
                        </td>
                    </tr>
                   
                </table>
            </fieldset>
        </div>
        <div id="advanced_panel" class="panel">
            <fieldset>
                <legend>{#advimage_dlg.swap_image}</legend>
                <input type="checkbox" id="onmousemovecheck" name="onmousemovecheck" class="checkbox"
                    onclick="ImageDialog.setSwapImage(this.checked);" />
                <label id="onmousemovechecklabel" for="onmousemovecheck">
                    {#advimage_dlg.alt_image}</label>
                <table border="0" cellpadding="4" cellspacing="0" width="100%">
                    <tr>
                        <td class="column1">
                            <label id="onmouseoversrclabel" for="onmouseoversrc">
                                {#advimage_dlg.mouseover}</label>
                        </td>
                        <td>
                            <table border="0" cellspacing="0" cellpadding="0">
                                <tr>
                                    <td>
                                        <input id="onmouseoversrc" name="onmouseoversrc" type="text" value="" />
                                    </td>
                                    <td id="onmouseoversrccontainer">
                                        &nbsp;
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <label for="over_list">
                                {#advimage_dlg.image_list}</label>
                        </td>
                        <td>
                            <select id="over_list" name="over_list" onchange="document.getElementById('onmouseoversrc').value=this.options[this.selectedIndex].value;">
                            </select>
                        </td>
                    </tr>
                    <tr>
                        <td class="column1">
                            <label id="onmouseoutsrclabel" for="onmouseoutsrc">
                                {#advimage_dlg.mouseout}</label>
                        </td>
                        <td class="column2">
                            <table border="0" cellspacing="0" cellpadding="0">
                                <tr>
                                    <td>
                                        <input id="onmouseoutsrc" name="onmouseoutsrc" type="text" value="" />
                                    </td>
                                    <td id="onmouseoutsrccontainer">
                                        &nbsp;
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <label for="out_list">
                                {#advimage_dlg.image_list}</label>
                        </td>
                        <td>
                            <select id="out_list" name="out_list" onchange="document.getElementById('onmouseoutsrc').value=this.options[this.selectedIndex].value;">
                            </select>
                        </td>
                    </tr>
                </table>
            </fieldset>
            <fieldset>
                <legend>{#advimage_dlg.misc}</legend>
                <table border="0" cellpadding="4" cellspacing="0">
                    <tr>
                        <td class="column1">
                            <label id="idlabel" for="id">
                                {#advimage_dlg.id}</label>
                        </td>
                        <td>
                            <input id="id" name="id" type="text" value="" />
                        </td>
                    </tr>
                    <tr>
                        <td class="column1">
                            <label id="dirlabel" for="dir">
                                {#advimage_dlg.langdir}</label>
                        </td>
                        <td>
                            <select id="dir" name="dir" onchange="ImageDialog.changeAppearance();">
                                <option value="">{#not_set}</option>
                                <option value="ltr">{#advimage_dlg.ltr}</option>
                                <option value="rtl">{#advimage_dlg.rtl}</option>
                            </select>
                        </td>
                    </tr>
                    <tr>
                        <td class="column1">
                            <label id="langlabel" for="lang">
                                {#advimage_dlg.langcode}</label>
                        </td>
                        <td>
                            <input id="lang" name="lang" type="text" value="" />
                        </td>
                    </tr>
                    <tr>
                        <td class="column1">
                            <label id="usemaplabel" for="usemap">
                                {#advimage_dlg.map}</label>
                        </td>
                        <td>
                            <input id="usemap" name="usemap" type="text" value="" />
                        </td>
                    </tr>
                    <tr>
                        <td class="column1">
                            <label id="longdesclabel" for="longdesc">
                                {#advimage_dlg.long_desc}</label>
                        </td>
                        <td>
                            <table border="0" cellspacing="0" cellpadding="0">
                                <tr>
                                    <td>
                                        <input id="longdesc" name="longdesc" type="text" value="" />
                                    </td>
                                    <td id="longdesccontainer">
                                        &nbsp;
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </fieldset>
        </div>
    </div>
    <div class="mceActionPanel">
        <div style="float: left">
            <input type="submit" id="insert" name="insert" value="{#insert}" />
        </div>
        <div style="float: right">
            <input type="button" id="cancel" name="cancel" value="{#cancel}" onclick="tinyMCEPopup.close();" />
        </div>
    </div>

</div>    
    
    

    <script>
        if (tinyMCE.activeEditor.getParam('umbraco_advancedMode') != true) {
            document.getElementById('appearance_tab').style.visibility = 'hidden';
            document.getElementById('advanced_tab').style.visibility = 'hidden';
        }
    </script>
-->

</form>

</body>
</html>
