<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ImageUploader.aspx.cs" Inherits="umbraco.presentation.umbraco.LiveEditing.Modules.SkinModule.ImageUploader" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>

    <style type="text/css">
    <!--
   
    #cropper {
	    cursor:move;
	    overflow:hidden;
	    width:<%= Request["w"] %>px;
	    height:<%= Request["h"] %>px;
	    clear:both;
	    border:1px solid #ccc;
	    background:#ccc;
        margin: 5px 0px 15px 5px;
    }
    -->
    </style>

    <cc1:UmbracoClientDependencyLoader runat="server" id="ClientLoader" />

    <umb:CssInclude ID="CssInclude1" runat="server" FilePath="ui/ui-lightness/jquery-ui.custom.css"  PathNameAlias="UmbracoClient" />
    <umb:CssInclude ID="CssInclude2" runat="server" FilePath="ui/default.css" PathNameAlias="UmbracoClient" />

    <umb:JsInclude ID="JsInclude1" runat="server" FilePath="ui/jquery.js" PathNameAlias="UmbracoClient"
        Priority="0" />
    <umb:JsInclude ID="JsInclude2" runat="server" FilePath="ui/jqueryui.js" PathNameAlias="UmbracoClient"
        Priority="1" />
    <umb:JsInclude ID="JsInclude3" runat="server" FilePath="mousewheel/jquery.mousewheel.js" PathNameAlias="UmbracoClient"
        Priority="2" />


    <script type="text/javascript">

        function setImage() {

            var val = $('#<%= Image.ClientID %>').val();
            top.jQuery('#<%= Request["ctrl"] %>').val(val);
            top.jQuery('#<%= Request["ctrl"] %>').trigger('change');
            closeModal();
        }

        function closeModal() {

            top.jQuery('.umbModalBoxIframe').closest(".umbModalBox").ModalWindowAPI().close();
            return false;
        }
    
    </script>

    <script type="text/javascript">

        var sliderChange = function (e, ui) {

            if (origwidht == 0) { 
                origwidht = $('#<%= Image1.ClientID %>').width(); 

            }
            if (origheight == 0) { 
                origheight = $('#<%= Image1.ClientID %>').height(); 
            }

            $('#cropper img').each(function (index, item) {
                var _new = $('#slider').slider("value");
                $('#<%= Scale.ClientID %>').val(_new);

                $(this).width(origwidht * (_new / 100));
                $(this).height(origheight * (_new / 100));
            });
        }

        var origheight = 0;
        var origwidht = 0;

        $(function () {

            if(<%= Request["w"] %> > <%= MaxWidth %> || <%= Request["h"] %> >  <%= MaxHeight %>)
            {               
                $("#cropper").css('width', <%= Request["w"] %> / 2);
                $("#cropper").css('height', <%= Request["h"] %> / 2);
            }


            $("#<%= Image1.ClientID %>").draggable({

                stop: function () {
                    $('#<%= X.ClientID %>').val($("#<%= Image1.ClientID %>").css('left').replace('px', ''));
                    $('#<%= Y.ClientID %>').val($("#<%= Image1.ClientID %>").css('top').replace('px', ''));
                }

            });

            $('#slider').slider({ change: sliderChange, slide: sliderChange, min: 5, max: 200, value: 100 });


            $('#cropper').mousewheel(function (event, delta, deltaX, deltaY) {

                var speed = 5;
                var mySlider = $("#slider");
                var sliderVal = mySlider.slider("option", "value");

                sliderVal += (delta * speed);

                if (sliderVal > mySlider.slider("option", "max")) sliderVal = mySlider.slider("option", "max");
                else if (sliderVal < mySlider.slider("option", "min")) sliderVal = mySlider.slider("option", "min");

                $("#slider").slider("value", sliderVal);


                return false;
            });
        });

        function ResetToDefault() {

            $("#slider").slider("value", 100);

            $('#cropper img').css('left','0px')
            $('#cropper img').css('top','0px');

            $('#<%= X.ClientID %>').val(0);
            $('#<%= Y.ClientID %>').val(0);
        }

    </script>
</head>
<body>
    <form id="form1" runat="server">

    <asp:HiddenField ID="Image" runat="server" />

    <asp:HiddenField ID="FileName" runat="server" />

    <asp:HiddenField ID="X" runat="server" Value="0"/>
    <asp:HiddenField ID="Y" runat="server" Value="0"/>
    <asp:HiddenField ID="Scale" runat="server" Value="100"/>

    <cc1:Feedback ID="fb_feedback1" runat="server" />

    <asp:PlaceHolder  ID="pnl_upload" runat="server">

    <cc1:Pane Text="Upload image file" runat="server">
    
    <cc1:PropertyPanel runat="server" Text="Select a image file <br/><small>jpg, gif and png files can be used</small>">
        <asp:FileUpload ID="FileUpload1" runat="server" />
    </cc1:PropertyPanel>
    </cc1:Pane>       
        <p style="margin-top: 20px;">    
            <asp:Button ID="bt_upload" runat="server" Text="Upload" onclick="bt_upload_Click" /> <em> or </em> <a href="#" onclick="closeModal();">Cancel</a>
        </p>     
    </asp:PlaceHolder>


    <asp:PlaceHolder ID="pnl_crop" runat="server" Visible="false">


    <cc1:Pane runat="server" Text="Crop and scale image">
    
    <cc1:PropertyPanel runat="server" Text="Crop <br /><small>Drag image with mouse to selct crop area</small>">
    <div id="cropper">
        <asp:Image ID="Image1" runat="server" />
    </div>
    </cc1:PropertyPanel>

    <cc1:PropertyPanel runat="server" Text="Scale <br /><small>Drag slider to choose size</small>" >
         <div id="slidercontainer" style="width: <%= scaleWidth %>">
	         <div id="slider"></div>
         </div>
    </cc1:PropertyPanel>

   
    
    </div>

    
    </cc1:Pane>

    <p style="margin-top: 20px;">
            <asp:Button ID="bt_crop" runat="server" Text="OK"  onclick="bt_crop_Click" /> <em> or </em> <a href="#" onclick="closeModal();">Cancel</a>
    </p>

    </asp:PlaceHolder>

    </form>
</body>
</html>
