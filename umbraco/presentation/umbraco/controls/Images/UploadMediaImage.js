/// <reference path="/umbraco_client/Application/NamespaceManager.js" />

Umbraco.Sys.registerNamespace("Umbraco.Controls");

(function($) {
    Umbraco.Controls.UploadMediaImage = function(txtBoxTitleID, btnID, uploadFileID) {
        return {
            _txtBoxTitleID: txtBoxTitleID,
            _btnID: btnID,
            _uplaodFileID: uploadFileID,
            
            validateImage: function() {
                // Disable save button
                var imageTypes = ",jpeg,jpg,gif,bmp,png,tiff,tif,";
                var tb_title = document.getElementById(this._txtBoxTitleID);
                var bt_submit = $("#" + this._btnID);
                var tb_image = document.getElementById(this._uplaodFileID);

                bt_submit.attr("disabled","disabled").css("color", "gray");

                var imageName = tb_image.value;
                if (imageName.length > 0) {
                    var extension = imageName.substring(imageName.lastIndexOf(".") + 1, imageName.length);
                    if (imageTypes.indexOf(',' + extension.toLowerCase() + ',') > -1) {
                        bt_submit.removeAttr("disabled").css("color", "#000");
                        if (tb_title.value == "")
                            tb_title.value = imageName.substring(imageName.lastIndexOf("\\") + 1, imageName.length).replace("." + extension, "");
                    }
                }
            }
        };
    }
})(jQuery);