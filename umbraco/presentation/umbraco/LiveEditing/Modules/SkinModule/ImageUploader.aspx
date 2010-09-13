<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ImageUploader.aspx.cs" Inherits="umbraco.presentation.umbraco.LiveEditing.Modules.SkinModule.ImageUploader" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>

    <style type="text/css">
    <!--
    
    #cropcontainer
    {
        margin-top: 20px;
        margin-bottom: 20px;
    }
    #cropper {
	    cursor:move;
	    overflow:hidden;
	    width:<%= Request["w"] %>px;
	    height:<%= Request["h"] %>px;
	    clear:both;
	    border:1px solid black;
	    background:black;
    }
    -->
    </style>

    <script type="text/javascript" src="../../../../umbraco_client/ui/jquery.js"></script>
    <script type="text/javascript" src="../../../../umbraco_client/ui/jqueryui.js"></script>
    <script type="text/javascript" src="../../../../umbraco_client/mousewheel/jquery.mousewheel.js"></script>


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

            if (origwidht == 0) { origwidht = $('#<%= Image1.ClientID %>').width(); }
            if (origheight == 0) { origheight = $('#<%= Image1.ClientID %>').height(); }

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



    </script>
</head>
<body>
    <form id="form1" runat="server">

    <asp:HiddenField ID="Image" runat="server" />

     <asp:HiddenField ID="FileName" runat="server" />

    <asp:HiddenField ID="X" runat="server" Value="0"/>
    <asp:HiddenField ID="Y" runat="server" Value="0"/>
    <asp:HiddenField ID="Scale" runat="server" Value="100"/>


    <asp:Panel ID="pnl_upload" runat="server">
   
    <h1>Upload image</h1>

    <asp:FileUpload ID="FileUpload1" runat="server" /> 
    <asp:Button ID="bt_upload" runat="server" Text="Upload" 
        onclick="bt_upload_Click" /><br />


     <br />

     <button type="button" onclick="closeModal();">Cancel</button>

     </asp:Panel>

    <asp:Panel ID="pnl_crop" runat="server" Visible="false">

    <h1>Crop image</h1>
    <div id="cropcontainer">

    <div id="cropper">
        <asp:Image ID="Image1" runat="server" />
    </div>

    <div id="slidercontainer" style="display:none;">
	     <div id="slider"></div>
    </div>

    </div>

    <asp:Button ID="bt_crop" runat="server" Text="Crop"  onclick="bt_crop_Click" />
     <button type="button" onclick="closeModal();">Cancel</button>

    </asp:Panel>

   
   

    </form>
</body>
</html>
